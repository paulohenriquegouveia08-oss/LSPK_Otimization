namespace LSPK.Optimizer.Core.Models;

/// <summary>
/// Origens físicas e chaves de registro auditadas para carregamento de programas no boot do Windows.
/// </summary>
public enum StartupSource
{
    /// <summary>Chave de Registro do Usuário Atual: HKCU\Software\Microsoft\Windows\CurrentVersion\Run</summary>
    HkcuRun,

    /// <summary>Chave de Registro Global da Máquina (32-bit): HKLM\Software\Microsoft\Windows\CurrentVersion\Run</summary>
    HklmRun,

    /// <summary>Chave de Registro Global em ambiente 64-bit: HKLM\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run</summary>
    HklmRun64,

    /// <summary>Pasta Inicializar do perfil do usuário: %APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup</summary>
    UserStartupFolder,

    /// <summary>Pasta Inicializar comum para todos os usuários: %PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup</summary>
    CommonStartupFolder
}

/// <summary>
/// Classificação do impacto estimado do programa no tempo total de carregamento inicial do sistema.
/// </summary>
public enum StartupImpact
{
    /// <summary>Impacto baixo (utilitários leves, scripts rápidos).</summary>
    Low,

    /// <summary>Impacto médio (aplicativos com inicialização em segundo plano).</summary>
    Medium,

    /// <summary>Impacto alto (launchers pesados como Steam, Discord, Spotify ou clientes em nuvem).</summary>
    High,

    /// <summary>Impacto não determinado.</summary>
    Unknown
}

/// <summary>
/// Representa uma entrada individual de programa configurado para iniciar junto com o sistema operacional.
/// </summary>
public class StartupEntry
{
    /// <summary>Identificador único interno para rastreamento no ciclo de vida da interface.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>Nome do item exibido no registro ou nome do arquivo de atalho.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Fabricante ou desenvolvedor do executável (extraído dos metadados FileVersionInfo ou certificado digital).</summary>
    public string Publisher { get; init; } = "Desconhecido";

    /// <summary>Comando bruto original tal como gravado no registro (incluindo aspas e argumentos de inicialização).</summary>
    public string RawCommand { get; init; } = string.Empty;

    /// <summary>Caminho absoluto do arquivo executável resolvido e isolado dos argumentos.</summary>
    public string ExecutablePath { get; init; } = string.Empty;

    /// <summary>Argumentos adicionais passados ao executável no boot (ex: "--autostart", "--minimized").</summary>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>Origem de onde esta entrada foi carregada (Registro HKCU/HKLM ou pasta Startup).</summary>
    public StartupSource Source { get; init; }

    /// <summary>Indica se o item está ativo no momento ou se foi desativado com backup seguro.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Indica se o arquivo executável de destino realmente existe no disco físico.</summary>
    public bool TargetExists { get; init; } = true;

    /// <summary>Indica se a entrada pertence ao sistema operacional ou antivírus e não pode ser desativada.</summary>
    public bool IsSystemProtected { get; init; }

    /// <summary>Nível de impacto estimado no boot do computador.</summary>
    public StartupImpact Impact { get; init; } = StartupImpact.Medium;

    /// <summary>Descrição amigável em português da origem da entrada.</summary>
    public string SourceDescription => Source switch
    {
        StartupSource.HkcuRun => "Registro do Usuário (HKCU)",
        StartupSource.HklmRun => "Registro Global (HKLM 32-bit)",
        StartupSource.HklmRun64 => "Registro Global (HKLM 64-bit)",
        StartupSource.UserStartupFolder => "Pasta Inicializar do Usuário",
        StartupSource.CommonStartupFolder => "Pasta Inicializar Comum",
        _ => "Desconhecido"
    };

    /// <summary>
    /// Retorna verdadeiro se o item for órfão (o programa foi desinstalado, mas seu registro residual permaneceu).
    /// </summary>
    public bool IsOrphaned => !TargetExists;
}
