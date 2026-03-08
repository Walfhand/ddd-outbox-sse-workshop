using Engine.Core.Events;

namespace Api.Features.Notifications.Customers;

public sealed record OrderAcceptedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DateTime OccurredAtUtc) : IIntegrationEvent;
