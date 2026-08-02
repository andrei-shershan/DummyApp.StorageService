using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.OrdersControllerTests;

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task AddOrderItem_ReturnsBadRequest_WhenOrderIdOrArtworkIdIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object);

        var result = await controller.AddOrderItem(Guid.Empty, new AddOrderItemRequest { ArtworkId = Guid.Empty, Quantity = 0 });

        Assert.IsType<BadRequestObjectResult>(result);
        orderService.Verify(x => x.AddOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderItem_ReturnsBadRequest_WhenServiceFails()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.AddOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.AddOrderItem(Guid.NewGuid(), new AddOrderItemRequest { ArtworkId = Guid.NewGuid(), Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddOrderItem_ReturnsOk_WhenServiceSucceeds()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.AddOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.AddOrderItem(Guid.NewGuid(), new AddOrderItemRequest { ArtworkId = Guid.NewGuid(), Quantity = 1 });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateOrderItem_ReturnsBadRequest_WhenOrderIdOrArtworkIdIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object);

        var result = await controller.UpdateOrderItem(Guid.Empty, Guid.Empty, new UpdateOrderItemRequest { Quantity = -1 });

        Assert.IsType<BadRequestObjectResult>(result);
        orderService.Verify(x => x.UpdateOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderItem_ReturnsBadRequest_WhenServiceFails()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.UpdateOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(false);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.UpdateOrderItem(Guid.NewGuid(), Guid.NewGuid(), new UpdateOrderItemRequest { Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOrderItem_ReturnsOk_WhenServiceSucceeds()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.UpdateOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        var controller = new OrdersController(orderService.Object);
        var result = await controller.UpdateOrderItem(Guid.NewGuid(), Guid.NewGuid(), new UpdateOrderItemRequest { Quantity = 1 });

        Assert.IsType<OkResult>(result);
    }
}
