using System;
using System.Linq;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.StorageService.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication EnsureStorageServiceDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<StorageServiceSettings>>().Value;

        if (db.Database.IsRelational())
        {
            EnsureDatabaseInitialized(db, app.Logger);
            db.Database.Migrate();
        }
        else
        {
            db.Database.EnsureCreated();
        }

        if (settings.Infrastructure.Databases.Storage.SeedPricesAndSizes)
        {
            SeedPricesAndSizes(db, app.Logger);
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

    private static void SeedPricesAndSizes(StorageDbContext db, ILogger logger)
    {
        if (db.PrintSizes.Any() || db.Prices.Any())
        {
            return;
        }

        var sizes = new[]
        {
            new PrintSize { Name = "A1" },
            new PrintSize { Name = "A2" },
            new PrintSize { Name = "A3" },
            new PrintSize { Name = "A4" },
            new PrintSize { Name = "A6" }
        };

        db.PrintSizes.AddRange(sizes);
        db.SaveChanges();

        var prices = new[]
        {
            new Price { PrintSizeId = sizes[0].Id, Value = 100m, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
            new Price { PrintSizeId = sizes[1].Id, Value = 80m, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
            new Price { PrintSizeId = sizes[2].Id, Value = 60m, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
            new Price { PrintSizeId = sizes[3].Id, Value = 40m, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
            new Price { PrintSizeId = sizes[4].Id, Value = 10m, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
        };

        db.Prices.AddRange(prices);
        db.SaveChanges();
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
