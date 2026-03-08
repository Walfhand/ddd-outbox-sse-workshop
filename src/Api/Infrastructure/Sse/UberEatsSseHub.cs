using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Api.Infrastructure.Sse;

public sealed record UberEatsNotification(
    Guid EventId,
    Guid OrderId,
    string EventType,
    string Message,
    DateTime OccurredAtUtc);

public interface IUberEatsNotificationStream
{
    IAsyncEnumerable<UberEatsNotification> Subscribe(
        string actorType,
        Guid actorId,
        CancellationToken cancellationToken);

    ValueTask PublishAsync(string actorType, Guid actorId, UberEatsNotification notification,
        CancellationToken cancellationToken);
}

public sealed class UberEatsNotificationStream : IUberEatsNotificationStream
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Channel<UberEatsNotification>>> _actors =
        new();

    public async IAsyncEnumerable<UberEatsNotification> Subscribe(
        string actorType,
        Guid actorId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var key = BuildKey(actorType, actorId);
        var subscriberId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<UberEatsNotification>();
        var subscribers = _actors.GetOrAdd(key, _ => new ConcurrentDictionary<Guid, Channel<UberEatsNotification>>());
        subscribers[subscriberId] = channel;

        try
        {
            await foreach (var notification in channel.Reader.ReadAllAsync(cancellationToken))
                yield return notification;
        }
        finally
        {
            if (_actors.TryGetValue(key, out var currentSubscribers))
            {
                currentSubscribers.TryRemove(subscriberId, out _);
                if (currentSubscribers.IsEmpty)
                    _actors.TryRemove(key, out _);
            }

            channel.Writer.TryComplete();
        }
    }

    public ValueTask PublishAsync(string actorType, Guid actorId, UberEatsNotification notification,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(actorType, actorId);
        if (!_actors.TryGetValue(key, out var subscribers))
            return ValueTask.CompletedTask;

        foreach (var (_, channel) in subscribers) channel.Writer.TryWrite(notification);
        return ValueTask.CompletedTask;
    }

    private static string BuildKey(string actorType, Guid actorId)
    {
        return $"{actorType}:{actorId:N}";
    }
}
