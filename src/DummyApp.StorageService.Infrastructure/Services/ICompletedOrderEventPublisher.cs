using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface ICompletedOrderEventPublisher
{
    Task PublishAsync(Guid orderId, OrderSummaryDto orderSummary);
}
