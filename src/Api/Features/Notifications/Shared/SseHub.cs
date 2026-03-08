using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Api.Features.Notifications.Shared;

public sealed record UberEatsNotification(
    Guid EventId,
    Guid OrderId,
    string EventType,
    string Message,
    DateTime OccurredAtUtc);

public interface IUberEatsSseHub
{
    IAsyncEnumerable<UberEatsNotification> Subscribe(
        string actorType,
        Guid actorId,
        CancellationToken cancellationToken);

    ValueTask<bool> PublishAsync(
        string actorType,
        Guid actorId,
        UberEatsNotification notification,
        CancellationToken cancellationToken);
}

public sealed class UberEatsSseHub : IUberEatsSseHub
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Channel<UberEatsNotification>>> _subscribers =
        new();

    public async IAsyncEnumerable<UberEatsNotification> Subscribe(
        string actorType,
        Guid actorId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var key = BuildKey(actorType, actorId);
        var streamId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<UberEatsNotification>();
        var channels = _subscribers.GetOrAdd(
            key,
            _ => new ConcurrentDictionary<Guid, Channel<UberEatsNotification>>());

        channels[streamId] = channel;

        try
        {
            await foreach (var notification in channel.Reader.ReadAllAsync(cancellationToken))
                yield return notification;
        }
        finally
        {
            if (_subscribers.TryGetValue(key, out var current))
            {
                current.TryRemove(streamId, out _);
                if (current.IsEmpty)
                    _subscribers.TryRemove(key, out _);
            }

            channel.Writer.TryComplete();
        }
    }

    public ValueTask<bool> PublishAsync(
        string actorType,
        Guid actorId,
        UberEatsNotification notification,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(actorType, actorId);
        if (!_subscribers.TryGetValue(key, out var channels) || channels.IsEmpty)
            return ValueTask.FromResult(false);

        var atLeastOne = false;
        foreach (var (_, channel) in channels)
            atLeastOne |= channel.Writer.TryWrite(notification);

        return ValueTask.FromResult(atLeastOne);
    }

    private static string BuildKey(string actorType, Guid actorId)
    {
        return $"{actorType}:{actorId:N}";
    }
}
