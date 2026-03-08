using Api.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Workshop.UberEatsNaive;

public sealed class CreateNaiveUberEatsOrder
{
    public sealed record Request
    {
        [FromBody] public RequestBody Body { get; set; } = null!;

        public sealed record RequestBody
        {
            public Guid CustomerId { get; set; }
            public Guid RestaurantId { get; set; }
            public List<OrderItemDto> Items { get; set; } = [];
            public bool ForceRestaurantNotificationFailure { get; set; }
        }
    }

    public sealed record OrderItemDto(string Name, int Quantity);

    public sealed record Response(
        Guid OrderId,
        string Status,
        bool RestaurantNotificationSent,
        Guid CustomerId,
        Guid RestaurantId);

    public sealed class Endpoint()
        : PostMinimalEndpoint<Request, Response>("workshop/ubereats/orders-naive");

    public sealed class Handler(
        INaiveUberEatsOrderStore orderStore,
        INaiveUberEatsNotifier notifier,
        ILogger<Handler> logger)
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var items = request.Body.Items
                .Select(x => new NaiveUberEatsOrderLine(x.Name, x.Quantity))
                .ToList();

            var order = orderStore.CreateOrder(
                request.Body.CustomerId,
                request.Body.RestaurantId,
                items);

            var restaurantNotificationSent = true;
            try
            {
                // Intentional anti-pattern: direct notification call after write, no durable outbox.
                await notifier.NotifyRestaurantOrderPlacedAsync(
                    order,
                    request.Body.ForceRestaurantNotificationFailure,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Intentional anti-pattern: swallow failure, order remains created.
                logger.LogWarning(ex, "Restaurant notification failed for order {OrderId}.", order.Id);
                restaurantNotificationSent = false;
            }

            return new Response(
                order.Id,
                order.Status.ToString(),
                restaurantNotificationSent,
                order.CustomerId,
                order.RestaurantId);
        }
    }
}
