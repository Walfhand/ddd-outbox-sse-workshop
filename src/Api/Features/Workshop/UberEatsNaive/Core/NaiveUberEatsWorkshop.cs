using System.Collections.Concurrent;
using Api.Infrastructure.Sse;

namespace Api.Features.Workshop.UberEatsNaive;

public enum NaiveUberEatsOrderStatus
{
    PendingRestaurantAcceptance = 0,
    AcceptedByRestaurant = 1,
    RejectedByRestaurant = 2
}

public sealed record NaiveUberEatsOrderLine(string Name, int Quantity);

public sealed class NaiveUberEatsOrder
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid RestaurantId { get; init; }
    public List<NaiveUberEatsOrderLine> Items { get; init; } = [];
    public NaiveUberEatsOrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? AcceptedAtUtc { get; set; }
}

public interface INaiveUberEatsOrderStore
{
    NaiveUberEatsOrder CreateOrder(Guid customerId, Guid restaurantId, IReadOnlyCollection<NaiveUberEatsOrderLine> items);
    bool TryGet(Guid orderId, out NaiveUberEatsOrder? order);
    bool TryAccept(Guid orderId, Guid restaurantId, out NaiveUberEatsOrder? order, out string? error);
}

public sealed class NaiveUberEatsOrderStore : INaiveUberEatsOrderStore
{
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<Guid, NaiveUberEatsOrder> _orders = new();

    public NaiveUberEatsOrder CreateOrder(Guid customerId, Guid restaurantId, IReadOnlyCollection<NaiveUberEatsOrderLine> items)
    {
        var order = new NaiveUberEatsOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            RestaurantId = restaurantId,
            Items = items.ToList(),
            Status = NaiveUberEatsOrderStatus.PendingRestaurantAcceptance,
            CreatedAtUtc = DateTime.UtcNow
        };

        _orders[order.Id] = order;
        return order;
    }

    public bool TryGet(Guid orderId, out NaiveUberEatsOrder? order)
    {
        return _orders.TryGetValue(orderId, out order);
    }

    public bool TryAccept(Guid orderId, Guid restaurantId, out NaiveUberEatsOrder? order, out string? error)
    {
        lock (_sync)
        {
            if (!_orders.TryGetValue(orderId, out order))
            {
                error = "Order not found.";
                return false;
            }

            if (order.RestaurantId != restaurantId)
            {
                error = "Order does not belong to this restaurant.";
                return false;
            }

            if (order.Status != NaiveUberEatsOrderStatus.PendingRestaurantAcceptance)
            {
                error = "Order is no longer pending.";
                return false;
            }

            order.Status = NaiveUberEatsOrderStatus.AcceptedByRestaurant;
            order.AcceptedAtUtc = DateTime.UtcNow;
            error = null;
            return true;
        }
    }
}

public interface INaiveUberEatsNotifier
{
    Task NotifyRestaurantOrderPlacedAsync(NaiveUberEatsOrder order, bool forceFailure, CancellationToken cancellationToken);
    Task NotifyCustomerOrderAcceptedAsync(NaiveUberEatsOrder order, bool forceFailure, CancellationToken cancellationToken);
}

public sealed class NaiveUberEatsNotifier(
    IUberEatsNotificationStream stream,
    ILogger<NaiveUberEatsNotifier> logger) : INaiveUberEatsNotifier
{
    public async Task NotifyRestaurantOrderPlacedAsync(
        NaiveUberEatsOrder order,
        bool forceFailure,
        CancellationToken cancellationToken)
    {
        await Task.Delay(80, cancellationToken);

        if (forceFailure)
            throw new InvalidOperationException("Restaurant notification provider is down.");

        await stream.PublishAsync(
            "restaurant",
            order.RestaurantId,
            new UberEatsNotification(
                Guid.NewGuid(),
                order.Id,
                "order-placed",
                $"Nouvelle commande {order.Id} a preparer.",
                DateTime.UtcNow),
            cancellationToken);

        logger.LogInformation("Restaurant notified for order {OrderId}.", order.Id);
    }

    public async Task NotifyCustomerOrderAcceptedAsync(
        NaiveUberEatsOrder order,
        bool forceFailure,
        CancellationToken cancellationToken)
    {
        await Task.Delay(80, cancellationToken);

        if (forceFailure)
            throw new InvalidOperationException("Customer notification provider is down.");

        await stream.PublishAsync(
            "customer",
            order.CustomerId,
            new UberEatsNotification(
                Guid.NewGuid(),
                order.Id,
                "order-accepted",
                $"Votre commande {order.Id} est acceptee par le restaurant.",
                DateTime.UtcNow),
            cancellationToken);

        logger.LogInformation("Customer notified for accepted order {OrderId}.", order.Id);
    }
}
