using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddStorageServiceApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IArtworkService, ArtworkService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddOpenApi();
        return services;
    }
}
