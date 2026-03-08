using Api.Features.Notifications.Shared;

namespace Api.Features.Notifications.Customers;

public static class OrderAcceptedIntegrationEventHandler
{
    public static async Task Handle(
        OrderAcceptedIntegrationEvent @event,
        IUberEatsSseHub sseHub,
        CancellationToken cancellationToken)
    {
        var sent = await sseHub.PublishAsync(
            "customer",
            @event.CustomerId,
            new UberEatsNotification(
                @event.EventId,
                @event.OrderId,
                "order-accepted",
                $"Votre commande {@event.OrderId} est acceptee par le restaurant.",
                @event.OccurredAtUtc),
            cancellationToken);

        if (!sent)
        {
            await Task.Delay(1000, cancellationToken);
            throw new SseTargetOfflineException(
                $"No active customer SSE connection for {@event.CustomerId}. Trigger retry.");
        }
    }
}
