using Transponder.Transports.AzureServiceBus.Abstractions;

namespace Transponder.Transports.AzureServiceBus;

/// <summary>
/// Extension methods to register Azure Service Bus transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportRegistrationOptions UseAzureServiceBus(
        this TransponderTransportRegistrationOptions options,
        Func<IServiceProvider, IAzureServiceBusHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        options.AddTransportFactory<AzureServiceBusTransportFactory>();
        options.AddTransportHost<IAzureServiceBusHostSettings, AzureServiceBusTransportHost>(
            settingsFactory,
            (_, settings) => new AzureServiceBusTransportHost(settings));

        return options;
    }

    public static TransponderTransportRegistrationOptions UseAzureServiceBus(
        this TransponderTransportRegistrationOptions options,
        IAzureServiceBusHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        return options.UseAzureServiceBus(_ => settings);
    }

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
