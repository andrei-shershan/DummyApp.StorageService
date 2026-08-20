using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private readonly StorageDbContext _dbContext;
    private readonly ILogger<VerificationCodeService> _logger;

    public VerificationCodeService(StorageDbContext dbContext, ILogger<VerificationCodeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> CreateVerificationCodeAsync(string email, string code, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            _logger.LogWarning("Invalid verification code request for email {Email}.", email);
            return false;
        }

        var verificationCode = new VerificationCode
        {
            Email = email.Trim().ToLowerInvariant(),
            Code = code,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.VerificationCodes.Add(verificationCode);
        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to persist verification code for email {Email}.", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while persisting verification code for email {Email}.", email);
            return false;
        }
    }

    public async Task<bool> VerifyVerificationCodeAsync(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            _logger.LogWarning("Invalid verification code verification request for email {Email}.", email);
            return false;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();

        var verificationCode = await _dbContext.VerificationCodes
            .Where(v => v.Email == normalizedEmail && v.Code == normalizedCode && !v.IsUsed && v.ExpiresAt >= DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();

        if (verificationCode is null)
        {
            _logger.LogWarning("Verification code validation failed for email {Email}.", email);
            return false;
        }

        verificationCode.IsUsed = true;

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to mark verification code used for email {Email}.", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while marking verification code used for email {Email}.", email);
            return false;
        }
    }
}
