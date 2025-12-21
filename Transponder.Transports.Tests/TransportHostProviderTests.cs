using Transponder.Transports.Abstractions;
using Transponder.Transports;

namespace Transponder.Transports.Tests;

public sealed class TransportHostProviderTests
{
    private sealed class StubTransportHost : ITransportHost
    {
        public StubTransportHost(Uri address)
        {
            Address = address;
        }

        public Uri Address { get; }

        public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ISendTransport> GetSendTransportAsync(Uri address, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IPublishTransport> GetPublishTransportAsync(Type messageType, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IReceiveEndpoint ConnectReceiveEndpoint(IReceiveEndpointConfiguration configuration)
            => throw new NotImplementedException();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public void GetHost_Returns_Registered_Host()
    {
        var host = new StubTransportHost(new Uri("foo://host"));
        var provider = new TransportHostProvider(new[] { host });

        ITransportHost resolved = provider.GetHost(new Uri("foo://other"));

        Assert.Same(host, resolved);
    }

    [Fact]
    public void GetHost_Throws_When_Missing_Scheme()
    {
        var provider = new TransportHostProvider(Array.Empty<ITransportHost>());

        Assert.Throws<InvalidOperationException>(() => provider.GetHost(new Uri("missing://host")));
    }
}
