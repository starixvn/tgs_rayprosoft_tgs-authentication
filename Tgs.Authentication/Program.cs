using System.Reflection;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
{
    _ = builder.Logging
        .AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.AddOtlpExporter();
        });

    _ = builder.Services
        .AddAuthentication()
        .AddBearerToken()
        .Services
        .AddProblemDetails()
        .AddRouting(options => options.LowercaseUrls = true)
        .AddControllers();

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
    app.MapControllers();
    app.MapSwagger();
    app.UseSwaggerUI();
    app.MapHealthChecks("/healthz");
    app.MapProfileViewer();
}

app.Run();
