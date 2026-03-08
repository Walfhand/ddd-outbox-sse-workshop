using DomainEssentials.Core.Abstractions;
using Engine.Core.Events;
using Engine.EFCore;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;

namespace Engine.Wolverine;

public class DomainEventDispatcherMiddleWare
{
    public async Task After(IAppDbContextFactory dbContextFactory,
        IMessageBus messageBus,
        IHttpContextAccessor contextAccessor,
        IDbContextOutbox outbox,
        IEventMapper eventMapper,
        Envelope envelope)
    {
        if (contextAccessor.HttpContext is not null && contextAccessor.HttpContext!.Request.Method == "GET")
            return;
        var context = dbContextFactory.CreateDbContext();
        if (context is not AppDbContextBase dbContext)
            return;

        outbox.Enroll(dbContext);
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            while (true)
            {
                var entities = dbContext.ChangeTracker.Entries<IAggregateRoot>()
                    .Select(x => x.Entity)
                    .ToList();
                var events =
                    entities.SelectMany(x => x.ClearDomainEvents())
                        .ToList();
                if (events.Count != 0)
                {
                    foreach (var @event in events)
                    {
                        var integrationEvent = eventMapper.MapToIntegrationEvent(@event);
                        if (integrationEvent is not null)
                            await outbox.PublishAsync(integrationEvent);
                        else
                            await messageBus.InvokeAsync(@event);
                    }

                    continue;
                }

                break;
            }

            await outbox.SaveChangesAndFlushMessagesAsync();
        });
    }
}