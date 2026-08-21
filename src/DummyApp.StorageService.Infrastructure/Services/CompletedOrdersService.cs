using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class CompletedOrdersService : ICompletedOrdersService
{
    private readonly StorageDbContext _dbContext;
    private readonly ILogger<CompletedOrdersService> _logger;

    public CompletedOrdersService(StorageDbContext dbContext, ILogger<CompletedOrdersService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> CreateCompletedOrdersTokenAsync(string email, Guid token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(email) || token == Guid.Empty || expiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid completed orders token request for email {Email}.", email);
            return false;
        }

        var completedOrdersToken = new CompletedOrdersToken
        {
            Email = email.Trim().ToLowerInvariant(),
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.CompletedOrdersTokens.Add(completedOrdersToken);

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to persist completed orders token for email {Email}.", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while persisting completed orders token for email {Email}.", email);
            return false;
        }
    }

    public async Task<string?> GetEmailByTokenAsync(Guid token)
    {
        if (token == Guid.Empty)
        {
            _logger.LogWarning("Invalid completed orders token supplied to GetEmailByTokenAsync.");
            return null;
        }

        var completedOrdersToken = await _dbContext.CompletedOrdersTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt >= DateTime.UtcNow);

        return completedOrdersToken?.Email;
    }
}
