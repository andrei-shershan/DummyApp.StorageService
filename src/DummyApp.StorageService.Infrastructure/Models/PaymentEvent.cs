namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record PaymentEvent(
    string OrderId,
    string SiteId,
    string PaymentStatus,
    string EventType
);
