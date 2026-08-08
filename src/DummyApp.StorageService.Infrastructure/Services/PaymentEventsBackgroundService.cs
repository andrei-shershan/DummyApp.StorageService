using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class PaymentEventsBackgroundService : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusOptions _serviceBusOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentEventsBackgroundService> _logger;
    private ServiceBusProcessor? _processor;

    public PaymentEventsBackgroundService(
        ServiceBusClient serviceBusClient,
        IOptions<StorageServiceSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<PaymentEventsBackgroundService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceBusOptions = settings.Value.ServiceBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_serviceBusOptions.PaymentEventsQueueName))
        {
            _logger.LogWarning("ServiceBus:PaymentEventsQueueName is not configured. Payment event subscription will not start.");
            return;
        }

        _processor = _serviceBusClient.CreateProcessor(_serviceBusOptions.PaymentEventsQueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("Starting Service Bus processor for queue {QueueName}.", _serviceBusOptions.PaymentEventsQueueName);
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = GetMessageBody(message);
        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogInformation("Message {MessageId} body is empty. Application properties: {Properties}.", message.MessageId, message.ApplicationProperties);
            body = BuildMessageBodyFromProperties(message);
        }

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<PaymentEventHandler>();
        var handled = await handler.HandleMessageAsync(body);

        if (handled)
        {
            await args.CompleteMessageAsync(message);
        }
        else
        {
            await args.AbandonMessageAsync(message);
        }
    }

    private static string? BuildMessageBodyFromProperties(ServiceBusReceivedMessage message)
    {
        if (!message.ApplicationProperties.TryGetValue("OrderId", out var orderIdObj)
            || !message.ApplicationProperties.TryGetValue("SiteId", out var siteIdObj)
            || !message.ApplicationProperties.TryGetValue("PaymentStatus", out var paymentStatusObj)
            || !message.ApplicationProperties.TryGetValue("EventType", out var eventTypeObj))
        {
            return null;
        }

        var orderId = orderIdObj?.ToString();
        var siteId = siteIdObj?.ToString();
        var paymentStatus = paymentStatusObj?.ToString();
        var eventType = eventTypeObj?.ToString();

        if (string.IsNullOrWhiteSpace(orderId)
            || string.IsNullOrWhiteSpace(siteId)
            || string.IsNullOrWhiteSpace(paymentStatus)
            || string.IsNullOrWhiteSpace(eventType))
        {
            return null;
        }

        return $"{{\"OrderId\":\"{orderId}\",\"SiteId\":\"{siteId}\",\"PaymentStatus\":\"{paymentStatus}\",\"EventType\":\"{eventType}\"}}";
    }

    private static string? GetMessageBody(ServiceBusReceivedMessage message)
    {
        var bytes = message.Body?.ToArray();
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(bytes);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error on queue {QueueName}.", _serviceBusOptions.PaymentEventsQueueName);
        return Task.CompletedTask;
    }
}
