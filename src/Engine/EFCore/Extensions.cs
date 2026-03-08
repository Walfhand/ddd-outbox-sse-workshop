using System.Linq.Expressions;
using Engine.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Engine.EFCore;

public static class Extensions
{
    public static IServiceCollection AddCustomDbContext<TContext>(
        this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TContext : DbContext, IDbContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddValidateOptions<Postgres>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<Postgres>();

            options.UseNpgsql(postgresOptions.ConnexionString,
                    dbOptions =>
                    {
                        dbOptions.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name);
                        dbOptions.EnableRetryOnFailure();
                    })
                .UseSnakeCaseNamingConvention();
        }, lifetime);
        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TContext>());

        return services;
    }


    public static void UseGlobalQueryFilterFor<TEntity>(this ModelBuilder builder,
        Expression<Func<TEntity, bool>> predicate)
    {
        foreach (var mutableEntityType in builder.Model.GetEntityTypes())
            if (mutableEntityType.ClrType.IsAssignableTo(typeof(TEntity)))
            {
                var parameter = Expression.Parameter(mutableEntityType.ClrType);
                var body = ReplacingExpressionVisitor.Replace(predicate.Parameters.First(), parameter,
                    predicate.Body);
                var lambdaExpression = Expression.Lambda(body, parameter);
                mutableEntityType.SetQueryFilter(lambdaExpression);
            }
    }


    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app)
        where TContext : DbContext, IDbContext
    {
        MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();

        return app;
    }

    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext, IDbContext
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }
}