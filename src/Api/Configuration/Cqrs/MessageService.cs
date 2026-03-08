using Wolverine;
using IMessage = QuickApi.Abstractions.Cqrs.IMessage;

namespace Api.Configuration.Cqrs;

internal sealed class MessageService(IMessageBus messageBus) : IMessage
{
    public async Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct = new())
    {
        return await messageBus.InvokeAsync<TResult>(message, ct);
    }

    public async Task InvokeAsync(object message, CancellationToken ct = new())
    {
        await messageBus.InvokeAsync(message, ct);
    }
}