using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
{
    _ = builder.Logging
        .AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            _ = options.AddFluentdForwardExporter(fluentOptions =>
            {
                var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
                fluentOptions.Host = nodeIp;
                fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
                fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
                    "MachineName",
                    () => Environment.MachineName));
            });
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

            var xml = $"document.xml";
            var path = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(path))
                c.IncludeXmlComments(path);
        });

    var instrumentationExtenionFilters = new[] { "html", "ico", "png", "jpg" };
    _ = builder.Services
        .AddOpenTelemetry()
        .WithTracing(
            tracingBuilder => tracingBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddEnvironmentVariableDetector())
                .AddSource("Tgs.*")
                .AddSource("TGS.*")
                .AddSource("Valhalla.*")
                .AddAspNetCoreInstrumentation(options => options.Filter =
                    httpContext => !httpContext.Request.Path.HasValue
                        || !httpContext.Request.Path.StartsWithSegments("/swagger")
                        && !httpContext.Request.Path.StartsWithSegments("/healthz")
                        && !httpContext.Request.Path.StartsWithSegments("/metrics")
                        && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                        && !instrumentationExtenionFilters.Any(
                            ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
                .AddProfileViewExporter()
                .AddOtlpExporter())
        .WithMetrics(meterBuilder => meterBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddEnvironmentVariableDetector())
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
    app.MapSwagger();
    app.UseSwaggerUI();
    app.MapHealthChecks("/healthz");
    app.MapProfileViewer();
}

app.Run();
