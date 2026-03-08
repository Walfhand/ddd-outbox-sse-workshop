using Engine.EFCore;
using Engine.Wolverine.Factory;

namespace Api.Infrastructure.Persistence.Configs;

public static class PersistenceConfig
{
    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services.AddCustomDbContext<AppDbContext>();
        services.AddScoped<IAppDbContextFactory, DbContextFactory>();
        return services;
    }
    
    public static WebApplication UseMigration(this WebApplication app)
    {
        app.UseMigration<AppDbContext>();
        return app;
    }
}