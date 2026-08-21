using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddStorageServiceApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IArtworkService, ArtworkService>();
        services.AddScoped<IPrintSizeService, PrintSizeService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<ICompletedOrdersService, CompletedOrdersService>();
        services.AddOpenApi();
        return services;
    }
}
