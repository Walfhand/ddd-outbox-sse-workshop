namespace Api.Infrastructure.Sse;

public static class SseStreamWriter
{
    public static async Task WriteAsync<T>(
        HttpContext httpContext,
        string connectedComment,
        IAsyncEnumerable<T> stream,
        Func<T, string> idSelector,
        Func<T, string> eventTypeSelector,
        Func<T, string> dataSelector)
    {
        var cancellationToken = httpContext.RequestAborted;
        var response = httpContext.Response;
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("Connection", "keep-alive");
        response.Headers.Append("X-Accel-Buffering", "no");
        response.ContentType = "text/event-stream";

        await response.WriteAsync($": {connectedComment}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);

        try
        {
            await foreach (var item in stream.WithCancellation(cancellationToken))
            {
                await response.WriteAsync($"id: {idSelector(item)}\n", cancellationToken);
                await response.WriteAsync($"event: {eventTypeSelector(item)}\n", cancellationToken);
                await response.WriteAsync($"data: {dataSelector(item)}\n\n", cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the client disconnects.
        }
    }
}
