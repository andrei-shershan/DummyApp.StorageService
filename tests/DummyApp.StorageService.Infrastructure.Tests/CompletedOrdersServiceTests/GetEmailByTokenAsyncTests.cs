using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.CompletedOrdersServiceTests;

public sealed class GetEmailByTokenAsyncTests
{
    [Fact]
    public async Task GetEmailByTokenAsync_ReturnsNull_WhenTokenIsEmpty()
    {
        await using var context = CreateContext("GetEmailByTokenAsync_EmptyToken");
        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var result = await service.GetEmailByTokenAsync(Guid.Empty);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEmailByTokenAsync_ReturnsNull_WhenTokenHasExpired()
    {
        await using var context = CreateContext("GetEmailByTokenAsync_ExpiredToken");
        context.CompletedOrdersTokens.Add(new CompletedOrdersToken
        {
            Email = "admin@example.com",
            Token = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var result = await service.GetEmailByTokenAsync(context.CompletedOrdersTokens.First().Token);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEmailByTokenAsync_ReturnsEmail_WhenTokenIsValid()
    {
        await using var context = CreateContext("GetEmailByTokenAsync_ValidToken");
        var token = Guid.NewGuid();
        context.CompletedOrdersTokens.Add(new CompletedOrdersToken
        {
            Email = "admin@example.com",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<CompletedOrdersService>>();
        var service = new CompletedOrdersService(context, loggerMock.Object);

        var result = await service.GetEmailByTokenAsync(token);

        Assert.Equal("admin@example.com", result);
    }

    private static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }
}
