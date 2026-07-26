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
        if (orderId == Guid.Empty || request is null || request.ArtworkId == Guid.Empty)
        {
            return BadRequest("Valid orderId and artworkId are required.");
        }

        var result = await _orderService.AddOrderItemAsync(orderId, request.ArtworkId, request.Quantity);
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

    [HttpPost("{orderId}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayOrder([FromRoute] Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("OrderId is required.");
        }

        var result = await _orderService.SetOrderStatusAsync(orderId, OrderStatus.Processing);
        if (!result)
        {
            return BadRequest("Unable to transition order to processing.");
        }

        return Ok();
    }
}
