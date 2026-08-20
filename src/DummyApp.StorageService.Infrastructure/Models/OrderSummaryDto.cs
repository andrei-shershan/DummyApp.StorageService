namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record OrderSummaryDto
{
    public IEnumerable<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
    public string Status { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrderAddressDto? Address { get; init; }
}
