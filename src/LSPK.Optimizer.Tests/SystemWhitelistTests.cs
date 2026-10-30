using LSPK.Optimizer.Core.Security;
using Xunit;

namespace LSPK.Optimizer.Tests;

/// <summary>
/// Suíte de testes unitários para a lista de proteção de processos essenciais (SystemWhitelist).
/// Garante que antivírus, processos do sistema e drivers vitais sejam impedidos de desativação.
/// </summary>
public class SystemWhitelistTests
{
    /// <summary>
    /// Valida que componentes protegidos do Windows (Defender, Explorer, Drivers de GPU e Áudio)
    /// são classificados corretamente como IsProtected = True.
    /// </summary>
    [Theory]
    [InlineData("explorer.exe", "C:\\Windows\\explorer.exe", "Microsoft Corporation")]
    [InlineData("msmpeng.exe", "C:\\ProgramData\\Microsoft\\Windows Defender\\Platform\\msmpeng.exe", "Microsoft Corporation")]
    [InlineData("securityhealthsystray.exe", "C:\\Windows\\System32\\SecurityHealthSystray.exe", "Microsoft Windows")]
    [InlineData("rtkngui64.exe", "C:\\Program Files\\Realtek\\Audio\\HDA\\rtkngui64.exe", "Realtek Semiconductor Corp.")]
    [InlineData("nvcontainer.exe", "C:\\Program Files\\NVIDIA Corporation\\nvcontainer.exe", "NVIDIA Corporation")]
    [InlineData("mbam.exe", "C:\\Program Files\\Malwarebytes\\mbam.exe", "Malwarebytes")]
    public void IsProtected_IdentifiesCriticalComponents(string name, string path, string publisher)
    {
        // Act: Avalia se o item pertence à lista branca de proteção
        bool isProtected = SystemWhitelist.IsProtected(name, path, publisher);

        // Assert: Deve ser estritamente protegido
        Assert.True(isProtected, $"O componente crítico {name} deveria ser classificado como protegido!");
    }

    /// <summary>
    /// Valida que softwares comuns de terceiros (Discord, Spotify, Steam) NÃO são protegidos,
    /// permitindo que o usuário os desative para acelerar o boot.
    /// </summary>
    [Theory]
    [InlineData("Discord", "C:\\Users\\User\\AppData\\Local\\Discord\\app.exe", "Discord Inc.")]
    [InlineData("Spotify", "C:\\Users\\User\\AppData\\Roaming\\Spotify\\Spotify.exe", "Spotify AB")]
    [InlineData("Steam", "C:\\Program Files (x86)\\Steam\\steam.exe", "Valve Corporation")]
    [InlineData("UncheckedApp", "C:\\Apps\\RandomApp.exe", "Unknown")]
    public void IsProtected_AllowsNonCriticalApps(string name, string path, string publisher)
    {
        // Act: Avalia o aplicativo comum
        bool isProtected = SystemWhitelist.IsProtected(name, path, publisher);

        // Assert: Não deve ser protegido pelo sistema
        Assert.False(isProtected, $"O aplicativo comum {name} NÃO deveria ser protegido!");
    }
}
