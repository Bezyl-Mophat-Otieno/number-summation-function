using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Company.Function.Services;

public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;

    public KeyVaultService(IConfiguration config, ILogger<KeyVaultService> logger)
    {
        _logger = logger;
        var keyVaultUrl = config["KeyVaultUrl"];

        if (string.IsNullOrEmpty(keyVaultUrl))
            throw new ArgumentNullException("KeyVaultUrl app setting is missing");

        _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve secret '{secretName}' from Key Vault.");
            return null;
        }
    }
}
