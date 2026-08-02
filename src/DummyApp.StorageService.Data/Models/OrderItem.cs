using System;

namespace DummyApp.StorageService.Data.Models;

public sealed class OrderItem
{
    public Guid OrderId { get; set; }
    public Guid ArtworkId { get; set; }
    public int Quantity { get; set; }
    public int? PrintSizeId { get; set; }
    public int? PriceId { get; set; }
    public decimal? PriceValue { get; set; }

    public Order? Order { get; set; }
    public Artwork? Artwork { get; set; }
    public PrintSize? PrintSize { get; set; }
    public Price? Price { get; set; }
}
