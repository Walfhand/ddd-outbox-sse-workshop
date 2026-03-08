using Api.Domain.Orders.AggregateRoots.ValueObjects;
using DomainEssentials.Core.Implementations;
using DomainEssentials.Core.Keys;

namespace Api.Domain.Orders.AggregateRoots;

public record OrderId : IdBase
{
    public OrderId(Guid id) : base(id)
    {
    }

    public OrderId()
    {
    }
}

public class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderLine> _orderLines = [];

    private Order()
    {
    }

    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();

    public static Order Create()
    {
        return new Order();
    }

    public void AddLine(int productId, int quantity)
    {
        var existing = _orderLines.SingleOrDefault(l => l.ProductId == productId);
        if (existing is not null)
        {
            _orderLines.Remove(existing);
            _orderLines.Add(existing with { Quantity = existing.Quantity + quantity });
        }
        else
        {
            _orderLines.Add(OrderLine.Create(productId, quantity));
        }
    }
}