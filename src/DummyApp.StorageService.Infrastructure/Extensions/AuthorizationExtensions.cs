using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddStorageServiceAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("RequireStorageRead", policy =>
                policy.RequireClaim("scope", "storage.read").RequireAuthenticatedUser());

            options.AddPolicy("RequireStorageWrite", policy =>
                policy.RequireClaim("scope", "storage.write").RequireAuthenticatedUser());

            options.AddPolicy("CreateArtwork", policy =>
                policy.RequireAssertion(context =>
                    context.User.Identity?.IsAuthenticated == true &&
                    (context.User.HasClaim("role", "Creator") || context.User.HasClaim("scope", "storage.write"))));
        });

        return services;
    }
}
