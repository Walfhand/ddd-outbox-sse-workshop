using Api.Domain.UberEatsOrders.Events;
using Api.Features.Notifications.Customers;
using Api.Features.Notifications.Restaurants;
using DomainEssentials.Core.Events;
using Engine.Core.Events;

namespace Api.Configuration.Integrations;

public class EventMapper : IEventMapper
{
    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            UberEatsOrderPlacedDomainEvent @event => new OrderPlacedIntegrationEvent(
                Guid.NewGuid(),
                @event.OrderId,
                @event.CustomerId,
                @event.RestaurantId,
                @event.OccurredAtUtc),
            UberEatsOrderAcceptedDomainEvent @event => new OrderAcceptedIntegrationEvent(
                Guid.NewGuid(),
                @event.OrderId,
                @event.CustomerId,
                @event.RestaurantId,
                @event.OccurredAtUtc),
            _ => null
        };
    }
}
