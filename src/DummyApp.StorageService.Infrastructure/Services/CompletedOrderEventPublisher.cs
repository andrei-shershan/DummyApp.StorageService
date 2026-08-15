using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using DummyApp.StorageService.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class CompletedOrderEventPublisher : ICompletedOrderEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<CompletedOrderEventPublisher> _logger;

    public CompletedOrderEventPublisher(ServiceBusSender sender, ILogger<CompletedOrderEventPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task PublishAsync(Guid orderId, OrderSummaryDto orderSummary)
    {
        var body = JsonSerializer.Serialize(orderSummary);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/json",
            Subject = "completed-order-event"
        };

        message.ApplicationProperties["OrderId"] = orderId.ToString();
        message.ApplicationProperties["Status"] = orderSummary.Status;

        try
        {
            await _sender.SendMessageAsync(message);
            _logger.LogInformation("Published completed order event for order {OrderId}.", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish completed order event for order {OrderId}.", orderId);
            throw;
        }
    }
}
