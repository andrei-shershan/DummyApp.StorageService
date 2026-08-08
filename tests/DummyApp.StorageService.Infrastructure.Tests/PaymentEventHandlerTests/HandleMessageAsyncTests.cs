using System;
using System.Text.Json;
using System.Threading.Tasks;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Options;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.PaymentEventHandlerTests;

public sealed class HandleMessageAsyncTests
{
    private readonly IOptions<StorageServiceSettings> _settings;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<ILogger<PaymentEventHandler>> _loggerMock;

    public HandleMessageAsyncTests()
    {
        var settings = new StorageServiceSettings
        {
            Application = new ApplicationOptions
            {
                SiteId = "local"
            },
            ServiceBus = new ServiceBusOptions()
        };

        _settings = Microsoft.Extensions.Options.Options.Create(settings);
        _orderServiceMock = new Mock<IOrderService>();
        _loggerMock = new Mock<ILogger<PaymentEventHandler>>();
    }

    [Fact]
    public async Task HandleMessageAsync_ReturnsTrueAndUpdatesOrder_WhenEventIsValid()
    {
        var paymentEvent = new PaymentEvent(Guid.NewGuid().ToString(), "local", "paid", "checkout.session.completed");
        var messageBody = JsonSerializer.Serialize(paymentEvent);
        _orderServiceMock
            .Setup(service => service.SetOrderStatusAsync(It.IsAny<Guid>(), OrderStatus.Completed))
            .ReturnsAsync(true);

        var handler = new PaymentEventHandler(_orderServiceMock.Object, _settings, _loggerMock.Object);

        var result = await handler.HandleMessageAsync(messageBody);

        Assert.True(result);
        _orderServiceMock.Verify(service => service.SetOrderStatusAsync(Guid.Parse(paymentEvent.OrderId), OrderStatus.Completed), Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ReturnsFalse_WhenSiteIdDoesNotMatch()
    {
        var paymentEvent = new PaymentEvent(Guid.NewGuid().ToString(), "other-site", "paid", "checkout.session.completed");
        var messageBody = JsonSerializer.Serialize(paymentEvent);
        var handler = new PaymentEventHandler(_orderServiceMock.Object, _settings, _loggerMock.Object);

        var result = await handler.HandleMessageAsync(messageBody);

        Assert.False(result);
        _orderServiceMock.Verify(service => service.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_ReturnsFalse_WhenEventIsNotCheckoutSessionCompleted()
    {
        var paymentEvent = new PaymentEvent(Guid.NewGuid().ToString(), "local", "paid", "payment.failed");
        var messageBody = JsonSerializer.Serialize(paymentEvent);
        var handler = new PaymentEventHandler(_orderServiceMock.Object, _settings, _loggerMock.Object);

        var result = await handler.HandleMessageAsync(messageBody);

        Assert.False(result);
        _orderServiceMock.Verify(service => service.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_ReturnsFalse_WhenBodyIsEmpty()
    {
        var handler = new PaymentEventHandler(_orderServiceMock.Object, _settings, _loggerMock.Object);

        var result = await handler.HandleMessageAsync(string.Empty);

        Assert.False(result);
        _orderServiceMock.Verify(service => service.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }
}
