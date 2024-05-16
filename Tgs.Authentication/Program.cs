using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Authentication.Actors;
using Tgs.Authentication.DependencyInjection;
using Tgs.Authentication.Models;
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
        .AddActors(options =>
        {
            options.UseJsonSerialization = false;
            options.Actors.RegisterActor<AccountActor>();
        });

    _ = builder.Services
        .AddKeyedSingleton(
            "IdentityKey",
            (sp, _) =>
            {
                var rsaFileContent = File.ReadAllText(builder.Configuration.GetValue<string>("Identity:SecurityKeyPath")!);
                var rsa = RSA.Create();
                rsa.ImportFromPem(rsaFileContent);

                return new RsaSecurityKey(rsa);
            })
        .AddKeyedSingleton(
            "IdentityKey",
            (sp, _) => new SigningCredentials(
                sp.GetRequiredKeyedService<RsaSecurityKey>("IdentityKey"),
                SecurityAlgorithms.RsaSha256))
        .AddKeyedSingleton(
            "IdentityKey",
            (sp, _) => JsonWebKeyConverter.ConvertFromRSASecurityKey(
                new RsaSecurityKey(sp.GetRequiredKeyedService<RsaSecurityKey>("IdentityKey")
                    .Rsa
                    .ExportParameters(false))))
        .AddSingleton(sp => new TicketDataProtector(
            Convert.FromBase64String(builder.Configuration.GetValue<string>("Identity:Key")!),
            Convert.FromBase64String(builder.Configuration.GetValue<string>("Identity:IV")!)))
        .AddAuthentication()
        .AddScheme<TgsTokenOptions, TgsTokenHandler>(
            BearerTokenDefaults.AuthenticationScheme,
            null)
        .Services
        .AddOptions<TgsTokenOptions>(BearerTokenDefaults.AuthenticationScheme)
        .Configure<IServiceProvider>((options, sp) => options.SigningCredentials =
            sp.GetRequiredKeyedService<SigningCredentials>("IdentityKey"))
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
