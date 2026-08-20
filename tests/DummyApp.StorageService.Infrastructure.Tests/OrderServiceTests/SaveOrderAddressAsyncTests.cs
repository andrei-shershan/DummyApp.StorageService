using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.OrderServiceTests;

public sealed class SaveOrderAddressAsyncTests : OrderServiceTestsBase
{
    [Fact]
    public async Task SaveOrderAddressAsync_ReturnsFalse_WhenOrderIdIsInvalid()
    {
        await using var context = CreateContext("SaveOrderAddressAsync_InvalidOrderId");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.SaveOrderAddressAsync(Guid.Empty, new OrderAddressDto());

        Assert.False(result);
    }

    [Fact]
    public async Task SaveOrderAddressAsync_ReturnsFalse_WhenAddressIsNull()
    {
        await using var context = CreateContext("SaveOrderAddressAsync_AddressIsNull");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);

        var result = await service.SaveOrderAddressAsync(Guid.NewGuid(), null!);

        Assert.False(result);
    }

    [Fact]
    public async Task SaveOrderAddressAsync_CreatesOrderAndAddress_WhenOrderDoesNotExist()
    {
        await using var context = CreateContext("SaveOrderAddressAsync_CreatesOrderAndAddress");
        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);
        var orderId = Guid.NewGuid();
        var address = new OrderAddressDto { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" };

        var result = await service.SaveOrderAddressAsync(orderId, address);

        Assert.True(result);
        var order = await context.Orders.Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Address, order!.Status);
        Assert.Equal(address.Email, order.Email);
        Assert.NotNull(order.Address);
        Assert.Equal(address.Email, order.Address.Email);
    }

    [Fact]
    public async Task SaveOrderAddressAsync_UpdatesAddress_WhenOrderExists()
    {
        await using var context = CreateContext("SaveOrderAddressAsync_UpdatesAddress");
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Active, Address = new OrderAddress { OrderId = orderId, FirstName = "John", LastName = "Doe", Email = "old@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" } };
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<OrderService>>();
        var service = CreateService(context, loggerMock);
        var address = new OrderAddressDto { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Phone = "+48123123124", Country = "PL", City = "Krakow", Street = "Second", HouseNumber = "20", PostalCode = "30-002" };

        var result = await service.SaveOrderAddressAsync(orderId, address);

        Assert.True(result);
        var updatedOrder = await context.Orders.Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Address, updatedOrder!.Status);
        Assert.Equal(address.Email, updatedOrder.Email);
        Assert.Equal(address.Email, updatedOrder.Address!.Email);
        Assert.Equal(address.City, updatedOrder.Address.City);
    }
}
