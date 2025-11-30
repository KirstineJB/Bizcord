using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;


namespace ProfileService.Api.vault;

public class VaultHelper
{
    private readonly IVaultClient _client;
    private readonly VaultSettings _settings;

    public VaultHelper(VaultSettings settings)
    {
        _settings = settings;

        var authMethod = new TokenAuthMethodInfo(_settings.Token);
        var vaultClientSettings = new VaultClientSettings(_settings.Address, authMethod);

        _client = new VaultClient(vaultClientSettings);
    }

    public async Task<string> GetRabbitMqConnectionStringAsync()
    {
        Secret<SecretData> secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: _settings.RabbitMqSecretPath,
            mountPoint: _settings.MountPoint);

        var data = secret.Data.Data;
        return data["RabbitMqConnectionString"].ToString()!;
    }
}

    



