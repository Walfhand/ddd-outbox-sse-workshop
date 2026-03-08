using Api.Domain.Orders.AggregateRoots;
using Engine.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Persistence.Configs.Etcs;

public class OrderEtc : BaseEtc<Order, OrderId>
{
    protected override OrderId KeyCreator(Guid id)
    {
        return new OrderId(id);
    }

    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);
        builder.OwnsMany(o => o.OrderLines);
    }
}