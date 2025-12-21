using Transponder.Transports.Grpc.Abstractions;

namespace Transponder.Transports.Grpc;

/// <summary>
/// Extension methods to register gRPC transports.
/// </summary>
public static class Extensions
{
    public static TransponderTransportRegistrationOptions UseGrpc(
        this TransponderTransportRegistrationOptions options,
        Func<IServiceProvider, IGrpcHostSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settingsFactory);

        options.AddTransportFactory<GrpcTransportFactory>();
        options.AddTransportHost<IGrpcHostSettings, GrpcTransportHost>(
            settingsFactory,
            (_, settings) => new GrpcTransportHost(settings));

        return options;
    }

    public static TransponderTransportRegistrationOptions UseGrpc(
        this TransponderTransportRegistrationOptions options,
        IGrpcHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        return options.UseGrpc(_ => settings);
    }

    public static TransponderTransportRegistrationOptions UseGrpc(
        this TransponderTransportRegistrationOptions options,
        Uri localAddress,
        IEnumerable<Uri>? remoteAddresses = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(localAddress);

        options.AddTransportFactory<GrpcTransportFactory>();
        options.AddTransportHost<GrpcTransportHost>(_ => new GrpcTransportHost(new GrpcHostSettings(
            localAddress,
            useTls: string.Equals(localAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))));

        if (remoteAddresses is null) return options;

        foreach (Uri remoteAddress in remoteAddresses)
        {
            if (remoteAddress == localAddress) continue;

            options.AddTransportHost(_ => new GrpcTransportHost(new GrpcHostSettings(
                remoteAddress,
                useTls: string.Equals(remoteAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))));
        }

        return options;
    }

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
