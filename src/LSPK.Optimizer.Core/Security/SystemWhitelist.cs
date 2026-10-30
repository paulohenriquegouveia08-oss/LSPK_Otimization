namespace LSPK.Optimizer.Core.Security;

/// <summary>
/// Lista de proteção inviolável para serviços essenciais, antivírus e processos do Windows.
/// Impede que o usuário ou a otimização automática desativem itens críticos do sistema.
/// </summary>
public static class SystemWhitelist
{
    /// <summary>
    /// Conjunto de nomes de executáveis e processos que não podem ser desativados.
    /// Inclui o shell do Windows (Explorer), processos de segurança (Defender) e drivers de vídeo/áudio.
    /// </summary>
    private static readonly HashSet<string> ProtectedProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // 1. Windows Core & Shell (Fundamentais para o funcionamento da interface e login)
        "explorer",
        "explorer.exe",
        "svchost",
        "svchost.exe",
        "runtimebroker",
        "runtimebroker.exe",
        "taskhostw",
        "taskhostw.exe",
        "sihost",
        "sihost.exe",
        "ctfmon",
        "ctfmon.exe",
        "lsass",
        "lsass.exe",
        "csrss",
        "csrss.exe",
        "services",
        "services.exe",

        // 2. Windows Security & Defender (Proteção em tempo real contra malwares)
        "msmpeng",
        "msmpeng.exe",
        "securityhealthsystray",
        "securityhealthsystray.exe",
        "smartscreen",
        "smartscreen.exe",
        "windefend",
        "mpcmdrun",
        "mpcmdrun.exe",

        // 3. Drivers e Utilitários Essenciais de Hardware (Áudio e Vídeo)
        "rtkngui64",
        "rtkngui64.exe",
        "ravcpl64",
        "ravcpl64.exe",
        "igfxpers",
        "igfxpers.exe",
        "nvcontainer",
        "nvcontainer.exe",
        "nvdisplay.container",
        "nvdisplay.container.exe",
        "radeonsoftware",
        "radeonsoftware.exe",

        // 4. Antivírus de Terceiros Homologados (Prevenção contra conflitos de segurança)
        "avp",
        "avp.exe",
        "bdservicehost",
        "bdservicehost.exe",
        "mbam",
        "mbam.exe",
        "ekrn",
        "ekrn.exe",
        "egui",
        "egui.exe",
        "nsw",
        "nsw.exe",
        "avastui",
        "avastui.exe",
        "avgui",
        "avgui.exe"
    };

    /// <summary>
    /// Fabricantes oficiais cujos binários localizados em pastas de sistema são considerados críticos.
    /// </summary>
    private static readonly string[] ProtectedPublishers =
    {
        "Microsoft Windows",
        "Microsoft Corporation",
        "NVIDIA Corporation",
        "Intel Corporation",
        "Advanced Micro Devices",
        "Realtek Semiconductor Corp."
    };

    /// <summary>
    /// Avalia se a entrada de inicialização pertence a um componente essencial protegido.
    /// Realiza verificações pelo nome do processo, nome da entrada e fabricante oficial.
    /// </summary>
    /// <param name="entryName">Nome da chave ou atalho.</param>
    /// <param name="executablePath">Caminho resolvido do binário.</param>
    /// <param name="publisher">Nome da empresa desenvolvedora.</param>
    /// <returns>Verdadeiro se o item for protegido; Falso caso possa ser gerenciado livremente.</returns>
    public static bool IsProtected(string entryName, string executablePath, string publisher)
    {
        if (string.IsNullOrWhiteSpace(entryName) && string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        // Checagem 1: Pelo nome do arquivo executável (com e sem extensão .exe)
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            string fileName = Path.GetFileName(executablePath).ToLowerInvariant();
            if (ProtectedProcessNames.Contains(fileName)) return true;

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(executablePath).ToLowerInvariant();
            if (ProtectedProcessNames.Contains(fileNameWithoutExt)) return true;
        }

        // Checagem 2: Pelo nome da entrada gravada no registro
        if (!string.IsNullOrWhiteSpace(entryName))
        {
            string entryLower = entryName.ToLowerInvariant();
            if (ProtectedProcessNames.Contains(entryLower)) return true;
        }

        // Checagem 3: Pelo fabricante do binário (se for Microsoft e residir em System32)
        if (!string.IsNullOrWhiteSpace(publisher))
        {
            foreach (var protPub in ProtectedPublishers)
            {
                if (publisher.Contains(protPub, StringComparison.OrdinalIgnoreCase))
                {
                    if (executablePath.Contains("System32", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
