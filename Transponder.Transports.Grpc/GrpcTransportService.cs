using Grpc.Core;

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

    public override async Task<SendResponse> Send(SendRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Destination is required."));
        }

        var destination = new Uri(request.Destination);

        if (!_host.TryGetEndpoint(destination, out var endpoint))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "No matching receive endpoint."));
        }

        var message = GrpcTransportMessageMapper.FromProto(request.Message);
        await endpoint.HandleAsync(message, _host.Address, destination, context.CancellationToken).ConfigureAwait(false);
        return new SendResponse();
    }

    public override async Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        var message = GrpcTransportMessageMapper.FromProto(request.Message);
        var endpoints = _host.GetEndpoints();

        foreach (var endpoint in endpoints)
        {
            await endpoint.HandleAsync(message, _host.Address, endpoint.InputAddress, context.CancellationToken)
                .ConfigureAwait(false);
        }

        return new PublishResponse();
    }
}
