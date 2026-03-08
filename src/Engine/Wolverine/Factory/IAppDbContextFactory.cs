using Engine.EFCore;

namespace Engine.Wolverine.Factory;

public interface IAppDbContextFactory
{
    IDbContext CreateDbContext();
}