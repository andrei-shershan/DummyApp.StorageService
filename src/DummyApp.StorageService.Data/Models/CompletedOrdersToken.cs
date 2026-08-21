using System;

namespace DummyApp.StorageService.Data.Models;

public sealed class CompletedOrdersToken
{
    public int Id { get; set; }
    public Guid Token { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
