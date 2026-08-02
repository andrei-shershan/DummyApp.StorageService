using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class UpdateOrderItemAsyncTests : OrderServiceTestsBase
{
    [Fact]
    public async Task UpdateOrderItemAsync_ReturnsFalse_WhenIdsAreInvalid()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_InvalidIds");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(Guid.Empty, Guid.NewGuid(), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ReturnsFalse_WhenQuantityIsNegative()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_NegativeQuantity");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), -1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ReturnsFalse_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_OrderDoesNotExist");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ReturnsFalse_WhenOrderIsNotEditable()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_OrderNotEditable");
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Completed };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(orderId, artworkId, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ReturnsFalse_WhenItemDoesNotExist()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_ItemDoesNotExist");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(orderId, artwork.Id, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_RemovesItem_WhenQuantityIsZero()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_RemoveItem");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        order.Items.Add(new OrderItem { OrderId = orderId, ArtworkId = artwork.Id, Quantity = 1 });
        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(orderId, artwork.Id, 0);

        Assert.True(result);
        var deletedItem = await context.OrderItems.FirstOrDefaultAsync(i => i.OrderId == orderId && i.ArtworkId == artwork.Id);
        Assert.Null(deletedItem);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_UpdatesQuantity_WhenItemExists()
    {
        await using var context = CreateContext("UpdateOrderItemAsync_UpdatesItem");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        order.Items.Add(new OrderItem { OrderId = orderId, ArtworkId = artwork.Id, Quantity = 1 });
        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.UpdateOrderItemAsync(orderId, artwork.Id, 5);

        Assert.True(result);
        var updatedItem = await context.OrderItems.FirstOrDefaultAsync(i => i.OrderId == orderId && i.ArtworkId == artwork.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(5, updatedItem!.Quantity);
    }
}
