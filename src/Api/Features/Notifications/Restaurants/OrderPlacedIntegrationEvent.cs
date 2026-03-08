using Engine.Core.Events;

namespace Api.Features.Notifications.Restaurants;

public sealed record OrderPlacedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DateTime OccurredAtUtc) : IIntegrationEvent;
