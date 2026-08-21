using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class GetOrdersByEmailAsyncTests : OrderServiceTestsBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrdersByEmailAsync_ReturnsEmpty_WhenEmailIsInvalid(string? email)
    {
        await using var context = CreateContext("GetOrdersByEmailAsync_InvalidEmail");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrdersByEmailAsync(email!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrdersByEmailAsync_ReturnsEmpty_WhenNoOrdersExistForEmail()
    {
        await using var context = CreateContext("GetOrdersByEmailAsync_NoOrders");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrdersByEmailAsync("admin@example.com");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrdersByEmailAsync_ReturnsMappedSummaries_WhenOrdersExist()
    {
        await using var context = CreateContext("GetOrdersByEmailAsync_OrdersFound");
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();

        var artwork = new Artwork
        {
            Id = artworkId,
            Name = "Sunrise",
            Description = "A bright morning",
            ImgUrl = "https://example.com/sunrise.png",
            ThumbnailUrl = "https://example.com/sunrise-thumb.png"
        };

        var printSize = new PrintSize
        {
            Id = 1,
            Name = "Large"
        };

        var price = new Price
        {
            Id = 2,
            PrintSizeId = printSize.Id,
            Value = 19.99m,
            UpdatedAt = DateTime.UtcNow
        };

        var order = new Order
        {
            Id = orderId,
            Email = "admin@example.com",
            Status = OrderStatus.Completed,
            Address = new OrderAddress
            {
                OrderId = orderId,
                FirstName = "Jane",
                LastName = "Doe",
                Phone = "123-456-7890",
                Email = "admin@example.com",
                Country = "USA",
                City = "Seattle",
                Street = "1st Ave",
                HouseNumber = "101",
                PostalCode = "98101"
            }
        };

        var item = new OrderItem
        {
            OrderId = orderId,
            ArtworkId = artworkId,
            Quantity = 2,
            PrintSizeId = printSize.Id,
            PriceId = price.Id,
            PriceValue = price.Value,
            Artwork = artwork,
            PrintSize = printSize,
            Price = price
        };

        order.Items.Add(item);
        context.Artworks.Add(artwork);
        context.PrintSizes.Add(printSize);
        context.Prices.Add(price);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrdersByEmailAsync("admin@example.com");

        var summary = Assert.Single(result);
        Assert.Equal(orderId, summary.OrderId);
        Assert.Equal("Completed", summary.Status);
        Assert.Equal("admin@example.com", summary.Email);
        Assert.NotNull(summary.Address);
        Assert.Equal("Jane", summary.Address!.FirstName);
        Assert.Single(summary.Items);
        Assert.Equal(artworkId, summary.Items.Single().ArtworkId);
        Assert.Equal("Sunrise", summary.Items.Single().Name);
        Assert.Equal(19.99m, summary.Items.Single().PriceValue);
    }
}
