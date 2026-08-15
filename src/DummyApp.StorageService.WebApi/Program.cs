using Azure.Messaging.ServiceBus;
using DummyApp.StorageService.Infrastructure.Extensions;
using DummyApp.StorageService.Infrastructure.Options;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddStorageServiceKeyVault(builder.Environment);
builder.Services.AddStorageServiceSettings(builder.Configuration);

builder.Services.AddStorageServicePersistence(builder.Configuration);
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<StorageServiceSettings>>().Value;
    var serviceBusOptions = settings.ServiceBus;

    if (string.IsNullOrWhiteSpace(serviceBusOptions.ConnectionString))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");
    }

    return new ServiceBusClient(serviceBusOptions.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var serviceBusOptions = sp.GetRequiredService<IOptions<StorageServiceSettings>>().Value.ServiceBus;

    if (string.IsNullOrWhiteSpace(serviceBusOptions.CompletedOrderEventsQueueName))
    {
        throw new InvalidOperationException("ServiceBus:CompletedOrderEventsQueueName is not configured.");
    }

    var client = sp.GetRequiredService<ServiceBusClient>();
    return client.CreateSender(serviceBusOptions.CompletedOrderEventsQueueName);
});

builder.Services.AddScoped<ICompletedOrderEventPublisher, CompletedOrderEventPublisher>();

builder.Services.AddScoped<PaymentEventHandler>();
builder.Services.AddHostedService<PaymentEventsBackgroundService>();

builder.Services.AddStorageServiceAuthentication(builder.Configuration);
builder.Services.AddStorageServiceAuthorization();
builder.Services.AddStorageServiceApi();

var app = builder.Build();

app.EnsureStorageServiceDatabase()
   .UseStorageServicePipeline();

app.Run();
