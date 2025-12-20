using Transponder.Transports.AzureServiceBus.Abstractions;

namespace Transponder.Transports.AzureServiceBus;

/// <summary>
/// Extension methods to register Azure Service Bus transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportBuilder AddAzureServiceBusTransport(
        this TransponderTransportBuilder builder,
        Func<IServiceProvider, IAzureServiceBusHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        builder.AddTransportFactory<AzureServiceBusTransportFactory>();
        builder.AddTransportHost<IAzureServiceBusHostSettings, AzureServiceBusTransportHost>(
            settingsFactory,
            (_, settings) => new AzureServiceBusTransportHost(settings));

        return builder;
    }
}
