using Api.Domain.Exceptions;
using Api.Domain.UberEatsOrders.AggregateRoots.ValueObjects;
using Api.Domain.UberEatsOrders.Events;
using DomainEssentials.Core.Implementations;
using DomainEssentials.Core.Keys;

namespace Api.Domain.UberEatsOrders.AggregateRoots;

public sealed record UberEatsOrderId : IdBase
{
    public UberEatsOrderId(Guid id) : base(id)
    {
    }

    public UberEatsOrderId()
    {
    }
}

public enum UberEatsOrderStatus
{
    PendingRestaurantAcceptance = 0,
    AcceptedByRestaurant = 1,
    RejectedByRestaurant = 2
}

public sealed class UberEatsOrder : AggregateRoot<UberEatsOrderId>
{
    private readonly List<UberEatsOrderItem> _items = [];

    private UberEatsOrder()
    {
    }

    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public UberEatsOrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public IReadOnlyCollection<UberEatsOrderItem> Items => _items.AsReadOnly();

    public static UberEatsOrder Place(Guid customerId, Guid restaurantId, IReadOnlyCollection<UberEatsOrderItem> items)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer id is required.", "UBER_EATS_CUSTOMER_ID_REQUIRED");
        if (restaurantId == Guid.Empty)
            throw new DomainException("Restaurant id is required.", "UBER_EATS_RESTAURANT_ID_REQUIRED");
        if (items.Count == 0)
            throw new DomainException("An order must contain at least one item.", "UBER_EATS_ITEMS_REQUIRED");

        var now = DateTime.UtcNow;
        var order = new UberEatsOrder
        {
            CustomerId = customerId,
            RestaurantId = restaurantId,
            Status = UberEatsOrderStatus.PendingRestaurantAcceptance,
            CreatedAtUtc = now
        };

        order._items.AddRange(items);
        order.AddDomainEvent(new UberEatsOrderPlacedDomainEvent(
            order.Id.Value,
            order.CustomerId,
            order.RestaurantId,
            now));

        return order;
    }

    public void AcceptByRestaurant(Guid restaurantId)
    {
        if (restaurantId == Guid.Empty)
            throw new DomainException("Restaurant id is required.", "UBER_EATS_RESTAURANT_ID_REQUIRED");
        if (restaurantId != RestaurantId)
            throw new DomainException("Order does not belong to this restaurant.", "UBER_EATS_RESTAURANT_MISMATCH");
        if (Status != UberEatsOrderStatus.PendingRestaurantAcceptance)
            throw new DomainException("Order is no longer pending.", "UBER_EATS_ORDER_STATUS_INVALID");

        var now = DateTime.UtcNow;
        Status = UberEatsOrderStatus.AcceptedByRestaurant;
        AcceptedAtUtc = now;

        AddDomainEvent(new UberEatsOrderAcceptedDomainEvent(
            Id.Value,
            CustomerId,
            RestaurantId,
            now));
    }
}
