using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
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

        return Ok(new OrderSummaryResponse { Items = summary.Items, Status = summary.Status, Address = summary.Address is not null ? new OrderAddressResponse
        {
            FirstName = summary.Address.FirstName,
            LastName = summary.Address.LastName,
            Phone = summary.Address.Phone,
            Email = summary.Address.Email,
            Country = summary.Address.Country,
            City = summary.Address.City,
            Street = summary.Address.Street,
            HouseNumber = summary.Address.HouseNumber,
            PostalCode = summary.Address.PostalCode
        } : null });
    }

    [HttpGet("{orderId}/address")]
    [ProducesResponseType(typeof(OrderAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderAddress([FromRoute] Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("OrderId is required.");
        }

        var address = await _orderService.GetOrderAddressAsync(orderId);
        if (address is null)
        {
            return NotFound();
        }

        return Ok(new OrderAddressResponse
        {
            FirstName = address.FirstName,
            LastName = address.LastName,
            Phone = address.Phone,
            Email = address.Email,
            Country = address.Country,
            City = address.City,
            Street = address.Street,
            HouseNumber = address.HouseNumber,
            PostalCode = address.PostalCode
        });
    }

    [HttpPost("{orderId}/address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveOrderAddress([FromRoute] Guid orderId, [FromBody] SaveOrderAddressRequest request)
    {
        if (orderId == Guid.Empty || request is null)
        {
            return BadRequest("OrderId and address are required.");
        }

        var address = new OrderAddressDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            Country = request.Country,
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            PostalCode = request.PostalCode
        };

        var result = await _orderService.SaveOrderAddressAsync(orderId, address);
        if (!result)
        {
            return BadRequest("Unable to save order address.");
        }

        return Ok();
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

