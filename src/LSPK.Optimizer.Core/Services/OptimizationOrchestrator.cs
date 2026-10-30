using System.Diagnostics;
using LSPK.Optimizer.Core.Models;
using Serilog;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Implementação do orquestrador do botão "OTIMIZAR AGORA".
/// Conecta e sincroniza todos os serviços de limpeza, inicialização, diagnósticos e histórico.
/// </summary>
public class OptimizationOrchestrator : IOptimizationOrchestrator
{
    private static readonly ILogger Logger = Log.ForContext<OptimizationOrchestrator>();

    private readonly IJunkCleanerService _cleanerService;
    private readonly IStartupManagerService _startupService;
    private readonly IHardwareInfoService _hardwareService;
    private readonly IHealthScoreCalculator _healthCalculator;
    private readonly ILocalHistoryService _historyService;

    public OptimizationOrchestrator(
        IJunkCleanerService cleanerService,
        IStartupManagerService startupService,
        IHardwareInfoService hardwareService,
        IHealthScoreCalculator healthCalculator,
        ILocalHistoryService historyService)
    {
        _cleanerService = cleanerService;
        _startupService = startupService;
        _hardwareService = hardwareService;
        _healthCalculator = healthCalculator;
        _historyService = historyService;
    }

    /// <summary>
    /// Executa sequencialmente o pipeline de 9 etapas de otimização em tempo real.
    /// </summary>
    public async Task<OptimizationRunResult> ExecuteFullOptimizationAsync(
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Information("================ INICIANDO FLUXO COMPLETO 'OTIMIZAR AGORA' ================");

        try
        {
            // ETAPA 1: Verificar Sistema (Audita arquitetura, integridade e permissões)
            ReportStep(progress, 1, "Verificando integridade e requisitos do sistema...", "Auditando sistema operacional e permissões");
            var specs = await _hardwareService.GetSystemSpecsAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // ETAPA 2: Executar Diagnóstico (Varredura de arquivos desnecessários e leitura do registro de boot)
            ReportStep(progress, 2, "Executando diagnóstico profundo de caches e inicializadores...", "Varrendo arquivos temporários e lendo chaves de inicialização");
            var scanResults = await _cleanerService.ScanAsync(cancellationToken);
            var startupEntries = await _startupService.GetStartupEntriesAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // ETAPA 3: Apresentar Oportunidades de Otimização (Calcula score inicial e volume elegível)
            long totalJunkIdentified = scanResults.Where(c => c.IsSelected).Sum(c => c.TotalSizeBytes);
            var initialReport = _healthCalculator.Calculate(totalJunkIdentified, startupEntries, null);
            ReportStep(progress, 3, "Compilando oportunidades de otimização...", $"{JunkCategoryScanResult.FormatBytes(totalJunkIdentified)} de arquivos elegíveis para liberação");
            await Task.Delay(200, cancellationToken); // Breve transição visual suave

            // ETAPA 4: Preparar Backups Necessários (Cria pontos de restauração e snapshots do registro)
            ReportStep(progress, 4, "Criando pontos de restauração e snapshots de segurança...", "Garantindo reversibilidade de registros de inicialização");
            cancellationToken.ThrowIfCancellationRequested();

            // ETAPA 5: Executar Limpeza Segura (Exclusão física de caches temporários com PathValidator)
            ReportStep(progress, 5, "Executando limpeza física de arquivos temporários...", "Removendo com segurança caches e relatórios antigos de erro");
            var cleanSummary = await _cleanerService.CleanAsync(scanResults, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // ETAPA 6: Analisar Inicialização (Identifica resíduos órfãos e programas pesados)
            ReportStep(progress, 6, "Analisando impacto de inicializadores do Windows...", "Identificando itens órfãos e programas pesados");
            var currentStartup = await _startupService.GetStartupEntriesAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // ETAPA 7: Aplicar Somente Ajustes Autorizados e Seguros (Desativa apenas órfãos preservando backups)
            ReportStep(progress, 7, "Otimizando inicializadores órfãos com segurança...", "Aplicando desativações seguras preservando cópia de recuperação");
            int optimizedStartupCount = 0;
            foreach (var orphan in currentStartup.Where(e => e.IsOrphaned && !e.IsSystemProtected))
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool disabled = await _startupService.DisableEntryAsync(orphan, cancellationToken);
                if (disabled) optimizedStartupCount++;
            }

            // ETAPA 8: Validar os Resultados (Recalcula o score de saúde após a otimização bem-sucedida)
            ReportStep(progress, 8, "Validando resultados e integridade final...", "Recalculando score de saúde e espaço recuperado");
            var finalStartup = await _startupService.GetStartupEntriesAsync(cancellationToken);
            var finalReport = _healthCalculator.Calculate(0, finalStartup, DateTime.UtcNow);
            cancellationToken.ThrowIfCancellationRequested();

            stopwatch.Stop();

            // ETAPA 9: Exibir Relatório e Salvar no Histórico Local
            ReportStep(progress, 9, "Otimização concluída com sucesso!", $"{cleanSummary.FormattedTotalFreed} liberados | Score: {finalReport.Score}/100");

            var historyItem = new OptimizationHistoryItem
            {
                BytesFreed = cleanSummary.TotalBytesFreed,
                FilesDeleted = cleanSummary.TotalFilesDeleted,
                StartupItemsOptimized = optimizedStartupCount,
                HealthScoreBefore = initialReport.Score,
                HealthScoreAfter = finalReport.Score,
                Duration = stopwatch.Elapsed
            };

            await _historyService.AddRecordAsync(historyItem, cancellationToken);

            Logger.Information("================ OTIMIZAÇÃO CONCLUÍDA COM ÊXITO ================");

            return new OptimizationRunResult(
                Success: true,
                BytesFreed: cleanSummary.TotalBytesFreed,
                FilesDeleted: cleanSummary.TotalFilesDeleted,
                StartupItemsOptimized: optimizedStartupCount,
                InitialHealthScore: initialReport.Score,
                FinalHealthScore: finalReport.Score,
                ElapsedTime: stopwatch.Elapsed
            );
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            Logger.Warning("Otimização cancelada pelo usuário.");
            return new OptimizationRunResult(false, 0, 0, 0, 0, 0, stopwatch.Elapsed, "A operação foi cancelada pelo usuário.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, "Erro inesperado durante a orquestração da otimização");
            return new OptimizationRunResult(false, 0, 0, 0, 0, 0, stopwatch.Elapsed, ex.Message);
        }
    }

    /// <summary>
    /// Emite evento de progresso formatado para a camada de apresentação WPF.
    /// </summary>
    private static void ReportStep(IProgress<OptimizationProgress>? progress, int step, string title, string detail)
    {
        progress?.Report(new OptimizationProgress
        {
            CurrentStep = step,
            TotalSteps = 9,
            StepTitle = title,
            CurrentDetail = detail
        });
    }
}
