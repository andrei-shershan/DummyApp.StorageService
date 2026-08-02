using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public abstract class OrderServiceTestsBase
{
    protected static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }

    protected static OrderService CreateService(StorageDbContext context, Mock<ILogger<OrderService>> loggerMock)
        => new OrderService(context, loggerMock.Object);
}
