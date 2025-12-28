using Microsoft.Extensions.Options;

using Sol9.Eventing.Abstractions;

using Transponder.Abstractions;

namespace Sol9.Eventing;

public sealed class IntegrationEventPublisher<TNotification> : IIntegrationEventHandler<TNotification>
    where TNotification : IIntegrationEvent
{
    private readonly IBus _bus;
    private readonly IntegrationEventPublisherOptions _options;

    public IntegrationEventPublisher(IBus bus, IOptions<IntegrationEventPublisherOptions> options)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification is not IMessage)
            throw new InvalidOperationException("Integration event must implement IMessage to be dispatched via Transponder.");

        if (_options.DestinationAddress is null)
            throw new InvalidOperationException("Integration event destination address is not configured.");

        ISendEndpoint endpoint = await _bus.GetSendEndpointAsync(_options.DestinationAddress, cancellationToken)
            .ConfigureAwait(false);
        await endpoint.SendAsync((dynamic)notification, cancellationToken).ConfigureAwait(false);
    }
}
