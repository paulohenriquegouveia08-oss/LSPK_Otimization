using LSPK.Optimizer.Core.Models;

namespace LSPK.Optimizer.Core.Security;

/// <summary>
/// Validador de segurança rígido para caminhos de arquivos.
/// Atua como a primeira e principal linha de defesa contra exclusão acidental ou indevida de dados.
/// Implementa uma lista negra inviolável de pastas protegidas e uma lista branca estrita por categoria.
/// </summary>
public static class PathValidator
{
    /// <summary>
    /// Matriz de prefixos de caminhos de pastas críticas do Windows e perfis de usuários
    /// que NUNCA podem ser excluídos ou alterados sob nenhuma hipótese.
    /// </summary>
    private static readonly string[] ProtectedDirectoryPrefixes;

    /// <summary>
    /// Construtor estático responsável por inicializar dinamicamente os caminhos protegidos
    /// de acordo com as variáveis de ambiente e diretórios especiais da máquina em execução.
    /// </summary>
    static PathValidator()
    {
        var protectedList = new List<string>();

        // 1. Diretórios essenciais do Windows
        string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrEmpty(windir))
        {
            protectedList.Add(Path.Combine(windir, "System32"));
            protectedList.Add(Path.Combine(windir, "SysWOW64"));
            protectedList.Add(Path.Combine(windir, "WinSxS"));
            protectedList.Add(Path.Combine(windir, "Boot"));
            protectedList.Add(Path.Combine(windir, "System"));
        }

        // 2. Diretórios de instalação de programas (Arquivos de Programas)
        string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrEmpty(progFiles)) protectedList.Add(progFiles);

        string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrEmpty(progFilesX86)) protectedList.Add(progFilesX86);

        // 3. Pastas pessoais do perfil do usuário (Documentos, Downloads, Desktop, Fotos, etc.)
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
        {
            protectedList.Add(Path.Combine(userProfile, "Documents"));
            protectedList.Add(Path.Combine(userProfile, "Desktop"));
            protectedList.Add(Path.Combine(userProfile, "Downloads"));
            protectedList.Add(Path.Combine(userProfile, "Pictures"));
            protectedList.Add(Path.Combine(userProfile, "Music"));
            protectedList.Add(Path.Combine(userProfile, "Videos"));
            protectedList.Add(Path.Combine(userProfile, "OneDrive"));
        }

        // Normaliza todos os caminhos para evitar desvios com barras invertidas ou relativas
        ProtectedDirectoryPrefixes = protectedList
            .Select(NormalizePath)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    /// <summary>
    /// Resolve o caminho canônico absoluto, eliminando sequências como '..' ou '.',
    /// e padroniza as barras finais para comparação segura.
    /// </summary>
    /// <param name="path">Caminho a ser normalizado.</param>
    /// <returns>Caminho absoluto e normalizado, ou string vazia em caso de erro sintático.</returns>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        try
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Avalia se o arquivo ou diretório pode ser excluído com 100% de segurança física,
    /// garantindo que ele não viole as pastas protegidas e pertença à categoria informada.
    /// </summary>
    /// <param name="targetPath">Caminho do arquivo ou pasta candidata.</param>
    /// <param name="category">Categoria de limpeza sob a qual a operação está sendo solicitada.</param>
    /// <returns>Verdadeiro se o caminho for estritamente seguro; Falso caso contrário.</returns>
    public static bool IsSafeToDelete(string targetPath, JunkCategory category)
    {
        if (string.IsNullOrWhiteSpace(targetPath)) return false;

        string normalized = NormalizePath(targetPath);
        if (string.IsNullOrEmpty(normalized)) return false;

        // REGRA 1: Bloqueio imediato caso seja a raiz de qualquer partição (ex: C:\, D:\ ou /)
        var root = Path.GetPathRoot(normalized);
        if (string.Equals(normalized, root?.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // REGRA 2: Bloqueio estrito se o caminho estiver dentro de qualquer diretório da lista negra
        foreach (var protectedDir in ProtectedDirectoryPrefixes)
        {
            if (normalized.Equals(protectedDir, StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(protectedDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // REGRA 3: Validação de Whitelist específica por categoria
        return category switch
        {
            JunkCategory.UserTemp => IsInUserTemp(normalized),
            JunkCategory.WindowsTemp => IsInWindowsTemp(normalized),
            JunkCategory.ThumbnailCache => IsThumbnailCache(normalized),
            JunkCategory.ShaderCache => IsShaderCache(normalized),
            JunkCategory.ErrorReports => IsErrorReport(normalized),
            JunkCategory.WindowsUpdateDownload => IsWindowsUpdateDownload(normalized),
            JunkCategory.BrowserCache => IsBrowserCache(normalized),
            JunkCategory.RecycleBin => true, // Gerenciada exclusivamente via API nativa SHEmptyRecycleBin
            _ => false
        };
    }

    /// <summary>Valida se o caminho está estritamente contido na pasta %TEMP% do usuário.</summary>
    private static bool IsInUserTemp(string path)
    {
        string userTemp = NormalizePath(Path.GetTempPath());
        return !string.IsNullOrEmpty(userTemp) &&
               path.StartsWith(userTemp + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Valida se o caminho está estritamente dentro de %WINDIR%\Temp.</summary>
    private static bool IsInWindowsTemp(string path)
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string winTemp = NormalizePath(Path.Combine(winDir, "Temp"));
        return !string.IsNullOrEmpty(winTemp) &&
               path.StartsWith(winTemp + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Valida se o arquivo pertence aos bancos de miniaturas do Windows Explorer (thumbcache_*.db).</summary>
    private static bool IsThumbnailCache(string path)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string explorerDir = NormalizePath(Path.Combine(localAppData, "Microsoft", "Windows", "Explorer"));
        string fileName = Path.GetFileName(path);

        return path.StartsWith(explorerDir, StringComparison.OrdinalIgnoreCase) &&
               (fileName.StartsWith("thumbcache_", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("iconcache_", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Valida se o caminho pertence aos diretórios homologados de shader caches de GPUs.</summary>
    private static bool IsShaderCache(string path)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string d3d = NormalizePath(Path.Combine(localAppData, "D3DSCache"));
        string nvidia = NormalizePath(Path.Combine(localAppData, "NVIDIA", "DXCache"));
        string amd = NormalizePath(Path.Combine(localAppData, "AMD", "DxCache"));

        return (path.StartsWith(d3d, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(nvidia, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(amd, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Valida se o caminho pertence a dumps de travamentos de programas ou WER.</summary>
    private static bool IsErrorReport(string path)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string crashDumps = NormalizePath(Path.Combine(localAppData, "CrashDumps"));
        string progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string werDir = NormalizePath(Path.Combine(progData, "Microsoft", "Windows", "WER"));

        return (path.StartsWith(crashDumps, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(werDir, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Valida se o caminho está dentro da pasta de downloads temporários do Windows Update.</summary>
    private static bool IsWindowsUpdateDownload(string path)
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string swDist = NormalizePath(Path.Combine(winDir, "SoftwareDistribution", "Download"));

        return !string.IsNullOrEmpty(swDist) &&
               path.StartsWith(swDist + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Valida se o arquivo reside em pastas legítimas de cache dos navegadores homologados.</summary>
    private static bool IsBrowserCache(string path)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string chrome = NormalizePath(Path.Combine(localAppData, "Google", "Chrome", "User Data"));
        string edge = NormalizePath(Path.Combine(localAppData, "Microsoft", "Edge", "User Data"));
        string brave = NormalizePath(Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data"));

        bool isBrowser = path.StartsWith(chrome, StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith(edge, StringComparison.OrdinalIgnoreCase) ||
                         path.StartsWith(brave, StringComparison.OrdinalIgnoreCase);

        // Garante que a exclusão ocorre apenas dentro de subpastas de Cache e nunca no perfil/senhas
        return isBrowser && (path.Contains("Cache", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("Code Cache", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("GPUCache", StringComparison.OrdinalIgnoreCase));
    }
}
