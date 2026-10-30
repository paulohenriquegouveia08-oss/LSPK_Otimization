namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Estrutura do manifesto de atualização automática distribuído estaticamente no servidor em updates/latest.json.
/// Consumido pelo UpdateService para verificar compatibilidade e integridade de novas versões.
/// </summary>
public class UpdateManifest
{
    /// <summary>Versão semântica mais recente disponível (ex: "1.0.1").</summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>Data e hora UTC em que a versão foi publicada.</summary>
    public DateTime ReleaseDate { get; init; }

    /// <summary>Versão mínima instalada exigida para permitir a aplicação direta deste pacote.</summary>
    public string MinRequiredVersion { get; init; } = "1.0.0";

    /// <summary>URL pública HTTPS direta para download do pacote compactado ZIP da atualização.</summary>
    public string DownloadUrl { get; init; } = string.Empty;

    /// <summary>Hash criptográfico SHA-256 do arquivo ZIP para validação antes da aplicação.</summary>
    public string Sha256 { get; init; } = string.Empty;

    /// <summary>Indica se a atualização é de segurança crítica e deve ser aplicada obrigatoriamente.</summary>
    public bool Critical { get; init; }

    /// <summary>Lista de notas de lançamento (changelog) com as novidades e correções da versão.</summary>
    public List<string> ReleaseNotes { get; init; } = new();
}

/// <summary>
/// Registro histórico persistido localmente no arquivo history.json em %LocalAppData%\LSPK Optimizer.
/// Garante que o usuário consiga auditar manutenções passadas mesmo após fechar o aplicativo.
/// </summary>
public class OptimizationHistoryItem
{
    /// <summary>Identificador único do registro.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>Data e hora em que a otimização foi executada.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Volume total de dados efetivamente excluídos e recuperados no disco.</summary>
    public long BytesFreed { get; init; }

    /// <summary>Número total de arquivos excluídos nesta sessão.</summary>
    public int FilesDeleted { get; init; }

    /// <summary>Número de entradas de inicialização órfãs que foram desativadas/otimizadas.</summary>
    public int StartupItemsOptimized { get; init; }

    /// <summary>Score de saúde do sistema diagnosticado antes do início da otimização.</summary>
    public int HealthScoreBefore { get; init; }

    /// <summary>Score de saúde recalculado após a conclusão bem-sucedida das etapas.</summary>
    public int HealthScoreAfter { get; init; }

    /// <summary>Tempo total decorrido durante a execução de todas as etapas.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Texto formatado da quantidade de espaço liberado (ex: "2.3 GB").</summary>
    public string FormattedBytesFreed => JunkCategoryScanResult.FormatBytes(BytesFreed);
}
