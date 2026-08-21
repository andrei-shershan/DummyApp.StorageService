using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.WebApi.Models;

public sealed class OrderSummaryResponse
{
    public Guid OrderId { get; set; }
    public IEnumerable<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
    public string Status { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public OrderAddressResponse? Address { get; set; }
}
