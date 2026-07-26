namespace DummyApp.StorageService.WebApi.Models;

public sealed class AddOrderItemRequest
{
    public Guid ArtworkId { get; set; }
    public int Quantity { get; set; }
}
