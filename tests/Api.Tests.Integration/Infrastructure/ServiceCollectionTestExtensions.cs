using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests.Integration.Infrastructure;

public static class ServiceCollectionTestExtensions
{
    public static IServiceCollection AddTestMocks(this IServiceCollection services)
    {
        services.RemoveAll<TestMockHub>();
        var hub = new TestMockHub();
        hub.Reset();
        services.AddSingleton(hub);
        return services;
    }
}