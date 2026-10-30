using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Estrutura consolidada de dados contendo o resultado da execução completa de 1 clique do otimizador.
/// </summary>
/// <param name="Success">Indica se a operação foi concluída sem interrupções críticas.</param>
/// <param name="BytesFreed">Volume físico total em bytes liberado no disco do sistema.</param>
/// <param name="FilesDeleted">Quantidade total de arquivos apagados.</param>
/// <param name="StartupItemsOptimized">Número de entradas órfãs de inicialização ajustadas.</param>
/// <param name="InitialHealthScore">Pontuação de saúde do sistema mensurada no diagnóstico inicial.</param>
/// <param name="FinalHealthScore">Pontuação de saúde recalculada após as otimizações.</param>
/// <param name="ElapsedTime">Tempo total decorrido durante as 9 etapas.</param>
/// <param name="ErrorMessage">Mensagem descritiva em caso de falha ou cancelamento pelo usuário.</param>
public record OptimizationRunResult(
    bool Success,
    long BytesFreed,
    int FilesDeleted,
    int StartupItemsOptimized,
    int InitialHealthScore,
    int FinalHealthScore,
    TimeSpan ElapsedTime,
    string? ErrorMessage = null
);

/// <summary>
/// Contrato do orquestrador central do botão "OTIMIZAR AGORA".
/// Coordena a execução sequencial e atômica das 9 etapas da otimização do Windows.
/// </summary>
public interface IOptimizationOrchestrator
{
    /// <summary>
    /// Inicia o pipeline completo de otimização reportando o progresso para a tela em tempo real.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento seguro a qualquer momento.</param>
    /// <param name="progress">Interface de notificação com etapa, título, detalhe e percentual.</param>
    /// <returns>Resultado final consolidado com métricas da otimização.</returns>
    Task<OptimizationRunResult> ExecuteFullOptimizationAsync(
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progress = null
    );
}
