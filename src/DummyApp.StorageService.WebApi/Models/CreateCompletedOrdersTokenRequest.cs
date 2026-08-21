using System;

namespace DummyApp.StorageService.WebApi.Models;

public sealed class CreateCompletedOrdersTokenRequest
{
    public string Email { get; init; } = string.Empty;
    public Guid Token { get; init; }
    public DateTime ExpiresAt { get; init; }
}
