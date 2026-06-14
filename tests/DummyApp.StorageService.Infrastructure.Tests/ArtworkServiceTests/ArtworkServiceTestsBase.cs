using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DummyApp.StorageService.Infrastructure.Tests;

public abstract class ArtworkServiceTestsBase
{
    protected static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }

    protected static ArtworkService CreateService(StorageDbContext context, ILogger<ArtworkService> logger)
    {
        return new ArtworkService(context, logger);
    }
}
