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
            EnsureDatabaseInitialized(db, app.Logger);
            db.Database.Migrate();
        }
        else
        {
            db.Database.EnsureCreated();
        }

        return app;
    }

    private static void EnsureDatabaseInitialized(StorageDbContext db, ILogger logger)
    {
        var retries = 10;
        while (true)
        {
            try
            {
                db.Database.OpenConnection();
                db.Database.CloseConnection();
                break;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (retries-- > 0)
            {
                logger.LogWarning(ex, "Database not ready, retrying in 10s… ({Retries} attempts left)", retries);
                Thread.Sleep(10000);
            }
        }
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
