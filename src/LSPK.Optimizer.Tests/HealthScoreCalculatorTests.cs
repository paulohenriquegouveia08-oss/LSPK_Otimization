using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;
using Xunit;

namespace LSPK.Optimizer.Tests;

/// <summary>
/// Suíte de testes unitários para o cálculo do Score de Saúde (HealthScoreCalculator).
/// Garante que o score varie de 15 a 100 de forma estritamente determinística.
/// </summary>
public class HealthScoreCalculatorTests
{
    private readonly HealthScoreCalculator _calculator = new();

    /// <summary>
    /// Valida que um computador limpo (zero arquivos temporários e sem órfãos) receba a pontuação máxima 100.
    /// </summary>
    [Fact]
    public void Calculate_CleanSystem_ReturnsHighScore()
    {
        var entries = new List<StartupEntry>();
        var report = _calculator.Calculate(0, entries, DateTime.UtcNow);

        // Assert: Sistema totalmente limpo deve receber 100 pontos e status "Excelente"
        Assert.Equal(100, report.Score);
        Assert.Equal("Excelente", report.StatusLevel);
    }

    /// <summary>
    /// Valida que um sistema sobrecarregado com 20 GB de lixo, múltiplos inicializadores pesados
    /// e entradas órfãs sofra deduções auditáveis na pontuação.
    /// </summary>
    [Fact]
    public void Calculate_HeavyJunkAndOrphans_ReducesScoreDeterministically()
    {
        long heavyJunk = 20L * 1024 * 1024 * 1024; // 20 Gigabytes de lixo
        var entries = new List<StartupEntry>
        {
            new() { Name = "Orphan1", TargetExists = false, IsEnabled = true },
            new() { Name = "Orphan2", TargetExists = false, IsEnabled = true },
            new() { Name = "Heavy1", Impact = StartupImpact.High, IsEnabled = true },
            new() { Name = "Heavy2", Impact = StartupImpact.High, IsEnabled = true },
            new() { Name = "Heavy3", Impact = StartupImpact.High, IsEnabled = true },
            new() { Name = "Heavy4", Impact = StartupImpact.High, IsEnabled = true },
            new() { Name = "Heavy5", Impact = StartupImpact.High, IsEnabled = true }
        };

        var report = _calculator.Calculate(heavyJunk, entries, DateTime.UtcNow.AddDays(-60));

        // Assert: A pontuação deve cair para menos de 60 e apresentar recomendações claras
        Assert.True(report.Score < 60, $"Score esperado < 60, obtido: {report.Score}");
        Assert.True(report.Recommendations.Count >= 3);
    }

    /// <summary>
    /// Valida que a pontuação respeite o piso mínimo de 15 pontos e o teto máximo de 100 pontos.
    /// </summary>
    [Fact]
    public void Calculate_ClampsBetween15And100()
    {
        long massiveJunk = 100L * 1024 * 1024 * 1024;
        var lotsOfOrphans = Enumerable.Range(0, 50)
            .Select(i => new StartupEntry { Name = $"Bad{i}", TargetExists = false, IsEnabled = true })
            .ToList();

        var report = _calculator.Calculate(massiveJunk, lotsOfOrphans, null);

        // Assert: Clamping estrito
        Assert.True(report.Score >= 15);
        Assert.True(report.Score <= 100);
    }
}
