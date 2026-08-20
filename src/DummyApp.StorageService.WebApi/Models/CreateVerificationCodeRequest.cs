namespace DummyApp.StorageService.WebApi.Models;

public sealed class CreateVerificationCodeRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
