using Api.Domain.Exceptions;

namespace Api.Domain.UberEatsOrders.AggregateRoots.ValueObjects;

public sealed record UberEatsOrderItem
{
    private UberEatsOrderItem()
    {
    }

    public string Name { get; private set; } = null!;
    public int Quantity { get; private set; }

    public static UberEatsOrderItem Create(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Item name is required.", "UBER_EATS_ITEM_NAME_REQUIRED");
        if (quantity <= 0)
            throw new DomainException("Item quantity must be greater than zero.", "UBER_EATS_ITEM_QUANTITY_INVALID");

        return new UberEatsOrderItem
        {
            Name = name.Trim(),
            Quantity = quantity
        };
    }
}
