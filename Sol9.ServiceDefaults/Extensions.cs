using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
        _ = builder.ConfigureOpenTelemetry();

        _ = builder.Services.AddServiceDiscovery();

        _ = builder.Services.ConfigureHttpClientDefaults(http =>
        {
            _ = http.AddStandardResilienceHandler();
            _ = http.AddServiceDiscovery();
        });

        _ = builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

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

        return app;
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
