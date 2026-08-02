using System;

namespace DummyApp.StorageService.Data.Models;

public sealed class Price
{
    public int Id { get; set; }
    public int PrintSizeId { get; set; }
    public PrintSize? PrintSize { get; set; }
    public decimal Value { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
