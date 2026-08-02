namespace DummyApp.StorageService.Infrastructure.Options;

public sealed class StorageServiceSettings
{
    public IdentityServerOptions IdentityServer { get; set; } = new();
    public InfrastructureOptions Infrastructure { get; set; } = new();
    public KeyVaultOptions KeyVault { get; set; } = new();
    public ReverseProxyOptions ReverseProxy { get; set; } = new();
}

public sealed class IdentityServerOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public sealed class InfrastructureOptions
{
    public DatabaseOptions Databases { get; set; } = new();
}

public sealed class DatabaseOptions
{
    public StorageDatabaseOptions Storage { get; set; } = new();
}

public sealed class StorageDatabaseOptions
{
    public bool UseInMemory { get; set; } = true;
    public string ConnectionString { get; set; } = string.Empty;
    public bool SeedPricesAndSizes { get; set; }
}

public sealed class KeyVaultOptions
{
    public string Url { get; set; } = string.Empty;
}

public sealed class ReverseProxyOptions
{
    public bool TrustAllProxies { get; set; }
}
