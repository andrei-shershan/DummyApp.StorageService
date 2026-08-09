using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.OrdersControllerTests;

public sealed class GetOrderAddressTests
{
    [Fact]
    public async Task GetOrderAddress_ReturnsBadRequest_WhenOrderIdIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object);

        var result = await controller.GetOrderAddress(Guid.Empty);

        Assert.IsType<BadRequestObjectResult>(result);
        orderService.Verify(x => x.GetOrderAddressAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderAddress_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var orderId = Guid.NewGuid();
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderAddressAsync(orderId)).ReturnsAsync((OrderAddressDto?)null);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.GetOrderAddress(orderId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetOrderAddress_ReturnsOk_WhenServiceReturnsAddress()
    {
        var orderId = Guid.NewGuid();
        var address = new OrderAddressDto { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" };
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderAddressAsync(orderId)).ReturnsAsync(address);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.GetOrderAddress(orderId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OrderAddressResponse>(okResult.Value);
        Assert.Equal(address.Email, response.Email);
    }
}
