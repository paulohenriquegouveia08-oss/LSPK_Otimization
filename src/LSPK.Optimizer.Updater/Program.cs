using System.Diagnostics;
using System.IO.Compression;

namespace LSPK.Optimizer.Updater;

/// <summary>
/// Executável utilitário desacoplado de atualização atômica e recuperação contra falhas (Rollback).
/// Executa em processo independente para permitir a substituição completa dos binários do LSPK.Optimizer.App.exe.
/// </summary>
public static class Program
{
    /// <summary>
    /// Ponto de entrada do atualizador.
    /// Argumentos esperados:
    /// --pid {ProcessIdDoAppPrincipal}
    /// --package {CaminhoDoArquivoZipBaixado}
    /// --target {DiretorioDeInstalacaoDoApp}
    /// </summary>
    public static void Main(string[] args)
    {
        int parentPid = 0;
        string packagePath = string.Empty;
        string targetDir = string.Empty;

        // 1. Parsing dos argumentos passados pelo aplicativo principal
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--pid", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                int.TryParse(args[++i], out parentPid);
            }
            else if (args[i].Equals("--package", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                packagePath = args[++i];
            }
            else if (args[i].Equals("--target", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                targetDir = args[++i];
            }
        }

        // Se o diretório alvo não foi informado, utiliza a pasta atual do executável
        if (string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(targetDir))
        {
            targetDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        // 2. Aguarda a finalização pacífica do processo pai (para liberar locks de arquivos executáveis)
        if (parentPid > 0)
        {
            try
            {
                var parentProc = Process.GetProcessById(parentPid);
                parentProc.WaitForExit(15000); // Aguarda até 15 segundos
            }
            catch
            {
                // O processo pai já havia encerrado
            }
        }

        Thread.Sleep(500); // Pausa de segurança para garantir a liberação de handles no sistema operacional

        string backupDir = Path.Combine(Path.GetTempPath(), $"LSPK_Backup_{Guid.NewGuid():N}");
        bool updateSucceeded = false;

        try
        {
            // 3. Criação de Backup temporário da versão atual para permitir Rollback imediato
            Directory.CreateDirectory(backupDir);
            foreach (var file in Directory.GetFiles(targetDir, "*.*", SearchOption.AllDirectories))
            {
                // Ignora o próprio executável do Updater em execução para não travar
                if (file.Contains("LSPK.Optimizer.Updater.exe", StringComparison.OrdinalIgnoreCase)) continue;

                string rel = Path.GetRelativePath(targetDir, file);
                string dest = Path.Combine(backupDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, true);
            }

            // 4. Extração e substituição dos arquivos com o pacote de atualização baixado
            if (File.Exists(packagePath))
            {
                using var archive = ZipFile.OpenRead(packagePath);
                foreach (var entry in archive.Entries)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(targetDir, entry.FullName));
                    // Prevenção de Zip Slip
                    if (!destinationPath.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase)) continue;

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                    entry.ExtractToFile(destinationPath, true);
                }

                updateSucceeded = true;
            }
        }
        catch (Exception)
        {
            // 5. ROLLBACK AUTOMÁTICO: Em caso de qualquer falha na substituição, restaura o backup anterior intacto
            try
            {
                if (Directory.Exists(backupDir))
                {
                    foreach (var bakFile in Directory.GetFiles(backupDir, "*.*", SearchOption.AllDirectories))
                    {
                        string rel = Path.GetRelativePath(backupDir, bakFile);
                        string dest = Path.Combine(targetDir, rel);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                        File.Copy(bakFile, dest, true);
                    }
                }
            }
            catch { /* Restauração emergencial best-effort */ }
        }
        finally
        {
            // Limpa o diretório temporário de backup
            try { if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true); } catch { }
        }

        // 6. Reinicia o aplicativo principal atualizado
        string mainApp = Path.Combine(targetDir, "LSPK.Optimizer.App.exe");
        if (File.Exists(mainApp))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mainApp,
                Arguments = updateSucceeded ? "--updated" : "--rollback",
                UseShellExecute = true
            });
        }
    }
}
