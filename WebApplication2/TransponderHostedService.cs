using Transponder.Abstractions;

namespace WebApplication2;

public sealed class TransponderHostedService : IHostedService
{
    private readonly IBusControl _bus;

    public TransponderHostedService(IBusControl bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _bus.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _bus.StopAsync(cancellationToken);
}
