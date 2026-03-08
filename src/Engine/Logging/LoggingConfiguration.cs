using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Engine.Logging;

public static class LoggingConfiguration
{
    public static IHostBuilder AddCustomLogging(this IHostBuilder builder)
    {
        builder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Wolverine", LogEventLevel.Warning)
                .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Warning)
                .MinimumLevel.Override("Hermes.Engine.Wolverine", LogEventLevel.Warning);

            loggerConfiguration.WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );

            loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("EnvironmentName", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName();
        });
        return builder;
    }
}