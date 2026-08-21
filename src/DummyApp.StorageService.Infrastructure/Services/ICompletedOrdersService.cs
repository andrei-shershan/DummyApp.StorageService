using System;
using System.Threading.Tasks;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface ICompletedOrdersService
{
    Task<bool> CreateCompletedOrdersTokenAsync(string email, Guid token, DateTime expiresAt);
    Task<string?> GetEmailByTokenAsync(Guid token);
}
