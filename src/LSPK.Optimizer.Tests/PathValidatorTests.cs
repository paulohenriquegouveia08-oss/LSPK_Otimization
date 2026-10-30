using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Security;
using Xunit;

namespace LSPK.Optimizer.Tests;

/// <summary>
/// Suíte de testes unitários para o validador de caminhos (PathValidator).
/// Garante que nenhuma operação de exclusão atinja pastas do sistema, pastas pessoais ou raízes de partição.
/// </summary>
public class PathValidatorTests
{
    /// <summary>
    /// Testa se o validador bloqueia imediatamente raízes de unidades de disco (ex: C:\, D:\ ou /).
    /// </summary>
    [Fact]
    public void IsSafeToDelete_BlocksRootDirectories()
    {
        // Assert: Nenhuma raiz de partição pode ser permitida sob nenhuma categoria
        Assert.False(PathValidator.IsSafeToDelete("C:\\", JunkCategory.UserTemp));
        Assert.False(PathValidator.IsSafeToDelete("C:", JunkCategory.WindowsTemp));
        Assert.False(PathValidator.IsSafeToDelete("D:\\", JunkCategory.ShaderCache));
        Assert.False(PathValidator.IsSafeToDelete("/", JunkCategory.UserTemp));
    }

    /// <summary>
    /// Testa se pastas críticas do Windows (System32, SysWOW64) são invioláveis e bloqueadas.
    /// </summary>
    [Fact]
    public void IsSafeToDelete_BlocksCriticalSystemDirectories()
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string system32 = Path.Combine(winDir, "System32", "kernel32.dll");
        string syswow64 = Path.Combine(winDir, "SysWOW64", "cmd.exe");

        // Assert: Componentes essenciais do Windows devem retornar False
        Assert.False(PathValidator.IsSafeToDelete(system32, JunkCategory.WindowsTemp));
        Assert.False(PathValidator.IsSafeToDelete(syswow64, JunkCategory.WindowsTemp));
        Assert.False(PathValidator.IsSafeToDelete(system32, JunkCategory.UserTemp));
    }

    /// <summary>
    /// Testa se diretórios pessoais do usuário (Documentos, Downloads, Desktop) são terminantemente protegidos.
    /// </summary>
    [Fact]
    public void IsSafeToDelete_BlocksPersonalUserDirectories()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string docs = Path.Combine(userProfile, "Documents", "relatorio.docx");
        string downloads = Path.Combine(userProfile, "Downloads", "setup.exe");
        string desktop = Path.Combine(userProfile, "Desktop", "foto.png");

        // Assert: Arquivos pessoais de usuário nunca podem ser apagados pelo otimizador
        Assert.False(PathValidator.IsSafeToDelete(docs, JunkCategory.UserTemp));
        Assert.False(PathValidator.IsSafeToDelete(downloads, JunkCategory.UserTemp));
        Assert.False(PathValidator.IsSafeToDelete(desktop, JunkCategory.UserTemp));
    }

    /// <summary>
    /// Testa se arquivos legítimos situados na pasta %TEMP% do usuário são aceitos para limpeza.
    /// </summary>
    [Fact]
    public void IsSafeToDelete_AllowsValidUserTempFiles()
    {
        string tempDir = Path.GetTempPath();
        string tempFile = Path.Combine(tempDir, "lspk_test_temp_file.tmp");

        // Assert: Arquivo temporário válido dentro do Temp deve retornar True
        Assert.True(PathValidator.IsSafeToDelete(tempFile, JunkCategory.UserTemp));
    }

    /// <summary>
    /// Testa se tentativas maliciosas de Directory Traversal (ex: ..\..\Windows\System32)
    /// são normalizadas e devidamente bloqueadas.
    /// </summary>
    [Fact]
    public void IsSafeToDelete_BlocksDirectoryTraversalAttacks()
    {
        string tempDir = Path.GetTempPath();
        string maliciousPath = Path.Combine(tempDir, "..", "..", "Windows", "System32", "calc.exe");

        // Assert: O caminho resolvido cai em System32 e deve ser rejeitado
        Assert.False(PathValidator.IsSafeToDelete(maliciousPath, JunkCategory.UserTemp));
    }
}
