using System.Runtime.InteropServices;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Security;
using Serilog;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Implementação oficial do motor de limpeza do LSPK Optimizer.
/// Garante execução assíncrona, tolerância a falhas de arquivos travados e contagem real de bytes.
/// </summary>
public class JunkCleanerService : IJunkCleanerService
{
    private static readonly ILogger Logger = Log.ForContext<JunkCleanerService>();

    /// <summary>
    /// Importação da API Win32 nativa Shell32 para esvaziamento seguro da Lixeira em todas as unidades.
    /// </summary>
    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    // Flags da API SHEmptyRecycleBin
    private const uint SHERB_NOCONFIRMATION = 0x00000001; // Não exibir caixa de diálogo do Windows
    private const uint SHERB_NOPROGRESSUI = 0x00000002;   // Não exibir janela de progresso nativa
    private const uint SHERB_NOSOUND = 0x00000004;        // Não tocar som de lixeira esvaziada

    /// <summary>
    /// Executa a varredura prévia de todas as categorias homologadas.
    /// Mede o espaço exato ocupado sem realizar nenhuma alteração destrutiva no disco.
    /// </summary>
    public async Task<List<JunkCategoryScanResult>> ScanAsync(
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null)
    {
        Logger.Information("Iniciando varredura diagnóstica de arquivos desnecessários...");
        var results = new List<JunkCategoryScanResult>();

        // Definição das categorias que serão auditadas sequencialmente
        var categories = new[]
        {
            (JunkCategory.UserTemp, "Arquivos Temporários do Usuário", "Arquivos temporários e caches criados por aplicativos em execução.", false),
            (JunkCategory.WindowsTemp, "Arquivos Temporários do Windows", "Logs e arquivos residuais deixados pelo sistema operacional.", false),
            (JunkCategory.ThumbnailCache, "Cache de Miniaturas", "Bancos de dados de visualização prévia de imagens e vídeos.", false),
            (JunkCategory.ShaderCache, "Cache de Shaders (DirectX / GPU)", "Caches antigos de renderização gráfica recompiláveis pelos jogos.", false),
            (JunkCategory.ErrorReports, "Relatórios de Erros e Crash Dumps", "Arquivos de despejo de memória gerados por falhas antigas.", false),
            (JunkCategory.WindowsUpdateDownload, "Cache de Atualização do Windows", "Instaladores residuais de atualizações já aplicadas com sucesso.", false),
            (JunkCategory.BrowserCache, "Cache de Navegadores", "Arquivos temporários de sites visitados (Chrome, Edge, Brave).", false),
            (JunkCategory.RecycleBin, "Lixeira do Windows", "Arquivos descartados pelo usuário aguardando exclusão definitiva.", true)
        };

        int index = 0;
        foreach (var (cat, title, desc, reqConfirm) in categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Notifica o progresso para a tela
            progress?.Report(new OptimizationProgress
            {
                CurrentStep = index + 1,
                TotalSteps = categories.Length,
                StepTitle = $"Analisando {title}...",
                CurrentDetail = desc
            });

            var scanResult = await ScanCategoryAsync(cat, title, desc, reqConfirm, cancellationToken);
            results.Add(scanResult);
            index++;
        }

        Logger.Information("Varredura concluída. Total de categorias: {Count}, Espaço total estimado: {TotalSize}",
            results.Count,
            JunkCategoryScanResult.FormatBytes(results.Sum(r => r.TotalSizeBytes)));

        return results;
    }

    /// <summary>
    /// Varrer uma categoria individual em segundo plano (Task.Run), identificando arquivos elegíveis.
    /// </summary>
    private Task<JunkCategoryScanResult> ScanCategoryAsync(
        JunkCategory category,
        string title,
        string description,
        bool requiresConfirm,
        CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var result = new JunkCategoryScanResult
            {
                Category = category,
                DisplayName = title,
                Description = description,
                RequiresConfirmation = requiresConfirm,
                IsSelected = !requiresConfirm // Lixeira vem desmarcada por padrão por segurança
            };

            var targetPaths = GetTargetDirectoriesForCategory(category);
            var scannedFiles = new List<ScannedJunkFile>();
            long totalSize = 0;

            foreach (var dir in targetPaths)
            {
                ct.ThrowIfCancellationRequested();
                if (!Directory.Exists(dir)) continue;

                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    // Enumeração ignorando pastas inacessíveis e links simbólicos (reparse points)
                    var files = dirInfo.EnumerateFiles("*", new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = FileAttributes.ReparsePoint
                    });

                    foreach (var file in files)
                    {
                        ct.ThrowIfCancellationRequested();

                        // Validação estrita: O arquivo pode ser excluído sob esta categoria?
                        if (!PathValidator.IsSafeToDelete(file.FullName, category))
                        {
                            continue;
                        }

                        // Protege arquivos muito recentes (< 24h) em pastas temp para não quebrar aplicativos ativos
                        if ((category == JunkCategory.UserTemp || category == JunkCategory.WindowsTemp) &&
                            DateTime.UtcNow - file.LastWriteTimeUtc < TimeSpan.FromHours(24))
                        {
                            continue;
                        }

                        long fileLen = 0;
                        try { fileLen = file.Length; } catch { continue; }

                        totalSize += fileLen;
                        scannedFiles.Add(new ScannedJunkFile(file.FullName, fileLen, file.LastWriteTimeUtc, category));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Aviso ao varrer diretório {Dir} para a categoria {Cat}", dir, category);
                }
            }

            result.Files.AddRange(scannedFiles);
            result.FileCount = scannedFiles.Count;
            result.TotalSizeBytes = totalSize;

            return result;
        }, ct);
    }

    /// <summary>
    /// Executa a exclusão física dos arquivos contidos nas categorias marcadas pelo usuário.
    /// Trata exceções de permissão ou bloqueio sem abortar a limpeza geral.
    /// </summary>
    public async Task<JunkCleanSummaryReport> CleanAsync(
        IEnumerable<JunkCategoryScanResult> selectedCategories,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var report = new JunkCleanSummaryReport();
        var categoriesList = selectedCategories.Where(c => c.IsSelected).ToList();

        Logger.Information("Iniciando limpeza física de {Count} categorias selecionadas...", categoriesList.Count);

        int total = categoriesList.Count;
        int current = 0;

        foreach (var catResult in categoriesList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current++;

            progress?.Report(new OptimizationProgress
            {
                CurrentStep = current,
                TotalSteps = total,
                StepTitle = $"Limpando {catResult.DisplayName}...",
                CurrentDetail = $"Processando {catResult.FileCount} arquivos elegíveis...",
                BytesProcessed = report.TotalBytesFreed
            });

            var cleanResult = await CleanSingleCategoryAsync(catResult, cancellationToken);
            report.CategoryResults.Add(cleanResult);
            report.TotalBytesFreed += cleanResult.BytesFreed;
            report.TotalFilesDeleted += cleanResult.FilesDeleted;
            report.TotalFilesSkippedLocked += cleanResult.FilesSkippedLocked;
        }

        stopwatch.Stop();
        report.Duration = stopwatch.Elapsed;

        Logger.Information("Limpeza finalizada em {Elapsed}s. Bytes liberados: {Bytes}, Arquivos removidos: {Files}, Arquivos bloqueados pulados: {Skipped}",
            stopwatch.Elapsed.TotalSeconds,
            report.FormattedTotalFreed,
            report.TotalFilesDeleted,
            report.TotalFilesSkippedLocked);

        return report;
    }

    /// <summary>
    /// Processa a exclusão de arquivos de uma única categoria.
    /// </summary>
    private Task<JunkCategoryCleanResult> CleanSingleCategoryAsync(
        JunkCategoryScanResult scanResult,
        CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var result = new JunkCategoryCleanResult
            {
                Category = scanResult.Category
            };

            // Caso especial: Lixeira do Windows (usa chamada direta da API Shell32)
            if (scanResult.Category == JunkCategory.RecycleBin)
            {
                if (OperatingSystem.IsWindows())
                {
                    try
                    {
                        SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                        result.BytesFreed = scanResult.TotalSizeBytes;
                        result.FilesDeleted = scanResult.FileCount;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Falha ao esvaziar lixeira: {ex.Message}");
                        Logger.Error(ex, "Falha ao esvaziar lixeira");
                    }
                }
                return result;
            }

            // Exclusão individual de arquivos normais
            foreach (var scannedFile in scanResult.Files)
            {
                ct.ThrowIfCancellationRequested();

                // Dupla checagem de segurança em tempo de execução
                if (!PathValidator.IsSafeToDelete(scannedFile.FullPath, scanResult.Category))
                {
                    Logger.Warning("Tentativa de exclusão bloqueada pelo validador: {Path}", scannedFile.FullPath);
                    continue;
                }

                try
                {
                    if (File.Exists(scannedFile.FullPath))
                    {
                        // Remove atributo de somente leitura para permitir exclusão
                        var attr = File.GetAttributes(scannedFile.FullPath);
                        if ((attr & FileAttributes.ReadOnly) != 0)
                        {
                            File.SetAttributes(scannedFile.FullPath, attr & ~FileAttributes.ReadOnly);
                        }

                        File.Delete(scannedFile.FullPath);
                        // Contabiliza apenas bytes efetivamente deletados
                        result.BytesFreed += scannedFile.SizeBytes;
                        result.FilesDeleted++;
                    }
                }
                catch (IOException)
                {
                    // Arquivo travado em uso por outro processo - ignora pacificamente conforme especificação
                    result.FilesSkippedLocked++;
                }
                catch (UnauthorizedAccessException)
                {
                    // Permissão insuficiente - ignora pacificamente sem crash
                    result.FilesSkippedLocked++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{scannedFile.FullPath}: {ex.Message}");
                }
            }

            // Após apagar arquivos, remove pastas vazias órfãs de temporários
            CleanEmptySubdirectories(scanResult.Category);

            return result;
        }, ct);
    }

    /// <summary>
    /// Remove diretórios vazios remanescentes em pastas temporárias para manter o sistema limpo.
    /// </summary>
    private static void CleanEmptySubdirectories(JunkCategory category)
    {
        if (category != JunkCategory.UserTemp && category != JunkCategory.WindowsTemp) return;

        var targetDirs = GetTargetDirectoriesForCategory(category);
        foreach (var rootDir in targetDirs)
        {
            if (!Directory.Exists(rootDir)) continue;
            try
            {
                foreach (var dir in Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            Directory.Delete(dir, false);
                        }
                    }
                    catch
                    {
                        // Ignora se estiver travada pelo sistema
                    }
                }
            }
            catch
            {
                // Ignora falhas de leitura
            }
        }
    }

    /// <summary>
    /// Retorna os caminhos dos diretórios do sistema alvo para cada categoria de limpeza.
    /// </summary>
    private static List<string> GetTargetDirectoriesForCategory(JunkCategory category)
    {
        var dirs = new List<string>();

        switch (category)
        {
            case JunkCategory.UserTemp:
                dirs.Add(Path.GetTempPath());
                break;

            case JunkCategory.WindowsTemp:
                string win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (!string.IsNullOrEmpty(win)) dirs.Add(Path.Combine(win, "Temp"));
                break;

            case JunkCategory.ThumbnailCache:
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(localApp))
                {
                    dirs.Add(Path.Combine(localApp, "Microsoft", "Windows", "Explorer"));
                }
                break;

            case JunkCategory.ShaderCache:
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(appData))
                {
                    dirs.Add(Path.Combine(appData, "D3DSCache"));
                    dirs.Add(Path.Combine(appData, "NVIDIA", "DXCache"));
                    dirs.Add(Path.Combine(appData, "AMD", "DxCache"));
                }
                break;

            case JunkCategory.ErrorReports:
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(local)) dirs.Add(Path.Combine(local, "CrashDumps"));

                string progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (!string.IsNullOrEmpty(progData)) dirs.Add(Path.Combine(progData, "Microsoft", "Windows", "WER"));
                break;

            case JunkCategory.WindowsUpdateDownload:
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (!string.IsNullOrEmpty(winDir))
                {
                    dirs.Add(Path.Combine(winDir, "SoftwareDistribution", "Download"));
                }
                break;

            case JunkCategory.BrowserCache:
                string userLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(userLocal))
                {
                    dirs.Add(Path.Combine(userLocal, "Google", "Chrome", "User Data", "Default", "Cache"));
                    dirs.Add(Path.Combine(userLocal, "Google", "Chrome", "User Data", "Default", "Code Cache"));
                    dirs.Add(Path.Combine(userLocal, "Microsoft", "Edge", "User Data", "Default", "Cache"));
                    dirs.Add(Path.Combine(userLocal, "BraveSoftware", "Brave-Browser", "User Data", "Default", "Cache"));
                }
                break;
        }

        return dirs;
    }
}
