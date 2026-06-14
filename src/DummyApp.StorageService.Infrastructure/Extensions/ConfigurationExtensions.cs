using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddStorageServiceKeyVault(this IConfigurationBuilder builder, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return builder;
        }

        var configuration = builder.Build();
        var keyVaultUrl = configuration[$"{nameof(StorageServiceSettings.KeyVault)}:{nameof(KeyVaultOptions.Url)}"];
        if (string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            return builder;
        }

        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var credential = string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(clientId);

        builder.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
        return builder;
    }
}
