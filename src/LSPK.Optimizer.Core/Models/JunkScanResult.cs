namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Representa os metadados de um arquivo individual identificado durante a etapa de varredura pré-limpeza.
/// </summary>
/// <param name="FullPath">Caminho canônico absoluto do arquivo no disco.</param>
/// <param name="SizeBytes">Tamanho real em bytes ocupado pelo arquivo no sistema de arquivos.</param>
/// <param name="LastModified">Data e hora UTC da última modificação do arquivo (usada para proteger arquivos recentes).</param>
/// <param name="Category">Categoria de limpeza à qual este arquivo pertence.</param>
/// <param name="IsLocked">Indica se o arquivo foi detectado como em uso ou bloqueado por outro processo.</param>
public record ScannedJunkFile(
    string FullPath,
    long SizeBytes,
    DateTime LastModified,
    JunkCategory Category,
    bool IsLocked = false
);

/// <summary>
/// Estrutura de dados que consolida o resultado do diagnóstico para uma categoria específica de limpeza.
/// É apresentada na interface com opções de seleção individual antes da execução física.
/// </summary>
public class JunkCategoryScanResult
{
    /// <summary>Identificador da categoria de limpeza.</summary>
    public JunkCategory Category { get; init; }

    /// <summary>Nome amigável da categoria exibido na interface gráfica (ex: "Arquivos Temporários do Usuário").</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Descrição explicativa detalhando o impacto e a finalidade da limpeza da categoria.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Volume total de dados recuperáveis somando todos os arquivos elegíveis identificados.</summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>Quantidade total de arquivos encontrados na varredura para esta categoria.</summary>
    public int FileCount { get; set; }

    /// <summary>Define se esta categoria está selecionada pelo usuário para ser limpa.</summary>
    public bool IsSelected { get; set; } = true;

    /// <summary>Indica se a categoria é sensível e necessita de consentimento explícito (ex: Lixeira).</summary>
    public bool RequiresConfirmation { get; init; }

    /// <summary>Lista de arquivos individuais auditados e validados por regras de segurança.</summary>
    public List<ScannedJunkFile> Files { get; init; } = new();

    /// <summary>Representação textual legível do tamanho (ex: "1.4 GB", "350 MB").</summary>
    public string FormattedSize => FormatBytes(TotalSizeBytes);

    /// <summary>
    /// Converte um valor bruto em bytes para uma unidade de medida legível com base decimal (B, KB, MB, GB, TB).
    /// </summary>
    /// <param name="bytes">Quantidade bruta de bytes.</param>
    /// <returns>String formatada com uma casa decimal e o sufixo correspondente.</returns>
    public static string FormatBytes(long bytes)
    {
        if (bytes <= 0) return "0 MB";
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}

/// <summary>
/// Contabiliza o resultado da execução física de limpeza de uma categoria, registrando bytes liberados e falhas toleradas.
/// </summary>
public class JunkCategoryCleanResult
{
    /// <summary>Categoria processada.</summary>
    public JunkCategory Category { get; init; }

    /// <summary>Volume físico em bytes efetivamente deletado com sucesso do disco.</summary>
    public long BytesFreed { get; set; }

    /// <summary>Número total de arquivos excluídos fisicamente.</summary>
    public int FilesDeleted { get; set; }

    /// <summary>Número de arquivos que estavam em uso por outros programas e foram ignorados pacificamente.</summary>
    public int FilesSkippedLocked { get; set; }

    /// <summary>Registro de mensagens de erro ou restrições de permissão ocorridas durante a operação.</summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>Espaço liberado formatado amigavelmente.</summary>
    public string FormattedFreed => JunkCategoryScanResult.FormatBytes(BytesFreed);
}

/// <summary>
/// Relatório consolidado final do módulo de limpeza emitido após o término de todas as categorias.
/// </summary>
public class JunkCleanSummaryReport
{
    /// <summary>Data e hora em que a limpeza foi finalizada.</summary>
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Soma total de bytes recuperados em todas as categorias processadas.</summary>
    public long TotalBytesFreed { get; set; }

    /// <summary>Total acumulado de arquivos apagados.</summary>
    public int TotalFilesDeleted { get; set; }

    /// <summary>Total de arquivos travados que foram ignorados de acordo com as regras de resiliência.</summary>
    public int TotalFilesSkippedLocked { get; set; }

    /// <summary>Duração total da operação de limpeza.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Resultados segregados por cada categoria processada.</summary>
    public List<JunkCategoryCleanResult> CategoryResults { get; init; } = new();

    /// <summary>Espaço total recuperado formatado para exibição ao usuário.</summary>
    public string FormattedTotalFreed => JunkCategoryScanResult.FormatBytes(TotalBytesFreed);
}
