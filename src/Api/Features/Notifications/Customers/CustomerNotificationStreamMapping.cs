using System.Text.Json;
using Api.Features.Notifications.Shared;
using Api.Infrastructure.Sse;

namespace Api.Features.Notifications.Customers;

public static class CustomerNotificationStreamMapping
{
    public static WebApplication MapCustomerNotificationStream(this WebApplication app)
    {
        app.MapGet("/api/v1/workshop/ubereats/stream/customers/{customerId:guid}",
            async (Guid customerId, HttpContext httpContext, IUberEatsSseHub hub) =>
            {
                await SseStreamWriter.WriteAsync(
                    httpContext,
                    "customer stream connected",
                    hub.Subscribe("customer", customerId, httpContext.RequestAborted),
                    x => x.EventId.ToString(),
                    x => x.EventType,
                    x => JsonSerializer.Serialize(x));
            });

        return app;
    }
}
