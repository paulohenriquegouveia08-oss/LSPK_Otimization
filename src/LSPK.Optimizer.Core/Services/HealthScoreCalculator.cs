using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Contrato de serviço para cálculo auditável do Score de Saúde do computador (0 a 100).
/// </summary>
public interface IHealthScoreCalculator
{
    /// <summary>
    /// Calcula a pontuação e gera recomendações com base em dados físicos verificáveis.
    /// </summary>
    /// <param name="totalJunkBytes">Volume total de arquivos desnecessários diagnosticados.</param>
    /// <param name="startupEntries">Lista de programas de inicialização identificados.</param>
    /// <param name="lastOptimizationDate">Data da última manutenção registrada.</param>
    /// <returns>Relatório consolidado de saúde com a pontuação e recomendações.</returns>
    SystemHealthReport Calculate(
        long totalJunkBytes,
        IReadOnlyList<StartupEntry> startupEntries,
        DateTime? lastOptimizationDate
    );
}

/// <summary>
/// Implementação do algoritmo determinístico de pontuação de saúde do LSPK Optimizer.
/// Inicia com pontuação máxima (100) e deduz pontos com base em métricas reais de degradação.
/// </summary>
public class HealthScoreCalculator : IHealthScoreCalculator
{
    public SystemHealthReport Calculate(
        long totalJunkBytes,
        IReadOnlyList<StartupEntry> startupEntries,
        DateTime? lastOptimizationDate)
    {
        int score = 100;
        var recommendations = new List<string>();

        // FATOR 1: Penalidade por volume de arquivos desnecessários acumulados no disco
        double junkGb = (double)totalJunkBytes / (1024 * 1024 * 1024);
        if (junkGb > 15.0)
        {
            score -= 40;
            recommendations.Add($"Existe mais de {junkGb:N1} GB de arquivos temporários e caches desnecessários ocupando o disco.");
        }
        else if (junkGb > 5.0)
        {
            score -= 25;
            recommendations.Add($"Encontrados {junkGb:N1} GB de arquivos temporários elegíveis para limpeza.");
        }
        else if (junkGb > 1.5)
        {
            score -= 15;
            recommendations.Add($"Recomenda-se realizar uma limpeza de rotina ({junkGb:N1} GB identificados).");
        }
        else if (junkGb > 0.5)
        {
            score -= 5;
        }

        // FATOR 2: Penalidade por excesso de inicializadores pesados (alto impacto no boot)
        int highImpact = startupEntries.Count(e => e.IsEnabled && e.Impact == StartupImpact.High);
        if (highImpact > 3)
        {
            int penalty = (highImpact - 3) * 5;
            score -= Math.Min(25, penalty);
            recommendations.Add($"{highImpact} programas de alto impacto iniciam automaticamente, atrasando o boot do Windows.");
        }

        // FATOR 3: Penalidade por entradas órfãs (resquícios de softwares já removidos)
        int orphans = startupEntries.Count(e => e.IsOrphaned);
        if (orphans > 0)
        {
            score -= Math.Min(15, orphans * 5);
            recommendations.Add($"{orphans} entradas de inicialização apontam para arquivos inexistentes.");
        }

        // FATOR 4: Penalidade por tempo sem manutenção preventiva
        if (lastOptimizationDate.HasValue)
        {
            var daysSince = (DateTime.UtcNow - lastOptimizationDate.Value).TotalDays;
            if (daysSince > 30)
            {
                score -= 10;
                recommendations.Add("Faz mais de 30 dias desde a última otimização do sistema.");
            }
        }
        else
        {
            score -= 10;
            recommendations.Add("Nenhuma otimização registrada neste computador.");
        }

        // Limita a pontuação entre um mínimo seguro (15) e o teto máximo (100)
        score = Math.Clamp(score, 15, 100);

        return new SystemHealthReport
        {
            Score = score,
            TotalJunkBytes = totalJunkBytes,
            TotalStartupPrograms = startupEntries.Count(e => e.IsEnabled),
            HighImpactStartupCount = highImpact,
            OrphanedStartupCount = orphans,
            LastOptimizationDate = lastOptimizationDate,
            Recommendations = recommendations
        };
    }
}
