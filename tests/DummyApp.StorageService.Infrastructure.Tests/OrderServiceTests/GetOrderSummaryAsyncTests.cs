using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class GetOrderSummaryAsyncTests : OrderServiceTestsBase
{
    [Fact]
    public async Task GetOrderSummaryAsync_ReturnsNull_WhenOrderIdIsInvalid()
    {
        await using var context = CreateContext("GetOrderSummaryAsync_InvalidOrderId");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderSummaryAsync(Guid.Empty);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderSummaryAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("GetOrderSummaryAsync_OrderDoesNotExist");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderSummaryAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderSummaryAsync_ReturnsSummary_WhenOrderExists()
    {
        await using var context = CreateContext("GetOrderSummaryAsync_ReturnsSummary");
        var artwork = new Artwork { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "Desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "thumb", IsActive = true };
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Active };
        order.Items.Add(new OrderItem { OrderId = orderId, ArtworkId = artwork.Id, Quantity = 2 });

        await context.Artworks.AddAsync(artwork);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderSummaryAsync(orderId);

        Assert.NotNull(result);
        Assert.Equal(orderId, result!.Items.Single().OrderId);
        Assert.Equal(order.Status.ToString(), result.Status);
    }
}
