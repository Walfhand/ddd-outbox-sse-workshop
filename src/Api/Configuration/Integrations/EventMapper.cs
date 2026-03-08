using DomainEssentials.Core.Events;
using Engine.Core.Events;

namespace Api.Configuration.Integrations;

public class EventMapper : IEventMapper
{
    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        throw new NotImplementedException();
    }
}