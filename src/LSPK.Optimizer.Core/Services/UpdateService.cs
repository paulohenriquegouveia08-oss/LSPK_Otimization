using System.Security.Cryptography;
using System.Text.Json;
using LSPK.Optimizer.Core.Models;
using Serilog;

namespace LSPK.Optimizer.Core.Services;

/// <summary>
/// Resultado da consulta de verificação de atualizações.
/// </summary>
/// <param name="UpdateAvailable">Indica se uma nova versão compatível e superior foi detectada.</param>
/// <param name="Manifest">Objeto do manifesto remoto contendo URL de download e SHA-256.</param>
/// <param name="ErrorMessage">Mensagem de erro em caso de falha de conexão ou parsing.</param>
public record UpdateCheckResult(
    bool UpdateAvailable,
    UpdateManifest? Manifest,
    string? ErrorMessage = null
);

/// <summary>
/// Contrato de serviço para verificação, download em segundo plano e validação de integridade de atualizações.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Consulta o manifesto remoto e compara com a versão instalada.
    /// </summary>
    Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, string manifestUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza o download assíncrono do pacote ZIP em segundo plano e valida seu hash SHA-256.
    /// </summary>
    Task<string?> DownloadUpdatePackageAsync(UpdateManifest manifest, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se o hash SHA-256 de um arquivo físico coincide com o hash esperado do manifesto.
    /// </summary>
    bool VerifyPackageIntegrity(string filePath, string expectedSha256);
}

/// <summary>
/// Implementação do serviço de atualização automática.
/// Utiliza HttpClient para requisições HTTPS e SHA256 para integridade criptográfica.
/// </summary>
public class UpdateService : IUpdateService
{
    private static readonly ILogger Logger = Log.ForContext<UpdateService>();
    private readonly HttpClient _httpClient;

    public UpdateService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    /// <summary>
    /// Consulta o arquivo JSON estático do servidor remoto para verificar se existe nova versão estável.
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        string currentVersion,
        string manifestUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Information("Consultando manifesto de atualizações em: {Url}", manifestUrl);

            // Requisita o manifesto remoto via HTTPS
            var response = await _httpClient.GetStringAsync(manifestUrl, cancellationToken);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest == null)
            {
                return new UpdateCheckResult(false, null, "Manifesto inválido ou vazio.");
            }

            // Compara as versões semânticas (SemVer)
            if (!Version.TryParse(currentVersion, out var installedVer) ||
                !Version.TryParse(manifest.Version, out var remoteVer))
            {
                return new UpdateCheckResult(false, null, "Falha ao comparar versões (SemVer inválido).");
            }

            bool isNewer = remoteVer > installedVer;
            Logger.Information("Versão instalada: {Installed}, Versão remota: {Remote}. Nova versão disponível: {Available}",
                installedVer, remoteVer, isNewer);

            return new UpdateCheckResult(isNewer, manifest);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Não foi possível verificar atualizações no momento.");
            return new UpdateCheckResult(false, null, ex.Message);
        }
    }

    /// <summary>
    /// Baixa o pacote compactado de atualização (.zip) em fluxo de streaming para não sobrecarregar a memória.
    /// Após a gravação em disco, executa validação de integridade por hash SHA-256.
    /// </summary>
    public async Task<string?> DownloadUpdatePackageAsync(
        UpdateManifest manifest,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Diretório local para salvar o pacote baixado temporariamente
            string updatesDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LSPK Optimizer",
                "Updates",
                "Pending"
            );
            Directory.CreateDirectory(updatesDir);

            string targetFile = Path.Combine(updatesDir, $"LSPKOptimizerUpdate-{manifest.Version}.zip");

            // Se o arquivo já existe no cache local e a assinatura é válida, reutiliza para economizar banda
            if (File.Exists(targetFile) && VerifyPackageIntegrity(targetFile, manifest.Sha256))
            {
                Logger.Information("Pacote de atualização já baixado e verificado: {Path}", targetFile);
                progress?.Report(100.0);
                return targetFile;
            }

            Logger.Information("Iniciando download do pacote de atualização: {Url}", manifest.DownloadUrl);

            using var response = await _httpClient.GetAsync(manifest.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var buffer = new byte[81920]; // Buffer de 80 KB
            long totalRead = 0;
            int bytesRead;

            // Gravação progressiva com reporte percentual
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    double pct = (double)totalRead / totalBytes * 100.0;
                    progress?.Report(pct);
                }
            }

            await fileStream.FlushAsync(cancellationToken);
            fileStream.Close();

            // Validação estrita de integridade via SHA-256 (rejeita pacotes adulterados ou corrompidos)
            if (!VerifyPackageIntegrity(targetFile, manifest.Sha256))
            {
                Logger.Error("Falha de integridade: SHA-256 do pacote baixado não corresponde ao esperado.");
                try { File.Delete(targetFile); } catch { }
                return null;
            }

            Logger.Information("Download e validação SHA-256 concluídos com sucesso: {File}", targetFile);
            return targetFile;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Erro durante download do pacote de atualização");
            return null;
        }
    }

    /// <summary>
    /// Calcula o hash criptográfico SHA-256 do arquivo e compara com a assinatura informada no manifesto.
    /// </summary>
    public bool VerifyPackageIntegrity(string filePath, string expectedSha256)
    {
        if (!File.Exists(filePath) || string.IsNullOrWhiteSpace(expectedSha256)) return false;

        try
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            string computedHex = Convert.ToHexString(hash).ToLowerInvariant();
            string expectedHex = expectedSha256.Trim().ToLowerInvariant();

            return string.Equals(computedHex, expectedHex, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Erro ao calcular hash SHA-256 de {File}", filePath);
            return false;
        }
    }
}
