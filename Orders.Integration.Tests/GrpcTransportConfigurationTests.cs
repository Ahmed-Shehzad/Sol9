using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Transponder;
using Transponder.Transports.Abstractions;
using Transponder.Transports.Grpc;

using Xunit;

namespace Orders.Integration.Tests;

public sealed class GrpcTransportConfigurationTests
{
    [Fact]
    public async Task UseGrpc_registers_hosts_and_provider_resolves_https_addressesAsync()
    {
        var services = new ServiceCollection();
        var local = new Uri("https://localhost:7268");

        _ = services.AddTransponder(local, options =>
        {
            _ = options.TransportBuilder.UseGrpc(local);
        });

        await using ServiceProvider provider = services.BuildServiceProvider();

        var hosts = provider
            .GetServices<ITransportHost>()
            .OfType<GrpcTransportHost>()
            .ToList();

        hosts.Count.ShouldBe(1);
        hosts[0].Address.ShouldBe(local);

        ITransportHostProvider hostProvider = provider.GetRequiredService<ITransportHostProvider>();
        ITransportHost resolved = hostProvider.GetHost(new Uri("https://localhost:7268/requests/test"));

        _ = resolved.ShouldBeOfType<GrpcTransportHost>();
        resolved.Address.ShouldBe(local);

        GrpcTransportHost localHost = hosts.Single(host => host.Address == local);
        localHost.Settings.UseTls.ShouldBeTrue();
    }

    [Fact]
    public async Task UseGrpc_sets_useTls_false_for_http_addressesAsync()
    {
        var services = new ServiceCollection();
        var local = new Uri("http://localhost:5187");
        var remote = new Uri("http://localhost:5296");

        _ = services.AddTransponder(local, options =>
        {
            _ = options.TransportBuilder.UseGrpc(local, new[] { remote });
        });

        await using ServiceProvider provider = services.BuildServiceProvider();

        var hosts = provider
            .GetServices<ITransportHost>()
            .OfType<GrpcTransportHost>()
            .ToList();

        hosts.Count.ShouldBe(2);
        hosts.All(host => host.Settings.UseTls == false).ShouldBeTrue();
    }

    [Fact]
    public async Task Grpc_transport_factory_resolves_for_https_schemeAsync()
    {
        var services = new ServiceCollection();
        var local = new Uri("https://localhost:7268");

        _ = services.AddTransponder(local, options =>
        {
            _ = options.TransportBuilder.UseGrpc(local);
        });

        await using ServiceProvider provider = services.BuildServiceProvider();

        ITransportRegistry registry = provider.GetRequiredService<ITransportRegistry>();
        bool resolved = registry.TryResolve(new Uri("https://localhost:7268"), out ITransportFactory? factory);

        resolved.ShouldBeTrue();
        _ = factory.ShouldBeOfType<GrpcTransportFactory>();
    }
}
