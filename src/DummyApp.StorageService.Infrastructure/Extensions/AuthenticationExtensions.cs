using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddStorageServiceAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.Get<StorageServiceSettings>() ?? throw new InvalidOperationException("StorageServiceSettings is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = settings.IdentityServer.Authority;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = settings.IdentityServer.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = settings.IdentityServer.Authority
                };
            });

        return services;
    }
}
