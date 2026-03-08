using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Integration.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        Environment.SetEnvironmentVariable("Postgres__ConnexionString", connectionString);
        Environment.SetEnvironmentVariable("Wolverine__RetryTimes", "0");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Postgres:ConnexionString"] = _connectionString
                //configure settings here
            };

            config.AddInMemoryCollection(new ReadOnlyDictionary<string, string?>(settings));
        });

        builder.ConfigureServices(services =>
        {
            services.AddTestMocks();
            //configure service here
        });
    }
}