using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.Infrastructure.Tests.PrintSizeServiceTests;

public abstract class PrintSizeServiceTestsBase
{
    protected static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }

    protected static PrintSizeService CreateService(StorageDbContext context)
    {
        return new PrintSizeService(context);
    }
}
