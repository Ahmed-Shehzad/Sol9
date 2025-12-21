using Transponder.Transports.Kafka.Abstractions;

namespace Transponder.Transports.Kafka;

/// <summary>
/// Extension methods to register Kafka transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportRegistrationOptions UseKafka(
        this TransponderTransportRegistrationOptions options,
        Func<IServiceProvider, IKafkaHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        options.AddTransportFactory<KafkaTransportFactory>();
        options.AddTransportHost<IKafkaHostSettings, KafkaTransportHost>(
            settingsFactory,
            (_, settings) => new KafkaTransportHost(settings));

        return options;
    }

    public static TransponderTransportRegistrationOptions UseKafka(
        this TransponderTransportRegistrationOptions options,
        IKafkaHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        return options.UseKafka(_ => settings);
    }

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
