namespace DummyApp.StorageService.WebApi.Models;

public sealed class UpdateOrderItemRequest
{
    public int Quantity { get; set; }
    public int? PrintSizeId { get; set; }
    public int? PriceId { get; set; }
}
