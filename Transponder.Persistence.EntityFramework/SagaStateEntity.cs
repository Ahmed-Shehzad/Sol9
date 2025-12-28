using System.Text.Json;

using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence.EntityFramework;

/// <summary>
/// Entity Framework entity for saga state.
/// </summary>
public sealed class SagaStateEntity
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Guid CorrelationId { get; set; }

    public Guid? ConversationId { get; set; }

    public string StateType { get; set; } = string.Empty;

    public string StateData { get; set; } = string.Empty;

    public DateTimeOffset UpdatedTime { get; set; }

    internal static SagaStateEntity FromState<TState>(TState state)
        where TState : class, ISagaState
    {
        ArgumentNullException.ThrowIfNull(state);

        return new SagaStateEntity
        {
            CorrelationId = state.CorrelationId,
            ConversationId = state.ConversationId,
            StateType = typeof(TState).FullName ?? typeof(TState).Name,
            StateData = JsonSerializer.Serialize(state, SerializerOptions),
            UpdatedTime = DateTimeOffset.UtcNow
        };
    }

    internal TState ToState<TState>()
        where TState : class, ISagaState
    {
        return JsonSerializer.Deserialize<TState>(StateData, SerializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize saga state.");
    }
}
