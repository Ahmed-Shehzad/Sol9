using Transponder.Transports.RabbitMq.Abstractions;

namespace Transponder.Transports.RabbitMq;

/// <summary>
/// Extension methods to register RabbitMQ transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportRegistrationOptions UseRabbitMq(
        this TransponderTransportRegistrationOptions options,
        Func<IServiceProvider, IRabbitMqHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        options.AddTransportFactory<RabbitMqTransportFactory>();
        options.AddTransportHost<IRabbitMqHostSettings, RabbitMqTransportHost>(
            settingsFactory,
            (_, settings) => new RabbitMqTransportHost(settings));

        return options;
    }

    public static TransponderTransportRegistrationOptions UseRabbitMq(
        this TransponderTransportRegistrationOptions options,
        IRabbitMqHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        return options.UseRabbitMq(_ => settings);
    }

    public static TransponderTransportBuilder AddRabbitMqTransport(
        this TransponderTransportBuilder builder,
        Func<IServiceProvider, IRabbitMqHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        builder.AddTransportFactory<RabbitMqTransportFactory>();
        builder.AddTransportHost<IRabbitMqHostSettings, RabbitMqTransportHost>(
            settingsFactory,
            (_, settings) => new RabbitMqTransportHost(settings));

        return builder;
    }
}
