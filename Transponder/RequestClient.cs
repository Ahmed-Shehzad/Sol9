using System.Collections.Concurrent;
using System.Diagnostics;

using Transponder.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

internal sealed class RequestClient<TRequest> : IRequestClient<TRequest>
    where TRequest : class, IMessage
{
    private readonly TransponderBus _bus;
    private readonly IMessageSerializer _serializer;
    private readonly ITransportHostProvider _hostProvider;
    private readonly Uri _destinationAddress;
    private readonly TimeSpan _timeout;
    private readonly ConcurrentDictionary<Guid, PendingRequest> _pendingRequests = new();
    private readonly Lock _sync = new();
    private IReceiveEndpoint? _responseEndpoint;
    private Uri? _responseAddress;

    public RequestClient(
        TransponderBus bus,
        IMessageSerializer serializer,
        ITransportHostProvider hostProvider,
        Uri destinationAddress,
        TimeSpan timeout)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _hostProvider = hostProvider ?? throw new ArgumentNullException(nameof(hostProvider));
        _destinationAddress = destinationAddress ?? throw new ArgumentNullException(nameof(destinationAddress));
        _timeout = timeout <= TimeSpan.Zero ? TimeSpan.FromSeconds(30) : timeout;
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TResponse : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(request);

#if DEBUG
        Trace.TraceInformation(
            $"[Transponder] RequestClient<{typeof(TRequest).Name}> starting. Destination={_destinationAddress}");
#endif

        await EnsureResponseEndpointAsync(cancellationToken).ConfigureAwait(false);

#if DEBUG
        Trace.TraceInformation(
            $"[Transponder] RequestClient<{typeof(TRequest).Name}> response endpoint ready. ResponseAddress={_responseAddress}");
#endif

        var requestId = Guid.NewGuid();
        var pending = new PendingRequest(typeof(TResponse));
        _pendingRequests[requestId] = pending;

        try
        {
            var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                [TransponderMessageHeaders.RequestId] = requestId.ToString("D")
            };

            if (_responseAddress is not null) headers[TransponderMessageHeaders.ResponseAddress] = _responseAddress.ToString();

            await _bus.SendInternalAsync(
                    _destinationAddress,
                    request,
                    requestId,
                    requestId,
                    null,
                    headers,
                    cancellationToken)
                .ConfigureAwait(false);

#if DEBUG
            Trace.TraceInformation(
                $"[Transponder] RequestClient<{typeof(TRequest).Name}> sent request. RequestId={requestId:D}");
#endif

            var delayTask = Task.Delay(_timeout, cancellationToken);
            Task completed = await Task.WhenAny(pending.Task, delayTask).ConfigureAwait(false);

            if (completed == delayTask)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException(
                    $"Request timed out after {_timeout.TotalSeconds:F0} seconds for {typeof(TRequest).Name}.");
            }

            object response = await pending.Task.ConfigureAwait(false);
            return (TResponse)response;
        }
        finally
        {
            _ = _pendingRequests.TryRemove(requestId, out _);
        }
    }

    private async Task EnsureResponseEndpointAsync(CancellationToken cancellationToken)
    {
        if (_responseEndpoint is not null) return;

        lock (_sync)
        {
            if (_responseEndpoint is not null) return;

            _responseAddress = _bus.CreateResponseAddress();
            ITransportHost host = _hostProvider.GetHost(_responseAddress);

            var configuration = new ReceiveEndpointConfiguration(
                _responseAddress,
                HandleResponseAsync);

            _responseEndpoint = host.ConnectReceiveEndpoint(configuration);
        }

        await _responseEndpoint!.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task HandleResponseAsync(IReceiveContext context)
    {
        ITransportMessage message = context.Message;

        TransponderMessageContext messageContext = TransponderMessageContextFactory.FromTransportMessage(
            message,
            context.SourceAddress,
            context.DestinationAddress);
        using IDisposable? scope = _bus.BeginConsumeScope(messageContext);

        if (!TryGetRequestId(message, out Guid requestId) || !_pendingRequests.TryGetValue(requestId, out PendingRequest? pending))
        {
#if DEBUG
            Trace.TraceWarning(
                $"[Transponder] RequestClient<{typeof(TRequest).Name}> response ignored. " +
                $"HasRequestId={TryGetRequestId(message, out _)} " +
                $"MessageType={message.MessageType ?? "unknown"}");
#endif
            return Task.CompletedTask;
        }

        try
        {
#if DEBUG
            Trace.TraceInformation(
                $"[Transponder] RequestClient<{typeof(TRequest).Name}> response received. RequestId={requestId:D}");
#endif
            object response = _serializer.Deserialize(message.Body.Span, pending.ResponseType);
            pending.TrySetResult(response);
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.TraceError(
                $"[Transponder] RequestClient<{typeof(TRequest).Name}> response deserialize failed. RequestId={requestId:D} Error={ex.Message}");
#endif
            string responseTypeName = pending.ResponseType.FullName ?? pending.ResponseType.Name;
            string messageTypeName = message.MessageType ?? "unknown";
            var wrapped = new InvalidOperationException(
                $"Failed to deserialize response for request {requestId} (message type: {messageTypeName}, response type: {responseTypeName}).",
                ex);
            pending.TrySetException(wrapped);
        }

        return Task.CompletedTask;
    }

    private static bool TryGetRequestId(ITransportMessage message, out Guid requestId)
    {
        if (message.CorrelationId.HasValue)
        {
            requestId = message.CorrelationId.Value;
            return true;
        }

        if (message.Headers.TryGetValue(TransponderMessageHeaders.RequestId, out object? headerValue) &&
            Guid.TryParse(headerValue?.ToString(), out Guid parsed))
        {
            requestId = parsed;
            return true;
        }

        requestId = Guid.Empty;
        return false;
    }

    private sealed class PendingRequest
    {
        private readonly TaskCompletionSource<object> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public PendingRequest(Type responseType)
        {
            ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
        }

        public Type ResponseType { get; }

        public Task<object> Task => _completion.Task;

        public void TrySetResult(object response) => _completion.TrySetResult(response);

        public void TrySetException(Exception exception) => _completion.TrySetException(exception);
    }
}
