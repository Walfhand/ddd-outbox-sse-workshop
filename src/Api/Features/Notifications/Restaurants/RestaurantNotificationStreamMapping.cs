using System.Text.Json;
using Api.Features.Notifications.Shared;
using Api.Infrastructure.Sse;

namespace Api.Features.Notifications.Restaurants;

public static class RestaurantNotificationStreamMapping
{
    public static WebApplication MapRestaurantNotificationStream(this WebApplication app)
    {
        app.MapGet("/api/v1/workshop/ubereats/stream/restaurants/{restaurantId:guid}",
            async (Guid restaurantId, HttpContext httpContext, IUberEatsSseHub hub) =>
            {
                await SseStreamWriter.WriteAsync(
                    httpContext,
                    "restaurant stream connected",
                    hub.Subscribe("restaurant", restaurantId, httpContext.RequestAborted),
                    x => x.EventId.ToString(),
                    x => x.EventType,
                    x => JsonSerializer.Serialize(x));
            });

        return app;
    }
}
