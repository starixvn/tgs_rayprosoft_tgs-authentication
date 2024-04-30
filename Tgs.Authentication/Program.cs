using System.Reflection;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Authentication.Actors;
using Tgs.Authentication.DependencyInjection;
using Tgs.Authentication.Repositories.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
{
    _ = builder.Logging
        .AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.AddOtlpExporter();
        });

    builder.Services
        .AddSingleton<IActorProxyFactory>(sp =>
        {
            var value = sp.GetRequiredService<IOptions<ActorRuntimeOptions>>().Value;

            return new ActorProxyFactory
            {
                DefaultOptions =
                {
                    UseJsonSerialization = value.UseJsonSerialization,
                    JsonSerializerOptions = value.JsonSerializerOptions,
                    DaprApiToken = value.DaprApiToken,
                    HttpEndpoint = value.HttpEndpoint
                }
            };
        })
        .AddActors(options =>
        {
            options.UseJsonSerialization = true;
            options.Actors.RegisterActor<AccountActor>();
        });

    _ = builder.Services
        .AddAuthentication()
        .AddBearerToken()
        .Services
        .AddProblemDetails()
        .AddRouting(options => options.LowercaseUrls = true)
        .AddControllers()
        .Services
        .AddTgsAuthentication()
        .AddTgsAuthenticationRepositories(dbBuilder => dbBuilder
            .UseNpgsql("name=Tgs"));

    _ = builder.Services
        .AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthenticationService", Version = "v1" });

            var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var path = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(path))
                c.IncludeXmlComments(path);
        });

    var instrumentationExtenionFilters = new[] { "html", "ico", "png", "jpg" };
    _ = builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resourceBuilder => resourceBuilder
            .AddEnvironmentVariableDetector())
        .WithTracing(
            tracingBuilder => tracingBuilder
                .AddSource("Tgs.*")
                .AddSource("TGS.*")
                .AddSource("Valhalla.*")
                .AddAspNetCoreInstrumentation(options => options.Filter =
                    httpContext => !httpContext.Request.Path.HasValue
                        || !httpContext.Request.Path.StartsWithSegments("/swagger")
                        && !httpContext.Request.Path.StartsWithSegments("/healthz")
                        && !httpContext.Request.Path.StartsWithSegments("/metrics")
                        && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                        && !httpContext.Request.Path.StartsWithSegments("/_vs/")
                        && !httpContext.Request.Path.StartsWithSegments("/_framework/")
                        && !instrumentationExtenionFilters.Any(
                            ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
                .AddProfileViewExporter()
                .AddOtlpExporter())
        .WithMetrics(meterBuilder => meterBuilder
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter())
        .Services
        .AddHealthChecks()
        .Services
        .AddProfileViewer();
}

var app = builder.Build();
{
    app.MapActorsHandlers();
    app.MapControllers();
    app.MapSwagger();
    app.UseSwaggerUI();
    app.MapHealthChecks("/healthz");
    app.MapProfileViewer();
}

app.Run();
