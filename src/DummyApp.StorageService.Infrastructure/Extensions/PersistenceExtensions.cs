using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddStorageServicePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.Get<StorageServiceSettings>() ?? throw new InvalidOperationException("StorageServiceSettings is not configured.");
        var storageSettings = settings.Infrastructure.Databases.Storage;

        services.AddDbContext<StorageDbContext>(options =>
        {
            if (storageSettings.UseInMemory)
            {
                options.UseInMemoryDatabase("StorageDb");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(storageSettings.ConnectionString))
                {
                    throw new InvalidOperationException("Database connection string is required when UseInMemory is false.");
                }

                options.UseMySQL(storageSettings.ConnectionString, sqlOptions =>
                    sqlOptions.MigrationsAssembly("DummyApp.StorageService.Data"));
            }
        });

        return services;
    }
}
