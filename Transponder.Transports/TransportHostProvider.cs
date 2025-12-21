using Transponder.Transports.Abstractions;

namespace Transponder.Transports;

/// <summary>
/// Default transport host resolver keyed by address scheme.
/// </summary>
public sealed class TransportHostProvider : ITransportHostProvider
{
    private readonly IReadOnlyDictionary<string, ITransportHost> _hostsByScheme;

    public TransportHostProvider(IEnumerable<ITransportHost> hosts)
    {
        ArgumentNullException.ThrowIfNull(hosts);

        var map = new Dictionary<string, ITransportHost>(StringComparer.OrdinalIgnoreCase);

        foreach (ITransportHost host in hosts) map[host.Address.Scheme] = host;

        _hostsByScheme = map;
    }

    public ITransportHost GetHost(Uri address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (!TryGetHost(address, out ITransportHost? host) || host is null) throw new InvalidOperationException($"No transport host registered for scheme '{address.Scheme}'.");

        return host;
    }

    public bool TryGetHost(Uri address, out ITransportHost? host)
    {
        ArgumentNullException.ThrowIfNull(address);
        return _hostsByScheme.TryGetValue(address.Scheme, out host);
    }
}
