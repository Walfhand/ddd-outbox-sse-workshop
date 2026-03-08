using Api.Features.Notifications.Shared;

namespace Api.Features.Notifications.Restaurants;

public static class OrderPlacedIntegrationEventHandler
{
    public static async Task Handle(
        OrderPlacedIntegrationEvent @event,
        IUberEatsSseHub sseHub,
        CancellationToken cancellationToken)
    {
        var sent = await sseHub.PublishAsync(
            "restaurant",
            @event.RestaurantId,
            new UberEatsNotification(
                @event.EventId,
                @event.OrderId,
                "order-placed",
                $"Nouvelle commande {@event.OrderId} a preparer.",
                @event.OccurredAtUtc),
            cancellationToken);

        if (!sent)
        {
            await Task.Delay(1000, cancellationToken);
            throw new SseTargetOfflineException(
                $"No active restaurant SSE connection for {@event.RestaurantId}. Trigger retry.");
        }
    }
}
