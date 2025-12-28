using Microsoft.Extensions.Options;

using Transponder.Abstractions;
using Transponder.Contracts.Orders;

using WebApplication1.Application.Integration;

namespace WebApplication1.Infrastructure.Integration;

internal sealed class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IBus _bus;
    private readonly IntegrationEventPublisherOptions _options;

    public IntegrationEventPublisher(IBus bus, IOptions<IntegrationEventPublisherOptions> options)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task PublishAsync(OrderCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (_options.DestinationAddress is null)
            throw new InvalidOperationException("Integration event destination address is not configured.");

        ISendEndpoint endpoint = await _bus.GetSendEndpointAsync(_options.DestinationAddress, cancellationToken)
            .ConfigureAwait(false);
        await endpoint.SendAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }
}
