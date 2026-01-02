using System.Diagnostics;

using Grpc.Core;

using Transponder.Transports.Abstractions;

namespace Transponder.Transports.Grpc;

/// <summary>
/// gRPC service that dispatches incoming transport messages to receive endpoints.
/// </summary>
public sealed class GrpcTransportService : Transport.TransportBase
{
    private readonly GrpcTransportHost _host;

    public GrpcTransportService(GrpcTransportHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public async override Task<SendResponse> Send(SendRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Destination)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Destination is required."));

        var destination = new Uri(request.Destination);

        if (!_host.TryGetEndpoint(destination, out GrpcReceiveEndpoint endpoint))
        {
#if DEBUG
            Trace.TraceWarning(
                $"[Transponder] GrpcTransportService no endpoint for destination {destination}.");
#endif
            throw new RpcException(new Status(StatusCode.NotFound, "No matching receive endpoint."));
        }

#if DEBUG
        Trace.TraceInformation(
            $"[Transponder] GrpcTransportService dispatching to {destination}.");
#endif
        ITransportMessage message = GrpcTransportMessageMapper.FromProto(request.Message);
        await endpoint.HandleAsync(message, _host.Address, destination, context.CancellationToken).ConfigureAwait(false);
        return new SendResponse();
    }

    public async override Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        ITransportMessage message = GrpcTransportMessageMapper.FromProto(request.Message);
        IReadOnlyCollection<GrpcReceiveEndpoint> endpoints = _host.GetEndpoints();

#if DEBUG
        Trace.TraceInformation(
            $"[Transponder] GrpcTransportService publishing to {endpoints.Count} endpoints.");
#endif
        foreach (GrpcReceiveEndpoint endpoint in endpoints)
            await endpoint.HandleAsync(message, _host.Address, endpoint.InputAddress, context.CancellationToken)
                .ConfigureAwait(false);

        return new PublishResponse();
    }
}
