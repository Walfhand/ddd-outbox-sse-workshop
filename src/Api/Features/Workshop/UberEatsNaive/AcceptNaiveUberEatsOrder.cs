using Api.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Workshop.UberEatsNaive;

public sealed class AcceptNaiveUberEatsOrder
{
    public sealed record Request
    {
        [FromRoute] public Guid OrderId { get; set; }
        [FromBody] public RequestBody Body { get; set; } = null!;

        public sealed record RequestBody
        {
            public Guid RestaurantId { get; set; }
            public bool ForceCustomerNotificationFailure { get; set; }
        }
    }

    public sealed record Response(
        bool Success,
        Guid OrderId,
        string? Status,
        bool CustomerNotificationSent,
        string? Error);

    public sealed class Endpoint()
        : PostMinimalEndpoint<Request, Response>("workshop/ubereats/orders-naive/{orderId:guid}/accept");

    public sealed class Handler(
        INaiveUberEatsOrderStore orderStore,
        INaiveUberEatsNotifier notifier,
        ILogger<Handler> logger)
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (!orderStore.TryAccept(request.OrderId, request.Body.RestaurantId, out var order, out var error))
                return new Response(false, request.OrderId, order?.Status.ToString(), false, error);

            var customerNotificationSent = true;
            try
            {
                // Intentional anti-pattern: direct synchronous notification after state transition.
                await notifier.NotifyCustomerOrderAcceptedAsync(
                    order!,
                    request.Body.ForceCustomerNotificationFailure,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Intentional anti-pattern: swallow notification error.
                logger.LogWarning(ex, "Customer notification failed for order {OrderId}.", order!.Id);
                customerNotificationSent = false;
            }

            return new Response(true, order!.Id, order.Status.ToString(), customerNotificationSent, null);
        }
    }
}
