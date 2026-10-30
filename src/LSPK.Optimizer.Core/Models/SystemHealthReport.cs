namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Avaliação de saúde do sistema calculada a partir de critérios estritamente mensuráveis.
/// Não utiliza números aleatórios ou simulações.
/// </summary>
public class SystemHealthReport
{
    /// <summary>Pontuação de saúde variando de 15 a 100 pontos.</summary>
    public int Score { get; init; }

    /// <summary>Classificação textual do nível de saúde do computador baseada na pontuação.</summary>
    public string StatusLevel => Score switch
    {
        >= 85 => "Excelente",
        >= 65 => "Bom",
        >= 45 => "Atenção",
        _ => "Crítico"
    };

    /// <summary>Cor hexadecimal recomendada para o status na interface gráfica.</summary>
    public string StatusColor => Score switch
    {
        >= 85 => "#10B981", // Verde Esmeralda
        >= 65 => "#3B82F6", // Azul Elétrico
        >= 45 => "#F59E0B", // Âmbar de Alerta
        _ => "#EF4444"      // Vermelho Crítico
    };

    /// <summary>Volume total em bytes de arquivos desnecessários diagnosticados no computador.</summary>
    public long TotalJunkBytes { get; init; }

    /// <summary>Total de programas ativos configurados para iniciar com o Windows.</summary>
    public int TotalStartupPrograms { get; init; }

    /// <summary>Quantidade de itens de alto impacto identificados na inicialização.</summary>
    public int HighImpactStartupCount { get; init; }

    /// <summary>Quantidade de entradas de inicialização órfãs que apontam para arquivos inexistentes.</summary>
    public int OrphanedStartupCount { get; init; }

    /// <summary>Data e hora em que a última otimização foi realizada pelo aplicativo.</summary>
    public DateTime? LastOptimizationDate { get; init; }

    /// <summary>Lista de recomendações automáticas para orientar o usuário sobre os pontos de melhoria.</summary>
    public List<string> Recommendations { get; init; } = new();
}

/// <summary>
/// Evento de progresso emitido em tempo real pelo orquestrador durante a execução do "Otimizar Agora".
/// Permite atualizar a barra de progresso e textos de status de forma suave na interface.
/// </summary>
public class OptimizationProgress
{
    /// <summary>Número da etapa atual em execução (1 a 9).</summary>
    public int CurrentStep { get; init; }

    /// <summary>Número total de etapas planejadas para a otimização completa (padrão 9).</summary>
    public int TotalSteps { get; init; } = 9;

    /// <summary>Título descritivo da etapa em andamento (ex: "Verificando integridade do sistema...").</summary>
    public string StepTitle { get; init; } = string.Empty;

    /// <summary>Detalhes técnicos ou arquivo/operação sendo executado no momento.</summary>
    public string CurrentDetail { get; init; } = string.Empty;

    /// <summary>Percentual total concluído baseado na fração de etapas finalizadas (0 a 100%).</summary>
    public double OverallPercent => TotalSteps > 0 ? ((double)CurrentStep / TotalSteps) * 100 : 0;

    /// <summary>Volume total de bytes já processados ou liberados até o momento.</summary>
    public long BytesProcessed { get; init; }
}
