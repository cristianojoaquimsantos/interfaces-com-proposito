using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using InterfacesComProposito.Application.Ports.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Security;

/// <summary>
/// Implementação de ISecretProvider utilizando Azure Key Vault e DefaultAzureCredential.
/// 
/// NOTA ARQUITETURAL:
/// Demonstra a recuperação segura de segredos de infraestrutura diretamente do Key Vault,
/// sem armazenar credenciais sensíveis em código ou em appsettings.json.
/// A camada Application consome apenas ISecretProvider.
/// </summary>
public sealed class AzureKeyVaultSecretProvider : ISecretProvider
{
    private readonly SecretClient? _secretClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureKeyVaultSecretProvider> _logger;
    private readonly bool _useAzure;

    public AzureKeyVaultSecretProvider(IConfiguration configuration, ILogger<AzureKeyVaultSecretProvider> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var vaultUri = _configuration["Azure:KeyVault:VaultUri"];

        if (!string.IsNullOrWhiteSpace(vaultUri) && Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri) && !vaultUri.Contains("YOUR_"))
        {
            _secretClient = new SecretClient(uri, new DefaultAzureCredential());
            _useAzure = true;
        }
        else
        {
            _useAzure = false;
            _logger.LogInformation("Azure Key Vault não configurado com URI válida. Utilizando provedor simulado/ambiente local.");
        }
    }

    public async Task<string?> ObterAsync(string nome, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        if (_useAzure && _secretClient is not null)
        {
            try
            {
                var response = await _secretClient.GetSecretAsync(nome, cancellationToken: cancellationToken);
                return response.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar secret '{Nome}' do Azure Key Vault.", nome);
                return null;
            }
        }

        // Recuperação de fallback em variáveis de ambiente para execução local autônoma
        return _configuration[nome] ?? Environment.GetEnvironmentVariable(nome);
    }
}
