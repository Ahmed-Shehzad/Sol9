using Cysharp.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Sol9.ServiceDefaults;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        ConfigureMessagePack();
        _ = builder.ConfigureOpenTelemetry();

        _ = builder.Services.AddServiceDiscovery();

        _ = builder.Services.ConfigureHttpClientDefaults(http =>
        {
            _ = http.AddStandardResilienceHandler();
            _ = http.AddServiceDiscovery();
        });

        IHealthChecksBuilder healthChecks = builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        AddInfrastructureHealthChecks(builder, healthChecks);
        _ = builder.Services.AddGrpcHealthChecks();

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        _ = app.MapHealthChecks("/health");
        _ = app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });
        _ = app.MapGrpcHealthChecksService();

        return app;
    }

    private static void ConfigureMessagePack()
    {
        IFormatterResolver resolver = CompositeResolver.Create(
            UlidMessagePackResolver.Instance,
            StandardResolver.Instance);
        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }

    private static void AddInfrastructureHealthChecks(IHostApplicationBuilder builder, IHealthChecksBuilder healthChecks)
    {
        IConfigurationSection connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
        foreach (IConfigurationSection connection in connectionStrings.GetChildren())
        {
            string? value = connection.Value;
            if (string.IsNullOrWhiteSpace(value)) continue;

            if (connection.Key.Contains("Redis", StringComparison.OrdinalIgnoreCase))
            {
                _ = healthChecks.AddRedis(value, name: $"redis-{connection.Key}", tags: ["ready"]);
                continue;
            }

            _ = healthChecks.AddNpgSql(value, name: $"postgres-{connection.Key}", tags: ["ready"]);
        }

        IConfigurationSection transponderDefaults = builder.Configuration.GetSection("TransponderDefaults");
        if (Uri.TryCreate(transponderDefaults["LocalAddress"], UriKind.Absolute, out Uri? localAddress)) _ = healthChecks.AddUrlGroup(localAddress, name: "transponder-local", tags: ["ready"]);

        if (Uri.TryCreate(transponderDefaults["RemoteAddress"], UriKind.Absolute, out Uri? remoteAddress)) _ = healthChecks.AddUrlGroup(remoteAddress, name: "transponder-remote", tags: ["ready"]);
    }

    private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        _ = builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.ParseStateValues = true;
            logging.IncludeScopes = true;
            if (useOtlpExporter) _ = logging.AddOtlpExporter();
        });
        _ = builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                _ = metrics.AddAspNetCoreInstrumentation();
                _ = metrics.AddHttpClientInstrumentation();
                _ = metrics.AddRuntimeInstrumentation();
                if (useOtlpExporter) _ = metrics.AddOtlpExporter();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment()) _ = tracing.SetSampler(new AlwaysOnSampler());

                _ = tracing.AddAspNetCoreInstrumentation();
                _ = tracing.AddHttpClientInstrumentation();
                if (useOtlpExporter) _ = tracing.AddOtlpExporter();
            });

        return builder;
    }
}
