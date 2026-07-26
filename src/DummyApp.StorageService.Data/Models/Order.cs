using System;
using System.Collections.Generic;

namespace DummyApp.StorageService.Data.Models;

public enum OrderStatus
{
    Active,
    Processing,
    Completed
}

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Active;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
