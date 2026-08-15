using System;
using System.Text.Json;
using System.Threading.Tasks;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class PaymentEventHandler
{
    private readonly IOrderService _orderService;
    private readonly ICompletedOrderEventPublisher _completedOrderEventPublisher;
    private readonly ApplicationOptions _applicationOptions;
    private readonly ILogger<PaymentEventHandler> _logger;

    public PaymentEventHandler(
        IOrderService orderService,
        ICompletedOrderEventPublisher completedOrderEventPublisher,
        IOptions<StorageServiceSettings> settings,
        ILogger<PaymentEventHandler> logger)
    {
        _orderService = orderService;
        _completedOrderEventPublisher = completedOrderEventPublisher;
        var configured = settings.Value;
        _applicationOptions = configured.Application;
        _logger = logger;
    }

    public async Task<bool> HandleMessageAsync(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Ignoring payment event because message body is empty or missing.");
            return false;
        }

        PaymentEvent paymentEvent;
        try
        {
            System.Console.WriteLine($"PaymentEventHandler.HandleMessageAsync: Received message body: {body}");
            paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Payment event message body could not be deserialized.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize payment event message.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment event message.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_applicationOptions.SiteId))
        {
            _logger.LogWarning("Ignoring payment event for order {OrderId} because application site id is not configured.", paymentEvent.OrderId);
            return false;
        }

        if (!string.Equals(paymentEvent.SiteId, _applicationOptions.SiteId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Ignoring payment event for order {OrderId} because site id '{EventSiteId}' does not match configured application site id '{ConfiguredSiteId}'.", paymentEvent.OrderId, paymentEvent.SiteId, _applicationOptions.SiteId);
            return false;
        }

        if (!string.Equals(paymentEvent.EventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Ignoring payment event for order {OrderId} because event type is '{EventType}'.", paymentEvent.OrderId, paymentEvent.EventType);
            return false;
        }

        if (!IsSuccessfulPaymentStatus(paymentEvent.PaymentStatus))
        {
            _logger.LogInformation("Ignoring payment event for order {OrderId} because payment status is '{PaymentStatus}'.", paymentEvent.OrderId, paymentEvent.PaymentStatus);
            return false;
        }

        if (!Guid.TryParse(paymentEvent.OrderId, out var orderId))
        {
            _logger.LogWarning("Ignoring payment event because order id '{OrderId}' is not a valid GUID.", paymentEvent.OrderId);
            return false;
        }

        var result = await _orderService.SetOrderStatusAsync(orderId, Data.Models.OrderStatus.Completed);

        if (!result)
        {
            _logger.LogWarning("Payment event for order {OrderId} did not update order status.", paymentEvent.OrderId);
            return false;
        }

        var orderSummary = await _orderService.GetOrderSummaryAsync(orderId);
        if (orderSummary is null)
        {
            _logger.LogError("Order summary for order {OrderId} could not be loaded after status update.", paymentEvent.OrderId);
            return false;
        }

        try
        {
            await _completedOrderEventPublisher.PublishAsync(orderId, orderSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish completed order event for order {OrderId}.", paymentEvent.OrderId);
            return false;
        }

        _logger.LogInformation("Order {OrderId} status updated to Completed and completed order event sent.", paymentEvent.OrderId);
        return true;
    }

    private static bool IsSuccessfulPaymentStatus(string paymentStatus)
        => !string.IsNullOrWhiteSpace(paymentStatus)
            && (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "complete", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "succeeded", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "success", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "completed", StringComparison.OrdinalIgnoreCase));
}
