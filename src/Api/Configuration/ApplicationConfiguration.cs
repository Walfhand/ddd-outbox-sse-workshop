using System.Reflection;
using Api.Configuration.Cqrs;
using Api.Configuration.Integrations;
using Api.Infrastructure.Persistence.Configs;
using Engine.Core.Events;
using Engine.Logging;
using Engine.ProblemDetails;
using Engine.Wolverine;
using QuickApi.Abstractions.Cqrs;
using QuickApi.Engine.Web;

namespace Api.Configuration;

internal static class ApplicationConfiguration
{
    public static WebApplicationBuilder AddEngineModules(this WebApplicationBuilder builder, Assembly assembly)
    {
        builder.Host.UseCustomWolverine(builder.Configuration, assembly);
        builder.Host.AddCustomLogging();

        builder.Services.AddCustomProblemDetails();
        builder.Services.AddMinimalEndpoints(options => { options.SetBaseApiPath("api/v1"); });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddData();


        builder.Services.AddScoped<IMessage, MessageService>();
        builder.Services.AddScoped<IEventMapper, EventMapper>();
        return builder;
    }

    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseMinimalEndpoints();
        app.UseMigration();
        return app;
    }
}