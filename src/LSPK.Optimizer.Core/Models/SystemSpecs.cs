namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Modelo com as especificações de hardware e métricas do sistema operacional auditado.
/// Todos os dados são extraídos via APIs reais do Windows (Kernel32, WMI e Environment).
/// </summary>
public class SystemSpecs
{
    /// <summary>Nome do Sistema Operacional (ex: "Windows").</summary>
    public string OsName { get; init; } = "Windows";

    /// <summary>Versão comercial e edição do Windows (ex: "11 Pro", "10 Home").</summary>
    public string OsVersion { get; init; } = "11 / 10";

    /// <summary>Número da compilação do Windows (ex: "22631").</summary>
    public string OsBuild { get; init; } = string.Empty;

    /// <summary>Arquitetura do processador (ex: "X64").</summary>
    public string OsArchitecture { get; init; } = "x64";

    /// <summary>Nome comercial completo da CPU (ex: "Intel(R) Core(TM) i7-13700K" ou "AMD Ryzen 7 7800X3D").</summary>
    public string CpuName { get; init; } = "Processador";

    /// <summary>Quantidade total de núcleos lógicos disponíveis para processamento multithread.</summary>
    public int CpuLogicalCores { get; init; } = 4;

    /// <summary>Capacidade total de memória física instalada em Gigabytes (GB).</summary>
    public double TotalRamGb { get; init; }

    /// <summary>Memória física disponível no momento em Gigabytes (GB).</summary>
    public double AvailableRamGb { get; init; }

    /// <summary>Memória física atualmente em utilização em Gigabytes (GB).</summary>
    public double UsedRamGb => Math.Max(0, TotalRamGb - AvailableRamGb);

    /// <summary>Letra da unidade onde o Windows está instalado (geralmente "C:").</summary>
    public string SystemDriveLetter { get; init; } = "C:";

    /// <summary>Capacidade total de armazenamento do disco do sistema em Gigabytes (GB).</summary>
    public double SystemDriveTotalGb { get; init; }

    /// <summary>Espaço livre remanescente no disco do sistema em Gigabytes (GB).</summary>
    public double SystemDriveFreeGb { get; init; }

    /// <summary>Espaço ocupado no disco do sistema em Gigabytes (GB).</summary>
    public double SystemDriveUsedGb => Math.Max(0, SystemDriveTotalGb - SystemDriveFreeGb);

    /// <summary>Percentual de uso do disco do sistema (0 a 100%).</summary>
    public int SystemDrivePercentUsed => SystemDriveTotalGb > 0
        ? (int)Math.Round((SystemDriveUsedGb / SystemDriveTotalGb) * 100)
        : 0;

    /// <summary>Percentual de uso da memória RAM (0 a 100%).</summary>
    public int RamPercentUsed => TotalRamGb > 0
        ? (int)Math.Round((UsedRamGb / TotalRamGb) * 100)
        : 0;
}
