using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.VerificationCodeServiceTests;

public sealed class VerificationCodeServiceTests
{
    [Fact]
    public async Task CreateVerificationCodeAsync_ReturnsFalse_WhenRequestIsInvalid()
    {
        await using var context = CreateContext("CreateVerificationCodeAsync_Invalid");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var result = await service.CreateVerificationCodeAsync(string.Empty, "123456", DateTime.UtcNow.AddMinutes(10));

        Assert.False(result);
    }

    [Fact]
    public async Task CreateVerificationCodeAsync_ReturnsTrue_WhenRequestIsValid()
    {
        await using var context = CreateContext("CreateVerificationCodeAsync_Valid");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var result = await service.CreateVerificationCodeAsync("Admin@Example.com", "123456", DateTime.UtcNow.AddMinutes(10));

        Assert.True(result);
        var saved = await context.VerificationCodes.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("admin@example.com", saved!.Email);
        Assert.Equal("123456", saved.Code);
    }

    [Fact]
    public async Task VerifyVerificationCodeAsync_ReturnsFalse_WhenRequestIsInvalid()
    {
        await using var context = CreateContext("VerifyVerificationCodeAsync_Invalid");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var result = await service.VerifyVerificationCodeAsync("admin@example.com", "12345");

        Assert.False(result);
    }

    [Fact]
    public async Task VerifyVerificationCodeAsync_ReturnsFalse_WhenNoMatchingCodeExists()
    {
        await using var context = CreateContext("VerifyVerificationCodeAsync_NoMatch");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var result = await service.VerifyVerificationCodeAsync("admin@example.com", "123456");

        Assert.False(result);
    }

    [Fact]
    public async Task VerifyVerificationCodeAsync_ReturnsTrue_AndMarksCodeUsed_WhenMatchingCodeExists()
    {
        await using var context = CreateContext("VerifyVerificationCodeAsync_Match");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var code = new VerificationCode
        {
            Email = "admin@example.com",
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await context.VerificationCodes.AddAsync(code);
        await context.SaveChangesAsync();

        var result = await service.VerifyVerificationCodeAsync("Admin@Example.com", "123456");

        Assert.True(result);
        var saved = await context.VerificationCodes.FirstAsync();
        Assert.True(saved.IsUsed);
    }

    [Fact]
    public async Task VerifyVerificationCodeAsync_ReturnsFalse_WhenCodeIsExpired()
    {
        await using var context = CreateContext("VerifyVerificationCodeAsync_Expired");
        var loggerMock = new Mock<ILogger<VerificationCodeService>>();
        var service = new VerificationCodeService(context, loggerMock.Object);

        var code = new VerificationCode
        {
            Email = "admin@example.com",
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await context.VerificationCodes.AddAsync(code);
        await context.SaveChangesAsync();

        var result = await service.VerifyVerificationCodeAsync("admin@example.com", "123456");

        Assert.False(result);
    }

    private static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }
}
