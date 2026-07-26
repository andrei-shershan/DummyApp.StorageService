using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.WebApi.Models;

public sealed class OrderSummaryResponse
{
    public IEnumerable<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
    public string Status { get; set; } = string.Empty;
}
