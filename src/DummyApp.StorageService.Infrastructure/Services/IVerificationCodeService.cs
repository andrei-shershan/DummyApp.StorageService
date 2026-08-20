using DummyApp.StorageService.Data.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IVerificationCodeService
{
    Task<bool> CreateVerificationCodeAsync(string email, string code, DateTime expiresAt);
    Task<bool> VerifyVerificationCodeAsync(string email, string code);
}
