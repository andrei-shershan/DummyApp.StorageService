using System;
using System.Collections.Generic;
using DummyApp.StorageService.Infrastructure.Models;
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
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

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
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.AddOrderItem(Guid.NewGuid(), new AddOrderItemRequest { ArtworkId = Guid.NewGuid(), Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddOrderItem_ReturnsOk_WhenServiceSucceeds()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.AddOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.AddOrderItem(Guid.NewGuid(), new AddOrderItemRequest { ArtworkId = Guid.NewGuid(), Quantity = 1 });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateOrderItem_ReturnsBadRequest_WhenOrderIdOrArtworkIdIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

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
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.UpdateOrderItem(Guid.NewGuid(), Guid.NewGuid(), new UpdateOrderItemRequest { Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOrderItem_ReturnsOk_WhenServiceSucceeds()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.UpdateOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(true);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.UpdateOrderItem(Guid.NewGuid(), Guid.NewGuid(), new UpdateOrderItemRequest { Quantity = 1 });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsBadRequest_WhenTokenIsInvalid()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.GetCompletedOrders(Guid.Empty);

        Assert.IsType<BadRequestObjectResult>(result);
        completedOrdersService.Verify(x => x.GetEmailByTokenAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsNotFound_WhenTokenDoesNotResolveToEmail()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        completedOrdersService.Setup(x => x.GetEmailByTokenAsync(It.IsAny<Guid>()))
            .ReturnsAsync((string?)null);

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.GetCompletedOrders(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsOk_WhenOrdersAreFound()
    {
        var token = Guid.NewGuid();
        var email = "admin@example.com";
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        completedOrdersService.Setup(x => x.GetEmailByTokenAsync(token))
            .ReturnsAsync(email);
        orderService.Setup(x => x.GetOrdersByEmailAsync(email))
            .ReturnsAsync(new[]
            {
                new OrderSummaryDto
                {
                    OrderId = Guid.NewGuid(),
                    Status = "Completed",
                    Email = email,
                    Items = Array.Empty<OrderItemDto>()
                }
            });

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);
        var result = await controller.GetCompletedOrders(token);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var summaries = Assert.IsAssignableFrom<IEnumerable<OrderSummaryResponse>>(okResult.Value);
        var summary = Assert.Single(summaries);

        Assert.Equal(email, summary.Email);
        Assert.Equal("Completed", summary.Status);
        orderService.Verify(x => x.GetOrdersByEmailAsync(email), Times.Once);
    }
}
