using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Engine.EFCore;

public abstract class DbContextDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder
            .UseNpgsql("",
                dbOptions => { dbOptions.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name); })
            .UseSnakeCaseNamingConvention();

        return CreateContext(optionsBuilder.Options, TimeProvider.System);
    }

    protected abstract TContext CreateContext(DbContextOptions<TContext> options, TimeProvider timeProvider);
}