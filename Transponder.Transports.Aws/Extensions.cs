using Transponder.Transports.Aws.Abstractions;

namespace Transponder.Transports.Aws;

/// <summary>
/// Extension methods to register AWS transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportRegistrationOptions UseAws(
        this TransponderTransportRegistrationOptions options,
        Func<IServiceProvider, IAwsTransportHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        options.AddTransportFactory<AwsTransportFactory>();
        options.AddTransportHost<IAwsTransportHostSettings, AwsTransportHost>(
            settingsFactory,
            (_, settings) => new AwsTransportHost(settings));

        return options;
    }

    public static TransponderTransportRegistrationOptions UseAws(
        this TransponderTransportRegistrationOptions options,
        IAwsTransportHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        return options.UseAws(_ => settings);
    }

    public static TransponderTransportBuilder AddAwsTransport(
        this TransponderTransportBuilder builder,
        Func<IServiceProvider, IAwsTransportHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        builder.AddTransportFactory<AwsTransportFactory>();
        builder.AddTransportHost<IAwsTransportHostSettings, AwsTransportHost>(
            settingsFactory,
            (_, settings) => new AwsTransportHost(settings));

        return builder;
    }
}
