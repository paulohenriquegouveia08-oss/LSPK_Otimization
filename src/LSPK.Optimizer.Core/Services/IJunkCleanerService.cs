using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Contrato do serviço central de diagnóstico e limpeza segura de arquivos desnecessários.
/// Responsável por auditar caches, temporários, relatórios de erro e efetuar exclusão atômica.
/// </summary>
public interface IJunkCleanerService
{
    /// <summary>
    /// Executa o diagnóstico completo calculando o espaço recuperável por categoria sem excluir arquivos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento seguro da operação.</param>
    /// <param name="progress">Interface para reporte de progresso em tempo real.</param>
    /// <returns>Lista com o resultado da varredura de cada categoria de limpeza.</returns>
    Task<List<JunkCategoryScanResult>> ScanAsync(
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null
    );

    /// <summary>
    /// Executa a exclusão física dos arquivos contidos nas categorias selecionadas pelo usuário.
    /// </summary>
    /// <param name="selectedCategories">Categorias marcadas para limpeza.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <param name="progress">Reporte de progresso em tempo real.</param>
    /// <returns>Relatório consolidado com a soma de bytes liberados e quantidade de arquivos removidos.</returns>
    Task<JunkCleanSummaryReport> CleanAsync(
        IEnumerable<JunkCategoryScanResult> selectedCategories,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null
    );
}
