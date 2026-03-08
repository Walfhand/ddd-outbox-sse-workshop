using Engine.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Engine.Wolverine.Factory;

public class DbContextFactory(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider container)
    : IAppDbContextFactory
{
    public IDbContext CreateDbContext()
    {
        try
        {
            return httpContextAccessor.HttpContext is null
                ? container.GetRequiredService<IDbContext>()
                : httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IDbContext>();
        }
        catch
        {
            return container.GetRequiredService<IDbContext>();
        }
    }
}