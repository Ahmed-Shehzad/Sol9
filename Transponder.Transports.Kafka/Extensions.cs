using Transponder.Transports.Kafka.Abstractions;

namespace Transponder.Transports.Kafka;

/// <summary>
/// Extension methods to register Kafka transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportBuilder AddKafkaTransport(
        this TransponderTransportBuilder builder,
        Func<IServiceProvider, IKafkaHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        builder.AddTransportFactory<KafkaTransportFactory>();
        builder.AddTransportHost<IKafkaHostSettings, KafkaTransportHost>(
            settingsFactory,
            (_, settings) => new KafkaTransportHost(settings));

        return builder;
    }
}
