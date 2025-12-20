using Transponder.Transports.Aws.Abstractions;

namespace Transponder.Transports.Aws;

/// <summary>
/// Extension methods to register AWS transports.
/// </summary>
public static class Extensions
{
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
