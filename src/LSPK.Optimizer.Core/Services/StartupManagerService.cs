using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Security;
using Serilog;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Implementação do serviço de gerenciamento de programas de inicialização.
/// Realiza leitura do registro, pastas de inicialização, parsing de comandos e backups reversíveis.
/// </summary>
public class StartupManagerService : IStartupManagerService
{
    private static readonly ILogger Logger = Log.ForContext<StartupManagerService>();

    /// <summary>Chave de registro própria da LSPK para arquivamento de backups de entradas desativadas.</summary>
    private const string BackupRegistryKeyPath = @"Software\LSPK\Optimizer\StartupBackup";

    /// <summary>Diretório local para armazenar atalhos (.lnk) desativados da pasta Startup.</summary>
    private static string ShortcutBackupDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LSPK Optimizer",
        "Backup",
        "StartupDisabled"
    );

    /// <summary>
    /// Audita todas as origens obrigatórias do Windows e compila a lista de itens ativos e desativados.
    /// </summary>
    public async Task<List<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var entries = new List<StartupEntry>();

            if (!OperatingSystem.IsWindows())
            {
                Logger.Warning("Sistema operacional não é Windows; retornando lista simulada de inicializadores para testes.");
                return entries;
            }

            try
            {
                // 1. Registro do Usuário (HKCU\...\Run)
                ReadRegistryKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", StartupSource.HkcuRun, entries);

                // 2. Registro Global da Máquina (HKLM\...\Run)
                ReadRegistryKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", StartupSource.HklmRun, entries);

                // 3. Registro Global para aplicativos 32-bit em Windows 64-bit (WoW6432Node)
                if (Environment.Is64BitOperatingSystem)
                {
                    ReadRegistryKey(Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", StartupSource.HklmRun64, entries);
                }

                // 4. Pastas Inicializar do Menu Iniciar (Usuário e Global)
                ReadStartupFolder(Environment.SpecialFolder.Startup, StartupSource.UserStartupFolder, entries);
                ReadStartupFolder(Environment.SpecialFolder.CommonStartup, StartupSource.CommonStartupFolder, entries);

                // 5. Itens desativados previamente salvos no repositório de backup da LSPK
                ReadBackupEntries(entries);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Erro ao listar itens de inicialização");
            }

            Logger.Information("Total de entradas de inicialização identificadas: {Count}", entries.Count);
            return entries;
        }, cancellationToken);
    }

    /// <summary>
    /// Lê valores gravados em uma chave específica do Registro do Windows.
    /// Extrai caminhos, metadados de fabricante e verifica a existência do executável.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ReadRegistryKey(RegistryKey rootKey, string subKeyPath, StartupSource source, List<StartupEntry> entries)
    {
        try
        {
            using var key = rootKey.OpenSubKey(subKeyPath, false);
            if (key == null) return;

            foreach (var valueName in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(valueName)) continue;
                var rawValue = key.GetValue(valueName)?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawValue)) continue;

                // Separa o executável dos parâmetros de linha de comando
                var (exePath, args) = ParseCommandLine(rawValue);
                bool exists = !string.IsNullOrEmpty(exePath) && File.Exists(exePath);
                string publisher = ResolvePublisher(exePath);
                bool isProtected = SystemWhitelist.IsProtected(valueName, exePath, publisher);

                entries.Add(new StartupEntry
                {
                    Name = valueName,
                    Publisher = publisher,
                    RawCommand = rawValue,
                    ExecutablePath = exePath,
                    Arguments = args,
                    Source = source,
                    IsEnabled = true,
                    TargetExists = exists,
                    IsSystemProtected = isProtected,
                    Impact = EstimateImpact(exePath, exists)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Falha ao ler chave de registro: {SubKey}", subKeyPath);
        }
    }

    /// <summary>
    /// Lê arquivos e atalhos (.lnk) presentes nas pastas especiais de Inicialização do Windows.
    /// </summary>
    private static void ReadStartupFolder(Environment.SpecialFolder folder, StartupSource source, List<StartupEntry> entries)
    {
        try
        {
            string folderPath = Environment.GetFolderPath(folder);
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

            foreach (var file in Directory.EnumerateFiles(folderPath, "*.*"))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".lnk" && ext != ".exe" && ext != ".bat" && ext != ".cmd") continue;

                string name = Path.GetFileNameWithoutExtension(file);
                string targetPath = file;
                string args = string.Empty;

                bool exists = File.Exists(targetPath);
                string publisher = ResolvePublisher(targetPath);
                bool isProtected = SystemWhitelist.IsProtected(name, targetPath, publisher);

                entries.Add(new StartupEntry
                {
                    Name = name,
                    Publisher = publisher,
                    RawCommand = file,
                    ExecutablePath = targetPath,
                    Arguments = args,
                    Source = source,
                    IsEnabled = true,
                    TargetExists = exists,
                    IsSystemProtected = isProtected,
                    Impact = EstimateImpact(targetPath, exists)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Falha ao ler pasta startup: {Folder}", folder);
        }
    }

    /// <summary>
    /// Lê as entradas que foram anteriormente desativadas pelo aplicativo para exibi-las na lista
    /// como itens desabilitados (permitindo que o usuário as reative com facilidade).
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ReadBackupEntries(List<StartupEntry> entries)
    {
        try
        {
            using var backupKey = Registry.CurrentUser.OpenSubKey(BackupRegistryKeyPath, false);
            if (backupKey != null)
            {
                foreach (var name in backupKey.GetValueNames())
                {
                    var rawValue = backupKey.GetValue(name)?.ToString() ?? string.Empty;
                    var (exePath, args) = ParseCommandLine(rawValue);
                    bool exists = !string.IsNullOrEmpty(exePath) && File.Exists(exePath);

                    // Evita duplicatas se o item já foi listado como ativo
                    if (!entries.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        entries.Add(new StartupEntry
                        {
                            Name = name,
                            Publisher = ResolvePublisher(exePath),
                            RawCommand = rawValue,
                            ExecutablePath = exePath,
                            Arguments = args,
                            Source = StartupSource.HkcuRun,
                            IsEnabled = false, // Marcado como Desativado no backup
                            TargetExists = exists,
                            IsSystemProtected = false,
                            Impact = StartupImpact.Low
                        });
                    }
                }
            }

            // Atalhos desativados salvos na pasta de backup da LSPK
            if (Directory.Exists(ShortcutBackupDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(ShortcutBackupDirectory, "*.*"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (!entries.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        entries.Add(new StartupEntry
                        {
                            Name = name,
                            Publisher = ResolvePublisher(file),
                            RawCommand = file,
                            ExecutablePath = file,
                            Source = StartupSource.UserStartupFolder,
                            IsEnabled = false,
                            TargetExists = true,
                            IsSystemProtected = false,
                            Impact = StartupImpact.Low
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Falha ao ler backups de inicialização");
        }
    }

    /// <summary>
    /// Desativa uma entrada de inicialização movendo seu valor para a chave de backup da LSPK.
    /// Garante que o antivírus e processos essenciais não sejam desativados.
    /// </summary>
    public async Task<bool> DisableEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (entry.IsSystemProtected)
            {
                Logger.Warning("Bloqueada tentativa de desativar componente crítico do sistema: {Name}", entry.Name);
                return false;
            }

            if (!OperatingSystem.IsWindows()) return true;

            try
            {
                Logger.Information("Desativando com segurança a entrada de inicialização: {Name}", entry.Name);

                // 1. Tratamento para entradas gravadas no Registro do Windows
                if (entry.Source == StartupSource.HkcuRun || entry.Source == StartupSource.HklmRun || entry.Source == StartupSource.HklmRun64)
                {
                    // Guarda o comando na chave de backup próprio da LSPK
                    using (var backupKey = Registry.CurrentUser.CreateSubKey(BackupRegistryKeyPath))
                    {
                        backupKey.SetValue(entry.Name, entry.RawCommand, RegistryValueKind.String);
                    }

                    // Remove da chave de inicialização ativa original
                    var rootKey = (entry.Source == StartupSource.HkcuRun) ? Registry.CurrentUser : Registry.LocalMachine;
                    string subPath = entry.Source == StartupSource.HklmRun64
                        ? @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
                        : @"Software\Microsoft\Windows\CurrentVersion\Run";

                    using (var runKey = rootKey.OpenSubKey(subPath, true))
                    {
                        runKey?.DeleteValue(entry.Name, false);
                    }

                    entry.IsEnabled = false;
                    return true;
                }

                // 2. Tratamento para arquivos na pasta Inicializar (move o atalho para a pasta de backup)
                if (entry.Source == StartupSource.UserStartupFolder || entry.Source == StartupSource.CommonStartupFolder)
                {
                    if (File.Exists(entry.RawCommand))
                    {
                        Directory.CreateDirectory(ShortcutBackupDirectory);
                        string destPath = Path.Combine(ShortcutBackupDirectory, Path.GetFileName(entry.RawCommand));
                        File.Move(entry.RawCommand, destPath, true);
                        entry.IsEnabled = false;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Erro ao desativar entrada {Name}", entry.Name);
            }

            return false;
        }, cancellationToken);
    }

    /// <summary>
    /// Reativa uma entrada previamente desativada, recuperando os dados do backup e gravando de volta no registro/pasta.
    /// </summary>
    public async Task<bool> EnableEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (!OperatingSystem.IsWindows()) return true;

            try
            {
                Logger.Information("Reativando entrada de inicialização previamente desativada: {Name}", entry.Name);

                // 1. Restaura entradas que estavam guardadas no backup do Registro
                using (var backupKey = Registry.CurrentUser.OpenSubKey(BackupRegistryKeyPath, true))
                {
                    var savedCmd = backupKey?.GetValue(entry.Name)?.ToString();
                    if (!string.IsNullOrEmpty(savedCmd))
                    {
                        // Regrava no HKCU Run
                        using (var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            runKey?.SetValue(entry.Name, savedCmd, RegistryValueKind.String);
                        }

                        // Remove do backup
                        backupKey?.DeleteValue(entry.Name, false);
                        entry.IsEnabled = true;
                        return true;
                    }
                }

                // 2. Restaura atalhos da pasta de backup para a pasta de Inicialização
                if (Directory.Exists(ShortcutBackupDirectory))
                {
                    string backupFile = Path.Combine(ShortcutBackupDirectory, Path.GetFileName(entry.RawCommand));
                    if (File.Exists(backupFile))
                    {
                        string targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        string restorePath = Path.Combine(targetDir, Path.GetFileName(backupFile));
                        File.Move(backupFile, restorePath, true);
                        entry.IsEnabled = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Erro ao reativar entrada {Name}", entry.Name);
            }

            return false;
        }, cancellationToken);
    }

    /// <summary>
    /// Remove resíduos órfãos do registro deixados por programas que já foram desinstalados do PC.
    /// </summary>
    public async Task<bool> RemoveOrphanedEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (!entry.IsOrphaned) return false;
            if (!OperatingSystem.IsWindows()) return true;

            try
            {
                Logger.Information("Removendo resquício de entrada órfã inexistente: {Name}", entry.Name);

                var rootKey = (entry.Source == StartupSource.HkcuRun) ? Registry.CurrentUser : Registry.LocalMachine;
                string subPath = entry.Source == StartupSource.HklmRun64
                    ? @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
                    : @"Software\Microsoft\Windows\CurrentVersion\Run";

                using var runKey = rootKey.OpenSubKey(subPath, true);
                runKey?.DeleteValue(entry.Name, false);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Erro ao remover entrada órfã {Name}", entry.Name);
                return false;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Realiza o parsing de uma linha de comando para isolar o executável dos parâmetros.
    /// Suporta comandos encapsulados entre aspas duplas ou separados por espaço.
    /// </summary>
    /// <param name="rawCommand">Linha de comando bruta lida do registro.</param>
    /// <returns>Tupla contendo o caminho do executável limpo e a string de argumentos adicionais.</returns>
    public static (string Executable, string Arguments) ParseCommandLine(string rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand)) return (string.Empty, string.Empty);

        rawCommand = rawCommand.Trim();

        // Caso 1: Comando iniciado com aspas (ex: "C:\Program Files\App\app.exe" /arg1)
        if (rawCommand.StartsWith("\""))
        {
            int closingQuote = rawCommand.IndexOf('"', 1);
            if (closingQuote > 1)
            {
                string exe = rawCommand.Substring(1, closingQuote - 1).Trim();
                string args = rawCommand.Substring(closingQuote + 1).Trim();
                return (exe, args);
            }
        }

        // Caso 2: Comando sem aspas, verificando se o primeiro token até o espaço é um arquivo existente
        int firstSpace = rawCommand.IndexOf(' ');
        if (firstSpace > 0)
        {
            string candidate = rawCommand.Substring(0, firstSpace);
            if (File.Exists(candidate))
            {
                return (candidate, rawCommand.Substring(firstSpace + 1).Trim());
            }
        }

        return (rawCommand, string.Empty);
    }

    /// <summary>
    /// Lê os metadados oficiais do executável (FileVersionInfo) para identificar a empresa desenvolvedora.
    /// </summary>
    private static string ResolvePublisher(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
        {
            return "Desconhecido";
        }

        try
        {
            var info = FileVersionInfo.GetVersionInfo(executablePath);
            if (!string.IsNullOrWhiteSpace(info.CompanyName))
            {
                return info.CompanyName;
            }
        }
        catch
        {
            // Falha silenciosa de leitura de cabeçalho PE
        }

        return "Desconhecido";
    }

    /// <summary>
    /// Estima o impacto do aplicativo no tempo de boot com base em assinaturas de launchers pesados conhecidos.
    /// </summary>
    private static StartupImpact EstimateImpact(string executablePath, bool exists)
    {
        if (!exists) return StartupImpact.Low;

        string name = Path.GetFileName(executablePath).ToLowerInvariant();
        if (name.Contains("steam") || name.Contains("epic") || name.Contains("discord") || name.Contains("spotify") || name.Contains("teams"))
        {
            return StartupImpact.High;
        }

        return StartupImpact.Medium;
    }
}
