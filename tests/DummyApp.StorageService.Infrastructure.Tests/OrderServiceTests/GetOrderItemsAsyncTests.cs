using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class GetOrderItemsAsyncTests
{
    [Fact]
    public async Task ReturnsEmpty_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("GetOrderItemsAsync_OrderDoesNotExist");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = new OrderService(context, loggerMock.Object);

        var result = await service.GetOrderItemsAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsItems_WhenOrderContainsOrderItems()
    {
        await using var context = CreateContext("GetOrderItemsAsync_ReturnsItems");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        var orderItem = new OrderItem { OrderId = orderId, ArtworkId = artwork.Id, Quantity = 2 };
        order.Items.Add(orderItem);

        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = new OrderService(context, loggerMock.Object);

        var result = await service.GetOrderItemsAsync(orderId);

        Assert.Single(result);
        var item = result.Single();
        Assert.Equal(orderId, item.OrderId);
        Assert.Equal(artwork.Id, item.ArtworkId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(artwork.Name, item.Name);
        Assert.Equal(artwork.Description, item.Description);
        Assert.Equal(artwork.ImgUrl, item.ImgUrl);
        Assert.Equal(artwork.ThumbnailUrl, item.ThumbnailUrl);
    }

    private static StorageDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StorageDbContext(options);
    }
}
