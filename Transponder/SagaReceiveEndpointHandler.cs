using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Transponder.Abstractions;
using Transponder.Persistence.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

internal sealed class SagaReceiveEndpointHandler
{
    private static readonly MethodInfo InvokeMethod =
        typeof(SagaReceiveEndpointHandler).GetMethod(
            nameof(InvokeInternalAsync),
            BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Saga handler invoker method not found.");

    private readonly Uri _inputAddress;
    private readonly SagaEndpointRegistry _registry;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;

    public SagaReceiveEndpointHandler(
        Uri inputAddress,
        SagaEndpointRegistry registry,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory)
    {
        _inputAddress = inputAddress ?? throw new ArgumentNullException(nameof(inputAddress));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task HandleAsync(IReceiveContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ITransportMessage transportMessage = context.Message;
        string? messageTypeName = transportMessage.MessageType;

        if (string.IsNullOrWhiteSpace(messageTypeName)) return;

        if (!_registry.TryGetHandlers(_inputAddress, messageTypeName, out IReadOnlyList<SagaMessageRegistration> registrations)) return;

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

    private async static Task InvokeInternalAsync<TSaga, TState, TMessage>(
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

        Guid? correlationId = consumeContext.CorrelationId ?? consumeContext.ConversationId;
        if (!correlationId.HasValue) return;

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
            if (state.CorrelationId == Guid.Empty) state.CorrelationId = correlationId.Value;

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
        else await repository.SaveAsync(state, cancellationToken).ConfigureAwait(false);
    }
}
