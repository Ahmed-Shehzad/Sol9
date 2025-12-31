using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Transponder.Abstractions;

namespace Orders.API;

public sealed class TransponderBusHostedService : IHostedService
{
    private readonly IBusControl _bus;

    public TransponderBusHostedService(IBusControl bus)
    {
        _bus = bus;
    }

    public Task StartAsync(CancellationToken cancellationToken) => _bus.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => _bus.StopAsync(cancellationToken);
}
