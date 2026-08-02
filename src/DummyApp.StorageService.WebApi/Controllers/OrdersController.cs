using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("{orderId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrderItem([FromRoute] Guid orderId, [FromBody] AddOrderItemRequest request)
    {
        if (orderId == Guid.Empty || request is null || request.ArtworkId == Guid.Empty || request.Quantity <= 0)
        {
            return BadRequest("Valid orderId, artworkId and positive quantity are required.");
        }

        var result = await _orderService.AddOrderItemAsync(orderId, request.ArtworkId, request.Quantity, request.PrintSizeId, request.PriceId);
        if (!result)
        {
            return BadRequest("Unable to add order item.");
        }

        return Ok();
    }

    [HttpPatch("{orderId}/items/{artworkId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderItem([FromRoute] Guid orderId, [FromRoute] Guid artworkId, [FromBody] UpdateOrderItemRequest request)
    {
        if (orderId == Guid.Empty || artworkId == Guid.Empty || request is null || request.Quantity < 0)
        {
            return BadRequest("Valid orderId, artworkId and non-negative quantity are required.");
        }

        var result = await _orderService.UpdateOrderItemAsync(orderId, artworkId, request.Quantity, request.PrintSizeId, request.PriceId);
        if (!result)
        {
            return BadRequest("Unable to update order item.");
        }

        return Ok();
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderSummary([FromRoute] Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("OrderId is required.");
        }

        var summary = await _orderService.GetOrderSummaryAsync(orderId);
        if (summary is null)
        {
            return NotFound();
        }

        return Ok(new OrderSummaryResponse { Items = summary.Items, Status = summary.Status });
    }

    [HttpPost("{orderId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetOrderStatus([FromRoute] Guid orderId, [FromBody] SetOrderStatusRequest request)
    {
        if (orderId == Guid.Empty || request is null || string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest("OrderId and status are required.");
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            return BadRequest("Invalid order status.");
        }

        var result = await _orderService.SetOrderStatusAsync(orderId, status);
        if (!result)
        {
            return BadRequest("Unable to update order status.");
        }

        return Ok();
    }
}

