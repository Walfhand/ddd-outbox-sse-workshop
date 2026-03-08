using Api.Domain.UberEatsOrders.AggregateRoots;
using Engine.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Persistence.Configs.Etcs;

public sealed class UberEatsOrderEtc : BaseEtc<UberEatsOrder, UberEatsOrderId>
{
    protected override UberEatsOrderId KeyCreator(Guid id)
    {
        return new UberEatsOrderId(id);
    }

    public override void Configure(EntityTypeBuilder<UberEatsOrder> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.RestaurantId).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.AcceptedAtUtc);

        builder.OwnsMany(x => x.Items, items =>
        {
            items.Property(x => x.Name).IsRequired().HasMaxLength(200);
            items.Property(x => x.Quantity).IsRequired();
        });
    }
}
