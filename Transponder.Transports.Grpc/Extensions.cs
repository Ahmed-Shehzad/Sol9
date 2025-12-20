using Transponder.Transports.Grpc.Abstractions;

namespace Transponder.Transports.Grpc;

/// <summary>
/// Extension methods to register gRPC transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportBuilder AddGrpcTransport(
        this TransponderTransportBuilder builder,
        Func<IServiceProvider, IGrpcHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        builder.AddTransportFactory<GrpcTransportFactory>();
        builder.AddTransportHost<IGrpcHostSettings, GrpcTransportHost>(
            settingsFactory,
            (_, settings) => new GrpcTransportHost(settings));

        return builder;
    }
}
