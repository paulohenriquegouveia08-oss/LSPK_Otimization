using LSPK.Optimizer.Core.Services;
using Xunit;

namespace LSPK.Optimizer.Tests;

/// <summary>
/// Suíte de testes unitários para a rotina de parsing de linhas de comando de inicialização.
/// </summary>
public class StartupManagerTests
{
    /// <summary>
    /// Valida se comandos de inicialização com aspas duplas e argumentos adicionais
    /// são devidamente divididos em caminho do executável limpo e argumentos.
    /// </summary>
    [Theory]
    [InlineData("\"C:\\Program Files\\App\\app.exe\" --silent", "C:\\Program Files\\App\\app.exe", "--silent")]
    [InlineData("\"D:\\Tools\\utility.exe\"", "D:\\Tools\\utility.exe", "")]
    public void ParseCommandLine_ParsesQuotesCorrectly(string raw, string expectedExe, string expectedArgs)
    {
        // Act: Executa o parser da linha de comando bruta
        var (exe, args) = StartupManagerService.ParseCommandLine(raw);

        // Assert: Executável e argumentos devem bater exatamente
        Assert.Equal(expectedExe, exe);
        Assert.Equal(expectedArgs, args);
    }
}

/// <summary>
/// Suíte de testes unitários para o serviço de atualizações e integridade criptográfica.
/// </summary>
public class UpdateServiceTests
{
    /// <summary>
    /// Valida se a rotina de cálculo e verificação de SHA-256 reconhece hashes válidos
    /// e rejeita imediatamente hashes divergentes (detecção de corrupção ou adulteração).
    /// </summary>
    [Fact]
    public void VerifyPackageIntegrity_ValidatesSha256Correctly()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "LSPK Optimizer Package Test Payload 2026");

            // Calcula o hash SHA-256 real do arquivo de teste
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(tempFile);
            string expectedHex = Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();

            var service = new UpdateService();

            // Assert 1: Hash correto deve retornar True
            Assert.True(service.VerifyPackageIntegrity(tempFile, expectedHex));

            // Assert 2: Hash adulterado/falso deve retornar False
            Assert.False(service.VerifyPackageIntegrity(tempFile, "0000000000000000000000000000000000000000000000000000000000000000"));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
