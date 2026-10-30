using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Contrato do serviço de gerenciamento e calibração de programas de inicialização do Windows.
/// Responsável por auditar chaves do Registro (HKCU/HKLM) e pastas Startup, desativando órfãos e mantendo backups.
/// </summary>
public interface IStartupManagerService
{
    /// <summary>
    /// Lista todos os programas configurados para inicializar junto com o Windows.
    /// </summary>
    Task<List<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa com segurança uma entrada, criando backup dedicado na chave da LSPK e impedindo perda de dados.
    /// </summary>
    Task<bool> DisableEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reativa uma entrada previamente desativada, restaurando seu comando original no registro ou pasta Startup.
    /// </summary>
    Task<bool> EnableEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove com segurança o resquício de uma entrada comprovadamente órfã (arquivo inexistente).
    /// </summary>
    Task<bool> RemoveOrphanedEntryAsync(StartupEntry entry, CancellationToken cancellationToken = default);
}
