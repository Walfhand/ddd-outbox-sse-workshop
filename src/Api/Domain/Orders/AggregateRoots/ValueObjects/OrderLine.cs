using Api.Domain.Exceptions;

namespace Api.Domain.Orders.AggregateRoots.ValueObjects;

public record OrderLine
{
    private OrderLine()
    {
    }

    private OrderLine(int quantity, int productId)
    {
        Quantity = quantity;
        ProductId = productId;
    }

    public int Quantity { get; init; }
    public int ProductId { get; init; }

    public static OrderLine Create(int quantity, int productId)
    {
        if (quantity < 1)
            throw new DomainException("Quantity cannot be less than 1", "");
        return productId < 1
            ? throw new DomainException("ProductId cannot be less than 1", "")
            : new OrderLine(quantity, productId);
    }
}