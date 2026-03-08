using System.Text.Json;
using Api.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Workshop.UberEatsNaive;

public sealed class NaiveUberEatsNotificationStream
{
    public sealed record CustomerRequest
    {
        [FromRoute] public Guid CustomerId { get; set; }
    }

    public sealed record RestaurantRequest
    {
        [FromRoute] public Guid RestaurantId { get; set; }
    }

    public sealed class CustomerEndpoint()
        : GetMinimalEndpoint<CustomerRequest, IResult>("workshop/ubereats/stream/customers/{customerId:guid}");

    public sealed class RestaurantEndpoint()
        : GetMinimalEndpoint<RestaurantRequest, IResult>("workshop/ubereats/stream/restaurants/{restaurantId:guid}");

    public sealed class CustomerHandler(
        IHttpContextAccessor contextAccessor,
        INaiveUberEatsNotificationStream stream)
    {
        public async Task<IResult> Handle(CustomerRequest request, CancellationToken cancellationToken)
        {
            return await WriteStreamAsync(
                contextAccessor,
                stream,
                "customer",
                request.CustomerId,
                cancellationToken);
        }
    }

    public sealed class RestaurantHandler(
        IHttpContextAccessor contextAccessor,
        INaiveUberEatsNotificationStream stream)
    {
        public async Task<IResult> Handle(RestaurantRequest request, CancellationToken cancellationToken)
        {
            return await WriteStreamAsync(
                contextAccessor,
                stream,
                "restaurant",
                request.RestaurantId,
                cancellationToken);
        }
    }

    private static async Task<IResult> WriteStreamAsync(
        IHttpContextAccessor contextAccessor,
        INaiveUberEatsNotificationStream stream,
        string actorType,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext is null)
            return Results.Problem("HTTP context is not available.");

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
            // Client disconnected.
        }

        return Results.Empty;
    }
}
