using DummyApp.StorageService.Infrastructure.Models;

using DummyApp.StorageService.Data.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IOrderService
{
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity);
    Task<IEnumerable<OrderItemDto>> GetOrderItemsAsync(Guid orderId);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId);
    Task<OrderStatus?> GetOrderStatusAsync(Guid orderId);
    Task<bool> SetOrderStatusAsync(Guid orderId, OrderStatus status);
}
