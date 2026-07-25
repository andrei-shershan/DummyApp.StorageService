using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddStorageServiceAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
