using Api.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Infrastructure;

[Collection("integration")]
public abstract class BaseIntegrationTest(IntegrationTestFixture fixture) : IAsyncLifetime
{
    protected IntegrationTestFixture Fixture { get; } = fixture;


    public Task InitializeAsync()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TestMockHub>().Reset();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    protected async Task WithDbContextAsync(Func<AppDbContext, Task> action)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(dbContext);
    }

    protected async Task WithScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }
}