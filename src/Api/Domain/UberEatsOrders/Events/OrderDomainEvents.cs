using DomainEssentials.Core.Events;

namespace Api.Domain.UberEatsOrders.Events;

public sealed record UberEatsOrderPlacedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record UberEatsOrderAcceptedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DateTime OccurredAtUtc) : IDomainEvent;
