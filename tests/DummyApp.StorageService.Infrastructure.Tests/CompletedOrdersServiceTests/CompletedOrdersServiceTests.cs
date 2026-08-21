using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.CompletedOrdersServiceTests;

public sealed class CompletedOrdersServiceTests
{
    [Fact]
    public async Task CreateCompletedOrdersTokenAsync_ReturnsFalse_WhenRequestIsInvalid()
    {
        await using var context = CreateContext("CreateCompletedOrdersTokenAsync_Invalid");
        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var result = await service.CreateCompletedOrdersTokenAsync(string.Empty, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        Assert.False(result);
    }

    [Fact]
    public async Task CreateCompletedOrdersTokenAsync_ReturnsFalse_WhenExpirationIsNotFuture()
    {
        await using var context = CreateContext("CreateCompletedOrdersTokenAsync_NonFutureExpiration");
        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var result = await service.CreateCompletedOrdersTokenAsync("admin@example.com", Guid.NewGuid(), DateTime.UtcNow.AddSeconds(-1));

        Assert.False(result);
    }

    [Fact]
    public async Task CreateCompletedOrdersTokenAsync_ReturnsTrue_WhenRequestIsValid()
    {
        await using var context = CreateContext("CreateCompletedOrdersTokenAsync_Valid");
        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var token = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var result = await service.CreateCompletedOrdersTokenAsync("Admin@Example.com", token, expiresAt);

        Assert.True(result);

        var saved = await context.CompletedOrdersTokens.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("admin@example.com", saved!.Email);
        Assert.Equal(token, saved.Token);
        Assert.Equal(expiresAt, saved.ExpiresAt);
    }

    private static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }
}
