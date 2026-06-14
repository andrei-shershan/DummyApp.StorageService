using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication EnsureStorageServiceDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        if (db.Database.IsRelational())
        {
            var retries = 10;
            while (true)
            {
                try
                {
                    db.Database.Migrate();
                    break;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex) when (retries-- > 0)
                {
                    app.Logger.LogWarning("DB not ready ({Message}), retrying in 3s… ({Retries} left)", ex.Message, retries);
                    Thread.Sleep(3000);
                }
            }
        }
        else
        {
            db.Database.EnsureCreated();
        }

        return app;
    }

    public static WebApplication UseStorageServicePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        var settings = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageServiceSettings>>().Value;

        var forwardedOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        if (settings.ReverseProxy.TrustAllProxies)
        {
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
        }

        app.UseForwardedHeaders(forwardedOptions);
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        return app;
    }
}
