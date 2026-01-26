using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Transponder.Abstractions;
using Transponder.Persistence.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

internal sealed class SagaReceiveEndpointHandler
{
    private static readonly MethodInfo InvokeMethod =
        typeof(SagaReceiveEndpointHandler).GetMethod(
            nameof(InvokeInternalAsync),
            BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("Saga handler invoker method not found.");

    private readonly Uri _inputAddress;
    private readonly SagaEndpointRegistry _registry;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SagaReceiveEndpointHandler> _logger;

    public SagaReceiveEndpointHandler(
        Uri inputAddress,
        SagaEndpointRegistry registry,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory,
        ILogger<SagaReceiveEndpointHandler> logger)
    {
        _inputAddress = inputAddress ?? throw new ArgumentNullException(nameof(inputAddress));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(IReceiveContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ITransportMessage transportMessage = context.Message;
        string? messageTypeName = transportMessage.MessageType;

        if (string.IsNullOrWhiteSpace(messageTypeName))
        {
            _logger.LogWarning(
                "SagaReceiveEndpointHandler missing message type. InputAddress={InputAddress}",
                _inputAddress);
            return;
        }

        if (!_registry.TryGetHandlers(_inputAddress, messageTypeName, out IReadOnlyList<SagaMessageRegistration> registrations))
        {
            _logger.LogDebug(
                "SagaReceiveEndpointHandler no handlers found. InputAddress={InputAddress}, MessageType={MessageType}",
                _inputAddress,
                messageTypeName);
            return;
        }

        _logger.LogDebug(
            "SagaReceiveEndpointHandler dispatching handlers. InputAddress={InputAddress}, MessageType={MessageType}, HandlerCount={HandlerCount}",
            _inputAddress,
            messageTypeName,
            registrations.Count);

        using IServiceScope scope = _scopeFactory.CreateScope();

        Type messageType = registrations[0].MessageType;
        object message = _serializer.Deserialize(transportMessage.Body.Span, messageType);

        foreach (SagaMessageRegistration registration in registrations)
        {
            var task = (Task)InvokeMethod.MakeGenericMethod(
                    registration.SagaType,
                    registration.StateType,
                    registration.MessageType)
                .Invoke(
                    null,
                    [
                        scope.ServiceProvider,
                        registration,
                        message,
                        transportMessage,
                        context.SourceAddress,
                        context.DestinationAddress,
                        context.CancellationToken
                    ])!;

            await task.ConfigureAwait(false);
        }
    }

    public async static Task InvokeInternalAsync<TSaga, TState, TMessage>(
        IServiceProvider serviceProvider,
        SagaMessageRegistration registration,
        object message,
        ITransportMessage transportMessage,
        Uri? sourceAddress,
        Uri? destinationAddress,
        CancellationToken cancellationToken)
        where TSaga : class
        where TState : class, ISagaState, new()
        where TMessage : class, IMessage
    {
        var typedMessage = (TMessage)message;

        TransponderBus bus = serviceProvider.GetRequiredService<TransponderBus>();
        var consumeContext = new ConsumeContext<TMessage>(
            typedMessage,
            transportMessage,
            sourceAddress,
            destinationAddress,
            cancellationToken,
            bus);

        TransponderMessageContext messageContext = TransponderMessageContextFactory.FromTransportMessage(
            transportMessage,
            sourceAddress,
            destinationAddress);
        using IDisposable? scope = bus.BeginConsumeScope(messageContext);

        Ulid? correlationId = consumeContext.CorrelationId ?? consumeContext.ConversationId;
        if (!correlationId.HasValue)
        {
            ILogger<SagaReceiveEndpointHandler>? logger = serviceProvider.GetService<ILogger<SagaReceiveEndpointHandler>>();
            logger?.LogWarning(
                "SagaReceiveEndpointHandler missing correlation id. MessageType={MessageType}",
                typeof(TMessage).Name);
            return;
        }

        ISagaRepository<TState> repository = serviceProvider.GetRequiredService<ISagaRepository<TState>>();
        TState? state = await repository.GetAsync(correlationId.Value, cancellationToken).ConfigureAwait(false);

        bool isNew = false;
        if (state is null)
        {
            if (!registration.StartIfMissing) return;

            state = new TState
            {
                CorrelationId = correlationId.Value,
                ConversationId = consumeContext.ConversationId
            };
            isNew = true;
        }
        else
        {
            if (state.CorrelationId == Ulid.Empty) state.CorrelationId = correlationId.Value;

            if (state.ConversationId == null && consumeContext.ConversationId.HasValue) state.ConversationId = consumeContext.ConversationId;
        }

        TSaga saga = serviceProvider.GetRequiredService<TSaga>();
        if (saga is not ISagaMessageHandler<TState, TMessage> handler)
            throw new InvalidOperationException(
                $"{typeof(TSaga).Name} does not implement ISagaMessageHandler<{typeof(TState).Name}, {typeof(TMessage).Name}>.");

        var sagaContext = new SagaConsumeContext<TState, TMessage>(
            consumeContext,
            state,
            registration.Style,
            isNew);

        bool skipHandler = false;
        if (saga is ISagaStepProvider<TState, TMessage> stepProvider)
        {
            IEnumerable<SagaStep<TState>> steps = stepProvider.GetSteps(sagaContext)
                ?? [];
            SagaStatus status = await sagaContext.ExecuteStepsAsync(steps, cancellationToken).ConfigureAwait(false);
            skipHandler = status != SagaStatus.Completed;
        }

        if (!skipHandler) await handler.HandleAsync(sagaContext).ConfigureAwait(false);

        if (sagaContext.IsCompleted) await repository.DeleteAsync(state.CorrelationId, cancellationToken).ConfigureAwait(false);
        else
        {
            bool saved = await repository.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            if (!saved)
            {
                ILogger<SagaReceiveEndpointHandler>? logger = serviceProvider.GetService<ILogger<SagaReceiveEndpointHandler>>();
                logger?.LogWarning(
                    "SagaReceiveEndpointHandler: Concurrency conflict saving saga state. CorrelationId={CorrelationId}, MessageType={MessageType}, Version={Version}",
                    state.CorrelationId,
                    typeof(TMessage).Name,
                    state.Version);
                // State was modified by another handler, skip this update
                // The message will be retried and processed with the updated state
            }
        }
    }
}
