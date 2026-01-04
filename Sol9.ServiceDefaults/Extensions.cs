using Cysharp.Serialization.MessagePack;

using MessagePack;
using MessagePack.Resolvers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Sol9.ServiceDefaults;

public static class Extensions
{
    private const string Ready = "ready";

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        ConfigureMessagePack();
        _ = builder.ConfigureOpenTelemetry();

        _ = builder.Services.AddServiceDiscovery();
        _ = builder.Services.AddGrpc();

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

    public static async Task EnsureDatabaseCreatedAndMigratedAsync<TContext>(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await EnsureDatabaseCreatedAndMigratedAsync(dbContext, cancellationToken).ConfigureAwait(false);
    }

    public static async Task EnsureDatabaseCreatedAndMigratedAsync<TContext>(
        this IDbContextFactory<TContext> factory,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        await using TContext dbContext = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await EnsureDatabaseCreatedAndMigratedAsync(dbContext, cancellationToken).ConfigureAwait(false);
    }

    private async static Task EnsureDatabaseCreatedAndMigratedAsync(
        DbContext dbContext,
        CancellationToken cancellationToken)
    {
        string? connectionString = dbContext.Database.GetConnectionString();
        await EnsureDatabaseExistsAsync(connectionString, cancellationToken).ConfigureAwait(false);

        IEnumerable<string> pendingMigrations =
            await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        if (pendingMigrations.Any())
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseExistsAsync(string? connectionString, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        string? database = builder.Database;
        if (string.IsNullOrWhiteSpace(database)) return;

        builder.Database = "postgres";
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var existsCommand =
            new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        _ = existsCommand.Parameters.AddWithValue("name", database);

        bool exists = await existsCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) is not null;
        if (exists) return;

        string safeDatabase = database.Replace("\"", "\"\"");
        await using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{safeDatabase}\"", connection);
        _ = await createCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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
                _ = healthChecks.AddRedis(value, name: $"redis-{connection.Key}", tags: [Ready]);
                continue;
            }

            _ = healthChecks.AddNpgSql(value, name: $"postgres-{connection.Key}", tags: [Ready]);
        }

        IConfigurationSection transponderDefaults = builder.Configuration.GetSection("TransponderDefaults");
        if (Uri.TryCreate(transponderDefaults["LocalAddress"], UriKind.Absolute, out Uri? localAddress)) _ = healthChecks.AddUrlGroup(localAddress, name: "transponder-local", tags: [Ready]);

        if (Uri.TryCreate(transponderDefaults["RemoteAddress"], UriKind.Absolute, out Uri? remoteAddress)) _ = healthChecks.AddUrlGroup(remoteAddress, name: "transponder-remote", tags: [Ready]);
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
