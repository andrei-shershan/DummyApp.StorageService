using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.OrdersControllerTests;

public sealed class GetOrderItemsTests
{
    [Fact]
    public async Task GetOrderItems_ReturnsBadRequest_WhenOrderIdIsEmpty()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object);

        var result = await controller.GetOrderItems(Guid.Empty);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("OrderId is required.", badRequest.Value);
        orderService.Verify(x => x.GetOrderItemsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderItems_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderItemsAsync(It.IsAny<Guid>())).ReturnsAsync((IEnumerable<DummyApp.StorageService.Infrastructure.Models.OrderItemDto>?)null);

        var controller = new OrdersController(orderService.Object);

        var result = await controller.GetOrderItems(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetOrderItems_ReturnsOk_WhenServiceReturnsItems()
    {
        var orderId = Guid.NewGuid();
        var expected = new[]
        {
            new DummyApp.StorageService.Infrastructure.Models.OrderItemDto
            {
                OrderId = orderId,
                ArtworkId = Guid.NewGuid(),
                Quantity = 1,
                Name = "Test",
                Description = "Desc",
                ImgUrl = "img",
                ThumbnailUrl = "thumb"
            }
        };

        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrderItemsAsync(orderId)).ReturnsAsync(expected);

        var controller = new OrdersController(orderService.Object);

        var result = await controller.GetOrderItems(orderId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<DummyApp.StorageService.Infrastructure.Models.OrderItemDto>>(okResult.Value);
        Assert.Equal(expected, returnedItems);
    }
}
