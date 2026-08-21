using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.OrdersControllerTests;

public sealed class GetOrderSummaryTests
{
    [Fact]
    public async Task GetOrderSummary_ReturnsBadRequest_WhenOrderIdIsEmpty()
    {
        var orderService = new Mock<IOrderService>();
        var completedOrdersService = new Mock<ICompletedOrdersService>();
        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.GetOrderSummary(Guid.Empty);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("OrderId is required.", badRequest.Value);
        orderService.Verify(x => x.GetOrderSummaryAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderSummary_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderSummaryAsync(It.IsAny<Guid>())).ReturnsAsync((OrderSummaryDto?)null);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.GetOrderSummary(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetOrderSummary_ReturnsOk_WhenServiceReturnsSummary()
    {
        var orderId = Guid.NewGuid();
        var expected = new OrderSummaryDto
        {
            Items = new[]
            {
                new OrderItemDto
                {
                    OrderId = orderId,
                    ArtworkId = Guid.NewGuid(),
                    Quantity = 1,
                    Name = "Test",
                    Description = "Desc",
                    ImgUrl = "img",
                    ThumbnailUrl = "thumb"
                }
            },
            Status = "Active"
        };

        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderSummaryAsync(orderId)).ReturnsAsync(expected);
        var completedOrdersService = new Mock<ICompletedOrdersService>();

        var controller = new OrdersController(orderService.Object, completedOrdersService.Object);

        var result = await controller.GetOrderSummary(orderId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSummary = Assert.IsType<OrderSummaryResponse>(okResult.Value);
        Assert.Equal(expected.Status, returnedSummary.Status);
        Assert.Equal(expected.Email, returnedSummary.Email);
        Assert.Equal(expected.Items, returnedSummary.Items);
    }
}
