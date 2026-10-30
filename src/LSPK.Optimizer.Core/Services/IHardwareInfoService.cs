using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Contrato do serviço de coleta de informações e telemetria de hardware e sistema operacional.
/// Responsável por ler CPU, memória RAM, versão do Windows e espaço livre na unidade do sistema.
/// </summary>
public interface IHardwareInfoService
{
    /// <summary>
    /// Extrai assincronamente as especificações reais de hardware e armazenamento da máquina.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Objeto SystemSpecs contendo os dados auditados.</returns>
    Task<SystemSpecs> GetSystemSpecsAsync(CancellationToken cancellationToken = default);
}
