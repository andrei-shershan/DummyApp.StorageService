using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageServiceSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageServiceSettings>().Bind(configuration);
        return services;
    }
}
