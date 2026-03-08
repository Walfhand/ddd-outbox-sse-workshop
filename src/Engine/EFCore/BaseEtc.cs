using DomainEssentials.Core.Implementations;
using DomainEssentials.Core.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engine.EFCore;

public abstract class BaseEtc<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TKey>
    where TKey : IdBase, new()
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => KeyCreator(value))
            .ValueGeneratedNever();
        builder.Property(x => x.Version).IsConcurrencyToken();
    }

    protected abstract TKey KeyCreator(Guid id);
}