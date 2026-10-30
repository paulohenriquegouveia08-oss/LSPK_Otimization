using System.Text.Json;
using LSPK.Optimizer.Core.Models;
using Serilog;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Contrato de serviço para armazenamento e consulta do histórico local de otimizações.
/// </summary>
public interface ILocalHistoryService
{
    /// <summary>
    /// Recupera os registros históricos armazenados em disco ordenados do mais recente para o mais antigo.
    /// </summary>
    Task<List<OptimizationHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo registro ao histórico local persistido em JSON.
    /// </summary>
    Task AddRecordAsync(OptimizationHistoryItem item, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de histórico local utilizando arquivo JSON em %LocalAppData%\LSPK Optimizer.
/// Garante que o usuário mantenha total privacidade, sem dependência de banco de dados remoto ou nuvem.
/// </summary>
public class LocalHistoryService : ILocalHistoryService
{
    private static readonly ILogger Logger = Log.ForContext<LocalHistoryService>();

    /// <summary>Caminho absoluto do arquivo history.json no diretório de dados locais do usuário.</summary>
    private static string HistoryFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LSPK Optimizer",
        "history.json"
    );

    /// <summary>
    /// Lê e desserializa o arquivo de histórico. Retorna lista vazia caso o arquivo não exista ou esteja corrompido.
    /// </summary>
    public async Task<List<OptimizationHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(HistoryFilePath))
                {
                    return new List<OptimizationHistoryItem>();
                }

                string json = File.ReadAllText(HistoryFilePath);
                var items = JsonSerializer.Deserialize<List<OptimizationHistoryItem>>(json);
                return items?.OrderByDescending(i => i.Timestamp).ToList() ?? new List<OptimizationHistoryItem>();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Erro ao carregar histórico local");
                return new List<OptimizationHistoryItem>();
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Adiciona um novo registro no topo da lista e persiste no arquivo, mantendo até 50 registros recentes.
    /// </summary>
    public async Task AddRecordAsync(OptimizationHistoryItem item, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            try
            {
                var dir = Path.GetDirectoryName(HistoryFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var list = new List<OptimizationHistoryItem>();
                if (File.Exists(HistoryFilePath))
                {
                    try
                    {
                        string existing = File.ReadAllText(HistoryFilePath);
                        list = JsonSerializer.Deserialize<List<OptimizationHistoryItem>>(existing) ?? new List<OptimizationHistoryItem>();
                    }
                    catch { /* Em caso de corrupção acidental, reinicia a lista */ }
                }

                // Insere no início para manter a ordem cronológica decrescente
                list.Insert(0, item);

                // Limita aos 50 registros mais recentes para economizar espaço
                if (list.Count > 50)
                {
                    list = list.Take(50).ToList();
                }

                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(HistoryFilePath, json);
                Logger.Information("Novo registro de otimização salvo no histórico local.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Falha ao gravar registro no histórico local");
            }
        }, cancellationToken);
    }
}
