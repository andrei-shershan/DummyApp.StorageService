using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class GetOrderAddressAsyncTests : OrderServiceTestsBase
{
    [Fact]
    public async Task GetOrderAddressAsync_ReturnsNull_WhenOrderIdIsInvalid()
    {
        await using var context = CreateContext("GetOrderAddressAsync_InvalidOrderId");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderAddressAsync(Guid.Empty);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderAddressAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("GetOrderAddressAsync_OrderDoesNotExist");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderAddressAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderAddressAsync_ReturnsAddress_WhenOrderHasAddress()
    {
        await using var context = CreateContext("GetOrderAddressAsync_ReturnsAddress");
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Active, Address = new OrderAddress { OrderId = orderId, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" } };

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.GetOrderAddressAsync(orderId);

        Assert.NotNull(result);
        Assert.Equal(order.Address.FirstName, result!.FirstName);
        Assert.Equal(order.Address.Email, result.Email);
    }
}
