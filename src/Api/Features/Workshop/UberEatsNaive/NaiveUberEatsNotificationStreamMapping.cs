using System.Text.Json;

namespace Api.Features.Workshop.UberEatsNaive;

public static class NaiveUberEatsNotificationStreamMapping
{
    public static WebApplication MapNaiveUberEatsStreams(this WebApplication app)
    {
        app.MapGet("/api/v1/workshop/ubereats/stream/customers/{customerId:guid}",
            async (Guid customerId, HttpContext httpContext, INaiveUberEatsNotificationStream stream) =>
            {
                await WriteStreamAsync(httpContext, stream, "customer", customerId);
            });

        app.MapGet("/api/v1/workshop/ubereats/stream/restaurants/{restaurantId:guid}",
            async (Guid restaurantId, HttpContext httpContext, INaiveUberEatsNotificationStream stream) =>
            {
                await WriteStreamAsync(httpContext, stream, "restaurant", restaurantId);
            });

        return app;
    }

    private static async Task WriteStreamAsync(HttpContext httpContext, INaiveUberEatsNotificationStream stream,
        string actorType, Guid actorId)
    {
        var cancellationToken = httpContext.RequestAborted;
        var response = httpContext.Response;
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("Connection", "keep-alive");
        response.Headers.Append("X-Accel-Buffering", "no");
        response.ContentType = "text/event-stream";

        await response.WriteAsync($": {actorType} stream connected\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);

        try
        {
            await foreach (var notification in stream.Subscribe(actorType, actorId, cancellationToken))
            {
                var data = JsonSerializer.Serialize(notification);
                await response.WriteAsync($"id: {notification.EventId}\n", cancellationToken);
                await response.WriteAsync($"event: {notification.EventType}\n", cancellationToken);
                await response.WriteAsync($"data: {data}\n\n", cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the client closes the SSE connection.
        }
    }
}
