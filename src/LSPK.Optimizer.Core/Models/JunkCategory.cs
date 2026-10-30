namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Enumeração que define todas as categorias de arquivos desnecessários auditáveis pelo módulo de limpeza.
/// Cada categoria possui regras de caminhos exclusivas e critérios rígidos de elegibilidade.
/// </summary>
public enum JunkCategory
{
    /// <summary>
    /// Arquivos temporários gerados por aplicativos na pasta do usuário (%TEMP% / %LOCALAPPDATA%\Temp).
    /// Costumam acumular instaladores parciais, logs e dados descartáveis de sessão.
    /// </summary>
    UserTemp,

    /// <summary>
    /// Arquivos temporários criados por serviços do Windows e instaladores do sistema (%WINDIR%\Temp).
    /// </summary>
    WindowsTemp,

    /// <summary>
    /// Cache de miniaturas gerado pelo Windows Explorer para pré-visualização rápida de imagens e vídeos (thumbcache_*.db).
    /// Reconstruído automaticamente pelo Explorer quando necessário.
    /// </summary>
    ThumbnailCache,

    /// <summary>
    /// Caches compilados de shaders gerados por drivers gráficos (DirectX, NVIDIA, AMD).
    /// A limpeza remove versões antigas ou corrompidas de shaders de jogos e aplicativos 3D.
    /// </summary>
    ShaderCache,

    /// <summary>
    /// Relatórios antigos de erros, falhas de aplicativos e despejos de memória (Crash Dumps)
    /// armazenados pelo Windows Error Reporting (WER).
    /// </summary>
    ErrorReports,

    /// <summary>
    /// Pacotes e instaladores descompactados de atualizações antigas já aplicadas pelo Windows Update
    /// localizados em %WINDIR%\SoftwareDistribution\Download.
    /// </summary>
    WindowsUpdateDownload,

    /// <summary>
    /// Caches de navegação na web de navegadores homologados (Google Chrome, Microsoft Edge, Brave).
    /// Remove arquivos em disco de sites visitados sem apagar senhas, cookies de login ou histórico.
    /// </summary>
    BrowserCache,

    /// <summary>
    /// Arquivos descartados pelo usuário aguardando exclusão definitiva na Lixeira de todas as unidades.
    /// Requer confirmação explícita do usuário antes de ser esvaziada.
    /// </summary>
    RecycleBin
}
