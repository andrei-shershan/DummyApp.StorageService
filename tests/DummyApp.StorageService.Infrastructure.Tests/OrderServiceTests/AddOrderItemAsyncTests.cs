using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class AddOrderItemAsyncTests : OrderServiceTestsBase
{
    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenIdsAreInvalid()
    {
        await using var context = CreateContext("AddOrderItemAsync_InvalidIds");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(Guid.Empty, Guid.NewGuid(), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenQuantityIsNotPositive()
    {
        await using var context = CreateContext("AddOrderItemAsync_InvalidQuantity");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), 0);

        Assert.False(result);
    }

    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenArtworkDoesNotExist()
    {
        await using var context = CreateContext("AddOrderItemAsync_ArtworkDoesNotExist");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task AddOrderItemAsync_CreatesOrderAndItem_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("OrderServiceTests_CreateOrderAndItem");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        await context.Artworks.AddAsync(artwork);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = new OrderService(context, loggerMock.Object);
        var orderId = Guid.NewGuid();

        var result = await service.AddOrderItemAsync(orderId, artwork.Id, 1);

        Assert.True(result);
        var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Single(order!.Items);
        Assert.Equal(1, order.Items.First().Quantity);
        Assert.Equal(artwork.Id, order.Items.First().ArtworkId);
    }

    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenOrderIsNotEditable()
    {
        await using var context = CreateContext("AddOrderItemAsync_OrderNotEditable");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Completed };
        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(orderId, artwork.Id, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenItemAlreadyExists()
    {
        await using var context = CreateContext("AddOrderItemAsync_ItemAlreadyExists");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        order.Items.Add(new OrderItem { OrderId = orderId, ArtworkId = artwork.Id, Quantity = 1 });

        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(orderId, artwork.Id, 1);

        Assert.False(result);
        var existingItem = await context.OrderItems.FirstOrDefaultAsync(i => i.OrderId == orderId && i.ArtworkId == artwork.Id);
        Assert.NotNull(existingItem);
        Assert.Equal(1, existingItem!.Quantity);
    }

    [Fact]
    public async Task AddOrderItemAsync_AddsNewItem_WhenOrderExistsAndEditable()
    {
        await using var context = CreateContext("AddOrderItemAsync_AddNewItemToExistingOrder");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Active };

        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.AddOrderItemAsync(orderId, artwork.Id, 1);

        Assert.True(result);
        var addedItem = await context.OrderItems.FirstOrDefaultAsync(i => i.OrderId == orderId && i.ArtworkId == artwork.Id);
        Assert.NotNull(addedItem);
        Assert.Equal(1, addedItem!.Quantity);
    }
}
