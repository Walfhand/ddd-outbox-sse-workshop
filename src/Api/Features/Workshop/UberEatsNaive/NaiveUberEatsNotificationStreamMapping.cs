using System.Text.Json;
using Api.Infrastructure.Sse;

namespace Api.Features.Workshop.UberEatsNaive;

public static class NaiveUberEatsNotificationStreamMapping
{
    public static WebApplication MapNaiveUberEatsStreams(this WebApplication app)
    {
        app.MapGet("/api/v1/workshop/ubereats/stream/customers/{customerId:guid}",
            async (Guid customerId, HttpContext httpContext, IUberEatsNotificationStream stream) =>
            {
                await SseStreamWriter.WriteAsync(
                    httpContext,
                    "customer stream connected",
                    stream.Subscribe("customer", customerId, httpContext.RequestAborted),
                    x => x.EventId.ToString(),
                    x => x.EventType,
                    x => JsonSerializer.Serialize(x));
            });

        app.MapGet("/api/v1/workshop/ubereats/stream/restaurants/{restaurantId:guid}",
            async (Guid restaurantId, HttpContext httpContext, IUberEatsNotificationStream stream) =>
            {
                await SseStreamWriter.WriteAsync(
                    httpContext,
                    "restaurant stream connected",
                    stream.Subscribe("restaurant", restaurantId, httpContext.RequestAborted),
                    x => x.EventId.ToString(),
                    x => x.EventType,
                    x => JsonSerializer.Serialize(x));
            });

        return app;
    }
}
