using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.OrdersControllerTests;

public sealed class SaveOrderAddressTests
{
    [Fact]
    public async Task SaveOrderAddress_ReturnsBadRequest_WhenOrderIdIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.SaveOrderAddress(Guid.Empty, new SaveOrderAddressRequest());

        Assert.IsType<BadRequestObjectResult>(result);
        orderService.Verify(x => x.SaveOrderAddressAsync(It.IsAny<Guid>(), It.IsAny<OrderAddressDto>()), Times.Never);
    }

    [Fact]
    public async Task SaveOrderAddress_ReturnsBadRequest_WhenRequestIsNull()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.SaveOrderAddress(Guid.NewGuid(), null!);

        Assert.IsType<BadRequestObjectResult>(result);
        orderService.Verify(x => x.SaveOrderAddressAsync(It.IsAny<Guid>(), It.IsAny<OrderAddressDto>()), Times.Never);
    }

    [Fact]
    public async Task SaveOrderAddress_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var orderId = Guid.NewGuid();
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.SaveOrderAddressAsync(orderId, It.IsAny<OrderAddressDto>())).ReturnsAsync(false);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.SaveOrderAddress(orderId, new SaveOrderAddressRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SaveOrderAddress_ReturnsOk_WhenServiceReturnsTrue()
    {
        var orderId = Guid.NewGuid();
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.SaveOrderAddressAsync(orderId, It.IsAny<OrderAddressDto>())).ReturnsAsync(true);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.SaveOrderAddress(orderId, new SaveOrderAddressRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" });

        Assert.IsType<OkResult>(result);
        orderService.Verify(x => x.SaveOrderAddressAsync(orderId, It.Is<OrderAddressDto>(a => a.FirstName == "John" && a.Email == "john.doe@example.com")), Times.Once);
    }
}
