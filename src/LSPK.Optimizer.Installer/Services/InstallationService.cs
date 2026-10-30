using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LSPK.Optimizer.Installer.Services;

/// <summary>
/// Estrutura de dados contendo o progresso real da instalação em bytes físicos e contagem de arquivos.
/// Utilizada para alimentar a barra de progresso do instalador sem simulações temporais.
/// </summary>
/// <param name="CurrentFile">Nome do arquivo sendo descompactado ou copiado no momento.</param>
/// <param name="FilesExtracted">Quantidade de arquivos já gravados com sucesso.</param>
/// <param name="TotalFiles">Total de arquivos que compõem o pacote da aplicação.</param>
/// <param name="BytesWritten">Volume físico de bytes gravados no disco até o momento.</param>
/// <param name="TotalBytes">Volume total de bytes do pacote a ser instalado.</param>
/// <param name="StatusDescription">Texto explicativo da operação atual.</param>
public record InstallProgress(
    string CurrentFile,
    int FilesExtracted,
    int TotalFiles,
    long BytesWritten,
    long TotalBytes,
    string StatusDescription
)
{
    /// <summary>Calcula o percentual verdadeiro concluído com base nos bytes gravados (0 a 100%).</summary>
    public double Percent => TotalBytes > 0 ? ((double)BytesWritten / TotalBytes) * 100.0 : 0;
}

/// <summary>
/// Serviço responsável pelo processo de instalação standalone e desinstalação limpa do LSPK Optimizer.
/// Instala no escopo por usuário (%LocalAppData%\Programs\LSPK Optimizer) dispensando elevação UAC.
/// </summary>
public class InstallationService
{
    /// <summary>Caminho padrão de instalação: %LocalAppData%\Programs\LSPK Optimizer.</summary>
    public static string DefaultInstallPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs",
        "LSPK Optimizer"
    );

    /// <summary>Chave de desinstalação registrada no Windows Add/Remove Programs (escopo de usuário HKCU).</summary>
    private const string UninstallRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\LSPKOptimizer";

    /// <summary>Armazena a última mensagem de erro ocorrida durante a instalação para exibição na UI.</summary>
    public static string? LastError { get; private set; }

    /// <summary>
    /// Converte um valor numérico em bytes para uma representação legível (B, KB, MB, GB, TB).
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    /// <summary>
    /// Executa o fluxo de instalação assíncrono: extração do payload com progresso real,
    /// criação de atalhos e registro do desinstalador no Windows.
    /// </summary>
    public async Task<bool> InstallAsync(
        string targetDirectory,
        bool createDesktopShortcut,
        IProgress<InstallProgress>? progress = null,
        CancellationToken ct = default)
    {
        LastError = null;
        return await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(targetDirectory);

                // 1. Tenta carregar o payload zip embutido nos recursos do executável ou arquivo local
                using var payloadStream = GetPayloadStream();
                if (payloadStream == null)
                {
                    // Caso não haja payload embutido, verifica se os executáveis estão disponíveis localmente
                    string localAppExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LSPK.Optimizer.App.exe");
                    if (File.Exists(localAppExe))
                    {
                        CopyLocalFilesWithProgress(AppDomain.CurrentDomain.BaseDirectory, targetDirectory, progress, ct);
                    }
                    else
                    {
                        throw new FileNotFoundException("Arquivo de dados da instalação (payload.zip) não encontrado no pacote do instalador.");
                    }
                }
                else
                {
                    // Extração de streaming com contagem real de bytes
                    ExtractZipWithRealProgress(payloadStream, targetDirectory, progress, ct);
                }

                ct.ThrowIfCancellationRequested();

                // 2. Copia o próprio executável do instalador para o diretório de destino como Uninstall.exe
                string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                string uninstallerPath = Path.Combine(targetDirectory, "Uninstall.exe");
                if (File.Exists(currentExe) && !string.Equals(currentExe, uninstallerPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(currentExe, uninstallerPath, true);
                }

                // 3. Cria atalhos no Menu Iniciar e opcionalmente na Área de Trabalho
                string mainAppExe = Path.Combine(targetDirectory, "LSPK.Optimizer.App.exe");
                CreateShortcuts(mainAppExe, targetDirectory, createDesktopShortcut);

                // 4. Registra o desinstalador nas Configurações de Aplicativos do Windows
                RegisterUninstaller(targetDirectory, uninstallerPath, mainAppExe);

                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }, ct);
    }

    /// <summary>
    /// Obtém o Stream do arquivo payload.zip (embutido como recurso no assembly ou ao lado do instalador).
    /// </summary>
    private static Stream? GetPayloadStream()
    {
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith("payload.zip", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(resourceName))
        {
            return asm.GetManifestResourceStream(resourceName);
        }

        string localZip = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payload.zip");
        if (File.Exists(localZip))
        {
            return File.OpenRead(localZip);
        }

        return null;
    }

    /// <summary>
    /// Extrai o arquivo ZIP utilizando buffers de 64 KB, calculando e reportando o progresso
    /// verdadeiro em bytes gravados para atualizar a barra de progresso da UI.
    /// </summary>
    private static void ExtractZipWithRealProgress(
        Stream zipStream,
        string targetDir,
        IProgress<InstallProgress>? progress,
        CancellationToken ct)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        long totalBytes = archive.Entries.Sum(e => e.Length);
        int totalFiles = archive.Entries.Count;
        long writtenBytes = 0;
        int fileCount = 0;

        byte[] buffer = new byte[65536]; // Buffer de gravação física de 64 KB

        foreach (var entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();

            // Prevenção contra vulnerabilidade de Zip Slip (traversal de diretório malicioso)
            string destinationPath = Path.GetFullPath(Path.Combine(targetDir, entry.FullName));
            if (!destinationPath.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            using var entryStream = entry.Open();
            using var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length);

            int read;
            while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ct.ThrowIfCancellationRequested();
                outStream.Write(buffer, 0, read);
                writtenBytes += read;

                // Emite evento com contagem real de bytes gravados
                progress?.Report(new InstallProgress(
                    CurrentFile: entry.Name,
                    FilesExtracted: fileCount,
                    TotalFiles: totalFiles,
                    BytesWritten: writtenBytes,
                    TotalBytes: totalBytes,
                    StatusDescription: $"Extraindo {entry.Name}..."
                ));
            }

            fileCount++;
        }
    }

    /// <summary>
    /// Modo de instalação direta a partir de arquivos locais de compilação.
    /// </summary>
    private static void CopyLocalFilesWithProgress(
        string sourceDir,
        string targetDir,
        IProgress<InstallProgress>? progress,
        CancellationToken ct)
    {
        var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("LSPKOptimizerSetup.exe", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        long totalBytes = files.Sum(f => new FileInfo(f).Length);
        long writtenBytes = 0;
        int count = 0;
        byte[] buffer = new byte[65536];

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            string relative = Path.GetRelativePath(sourceDir, file);
            string dest = Path.Combine(targetDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

            using var inStream = File.OpenRead(file);
            using var outStream = File.Create(dest);

            int read;
            while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ct.ThrowIfCancellationRequested();
                outStream.Write(buffer, 0, read);
                writtenBytes += read;

                progress?.Report(new InstallProgress(
                    CurrentFile: Path.GetFileName(file),
                    FilesExtracted: count,
                    TotalFiles: files.Length,
                    BytesWritten: writtenBytes,
                    TotalBytes: totalBytes,
                    StatusDescription: $"Instalando {Path.GetFileName(file)}..."
                ));
            }

            count++;
        }
    }

    /// <summary>
    /// Cria atalhos (.lnk) para o executável principal utilizando o objeto COM nativo WScript.Shell.
    /// </summary>
    private static void CreateShortcuts(string targetExe, string workingDir, bool createDesktop)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            // Atalho no Menu Iniciar do Windows
            string startMenuDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "LSPK Technology"
            );
            Directory.CreateDirectory(startMenuDir);
            string startMenuLink = Path.Combine(startMenuDir, "LSPK Optimizer.lnk");
            CreateShortcutFile(startMenuLink, targetExe, workingDir, "Otimizador Profissional do Windows");

            // Atalho na Área de Trabalho (se marcado pelo usuário)
            if (createDesktop)
            {
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string desktopLink = Path.Combine(desktopDir, "LSPK Optimizer.lnk");
                CreateShortcutFile(desktopLink, targetExe, workingDir, "Otimizador Profissional do Windows");
            }
        }
        catch { /* Fallback não crítico */ }
    }

    /// <summary>
    /// Invoca a automação COM do WScript.Shell para salvar fisicamente o atalho .lnk.
    /// </summary>
    private static void CreateShortcutFile(string shortcutPath, string targetPath, string workingDir, string description)
    {
        try
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDir;
            shortcut.Description = description;
            shortcut.Save();
        }
        catch { }
    }

    /// <summary>
    /// Registra a aplicação na chave de Desinstalação do Registro do Windows (HKCU),
    /// fazendo com que apareça em "Aplicativos e Recursos" no Painel de Controle do Windows.
    /// </summary>
    private static void RegisterUninstaller(string installDir, string uninstallerExe, string displayIcon)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(UninstallRegistryKey);
            key.SetValue("DisplayName", "LSPK Optimizer");
            key.SetValue("DisplayVersion", "1.0.0");
            key.SetValue("Publisher", "LSPK Technology");
            key.SetValue("DisplayIcon", displayIcon);
            key.SetValue("UninstallString", $"\"{uninstallerExe}\" --uninstall");
            key.SetValue("InstallLocation", installDir);
            key.SetValue("NoModify", 1, RegistryValueKind.DWord);
            key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        }
        catch { }
    }

    /// <summary>
    /// Executa a desinstalação completa: remoção de atalhos, exclusão da chave de registro
    /// e deleção dos binários da pasta %LocalAppData%\Programs\LSPK Optimizer.
    /// </summary>
    public static async Task<bool> UninstallAsync()
    {
        return await Task.Run(() =>
        {
            if (!OperatingSystem.IsWindows()) return true;

            try
            {
                // 1. Remove os atalhos criados no Menu Iniciar e Desktop
                string startMenuDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                    "LSPK Technology"
                );
                if (Directory.Exists(startMenuDir))
                {
                    Directory.Delete(startMenuDir, true);
                }

                string desktopLink = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "LSPK Optimizer.lnk"
                );
                if (File.Exists(desktopLink)) File.Delete(desktopLink);

                // 2. Remove o registro de desinstalação
                Registry.CurrentUser.DeleteSubKeyTree(UninstallRegistryKey, false);

                // 3. Remove as chaves de configuração e backups da LSPK
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\LSPK\Optimizer", false);

                // 4. Agenda remoção do diretório de instalação via comando CMD assíncrono pós-encerramento
                string installDir = DefaultInstallPath;
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout /t 2 & rmdir /s /q \"{installDir}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                return true;
            }
            catch
            {
                return false;
            }
        });
    }
}
