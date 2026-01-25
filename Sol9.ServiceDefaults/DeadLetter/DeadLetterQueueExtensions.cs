using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Transponder;
using Transponder.Transports;
using Transponder.Transports.Abstractions;

namespace Sol9.ServiceDefaults.DeadLetter;

public static class DeadLetterQueueExtensions
{
    public static IHostApplicationBuilder AddPostgresDeadLetterQueue(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var settings =
            PostgresDeadLetterQueueSettings.FromConfiguration(builder.Configuration);
        if (settings is null) return builder;

        _ = builder.Services.AddSingleton(settings);
        _ = builder.Services.AddDbContextFactory<PostgresDeadLetterQueueDbContext>(options =>
            _ = options.UseNpgsql(settings.ConnectionString));
        _ = builder.Services.AddSingleton<PostgresDeadLetterQueueStore>();
        _ = builder.Services.AddSingleton<PostgresDeadLetterTransportHost>();
        _ = builder.Services.AddSingleton<ITransportHost>(
            sp => sp.GetRequiredService<PostgresDeadLetterTransportHost>());
        _ = builder.Services.AddHostedService<PostgresDeadLetterQueueInitializer>();
        _ = builder.Services.AddSingleton(new ReceiveEndpointFaultSettings(deadLetterAddress: settings.Address));

        return builder;
    }

    public static WebApplication MapDeadLetterQueueEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment() || app.Services.GetService<PostgresDeadLetterQueueStore>() is null) return app;

        _ = app.MapGet("/dlq", async (
                int? limit,
                int? offset,
                PostgresDeadLetterQueueStore store,
                CancellationToken cancellationToken) =>
            Results.Ok(await store.ListAsync(
                    limit ?? 50,
                    offset ?? 0,
                    cancellationToken)
                .ConfigureAwait(false)));

        _ = app.MapGet("/dlq/{id:long}", async (
                long id,
                PostgresDeadLetterQueueStore store,
                CancellationToken cancellationToken) =>
            {
                DeadLetterMessageDetail? message = await store.GetAsync(id, cancellationToken).ConfigureAwait(false);
                return message is null ? Results.NotFound() : Results.Ok(message);
            });

        _ = app.MapPost("/dlq/{id:long}/replay", async (
                long id,
                string? destination,
                PostgresDeadLetterQueueStore store,
                ITransportHostProvider hostProvider,
                CancellationToken cancellationToken) =>
            {
                DeadLetterMessageDetail? message = await store.GetAsync(id, cancellationToken).ConfigureAwait(false);
                if (message is null) return Results.NotFound();

                Uri? destinationAddress = null;
                if (!string.IsNullOrWhiteSpace(destination))
                {
                    if (!Uri.TryCreate(destination, UriKind.Absolute, out destinationAddress))
                        return Results.BadRequest("Invalid destination address.");
                }
                else if (!string.IsNullOrWhiteSpace(message.DestinationAddress) &&
                         Uri.TryCreate(message.DestinationAddress, UriKind.Absolute, out Uri? parsed)) destinationAddress = parsed;

                if (destinationAddress is null)
                    return Results.BadRequest("Destination address is required to replay a message.");

                ITransportMessage replayMessage = message.ToTransportMessage();
                if (!string.IsNullOrWhiteSpace(destination))
                {
                    var headers = new Dictionary<string, object?>(
                        replayMessage.Headers,
                        StringComparer.OrdinalIgnoreCase)
                    {
                        [TransponderMessageHeaders.DestinationAddress] = destinationAddress.ToString()
                    };

                    replayMessage = new TransportMessage(
                        replayMessage.Body,
                        replayMessage.ContentType,
                        headers,
                        replayMessage.MessageId,
                        replayMessage.CorrelationId,
                        replayMessage.ConversationId,
                        replayMessage.MessageType,
                        replayMessage.SentTime);
                }

                try
                {
                    ITransportHost host = hostProvider.GetHost(destinationAddress);
                    ISendTransport transport = await host.GetSendTransportAsync(
                            destinationAddress,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await transport.SendAsync(replayMessage, cancellationToken).ConfigureAwait(false);
                    await store.MarkReplayAsync(id, true, null, cancellationToken).ConfigureAwait(false);
                    return Results.Accepted($"/dlq/{id}", new
                    {
                        Id = id,
                        Destination = destinationAddress.ToString()
                    });
                }
                catch (Exception ex)
                {
                    await store.MarkReplayAsync(id, false, ex.Message, cancellationToken).ConfigureAwait(false);
                    return Results.Problem(ex.Message);
                }
            });

        return app;
    }
}
