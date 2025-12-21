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

        var transportMessage = context.Message;
        var messageTypeName = transportMessage.MessageType;

        if (string.IsNullOrWhiteSpace(messageTypeName))
        {
            return;
        }

        if (!_registry.TryGetHandlers(_inputAddress, messageTypeName, out var registrations))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var messageType = registrations[0].MessageType;
        var message = _serializer.Deserialize(transportMessage.Body.Span, messageType);

        foreach (var registration in registrations)
        {
            var task = (Task)InvokeMethod.MakeGenericMethod(
                    registration.SagaType,
                    registration.StateType,
                    registration.MessageType)
                .Invoke(
                    null,
                    new object[]
                    {
                        scope.ServiceProvider,
                        registration,
                        message,
                        transportMessage,
                        context.SourceAddress,
                        context.DestinationAddress,
                        context.CancellationToken
                    })!;

            await task.ConfigureAwait(false);
        }
    }

    private static async Task InvokeInternalAsync<TSaga, TState, TMessage>(
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

        var bus = serviceProvider.GetRequiredService<TransponderBus>();
        var consumeContext = new ConsumeContext<TMessage>(
            typedMessage,
            transportMessage,
            sourceAddress,
            destinationAddress,
            cancellationToken,
            bus);

        var correlationId = consumeContext.CorrelationId ?? consumeContext.ConversationId;
        if (!correlationId.HasValue)
        {
            return;
        }

        var repository = serviceProvider.GetRequiredService<ISagaRepository<TState>>();
        var state = await repository.GetAsync(correlationId.Value, cancellationToken).ConfigureAwait(false);

        var isNew = false;
        if (state is null)
        {
            if (!registration.StartIfMissing)
            {
                return;
            }

            state = new TState
            {
                CorrelationId = correlationId.Value,
                ConversationId = consumeContext.ConversationId
            };
            isNew = true;
        }
        else
        {
            if (state.CorrelationId == Guid.Empty)
            {
                state.CorrelationId = correlationId.Value;
            }

            if (state.ConversationId == null && consumeContext.ConversationId.HasValue)
            {
                state.ConversationId = consumeContext.ConversationId;
            }
        }

        var saga = serviceProvider.GetRequiredService<TSaga>();
        if (saga is not ISagaMessageHandler<TState, TMessage> handler)
        {
            throw new InvalidOperationException(
                $"{typeof(TSaga).Name} does not implement ISagaMessageHandler<{typeof(TState).Name}, {typeof(TMessage).Name}>.");
        }

        var sagaContext = new SagaConsumeContext<TState, TMessage>(
            consumeContext,
            state,
            registration.Style,
            isNew);

        await handler.HandleAsync(sagaContext).ConfigureAwait(false);

        if (sagaContext.IsCompleted)
        {
            await repository.DeleteAsync(state.CorrelationId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await repository.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }
    }
}
