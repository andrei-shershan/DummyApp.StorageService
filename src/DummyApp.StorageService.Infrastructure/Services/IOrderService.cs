using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IOrderService
{
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity);
    Task<IEnumerable<OrderItemDto>> GetOrderItemsAsync(Guid orderId);
}
