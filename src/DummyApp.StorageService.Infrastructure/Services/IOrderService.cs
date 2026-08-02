using DummyApp.StorageService.Infrastructure.Models;

using DummyApp.StorageService.Data.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IOrderService
{
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null);
    Task<bool> UpdateOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null);
    Task<IEnumerable<OrderItemDto>> GetOrderItemsAsync(Guid orderId);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId);
    Task<OrderStatus?> GetOrderStatusAsync(Guid orderId);
    Task<bool> SetOrderStatusAsync(Guid orderId, OrderStatus status);
}
