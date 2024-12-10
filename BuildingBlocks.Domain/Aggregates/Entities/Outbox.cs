using BuildingBlocks.Contracts.Types;

namespace BuildingBlocks.Domain.Aggregates.Entities;

/// <summary>
/// Represents an outbox entity that holds messages to be processed outside the domain.
/// </summary>
public class Outbox() : BaseEntity(Ulid.NewUlid())
{
    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// Gets a value indicating whether the message has been processed.
    /// </summary>
    public bool Processed { get; private set; }

    /// <summary>
    /// Marks the message as processed.
    /// </summary>
    public void MarkAsProcessed()
    {
        Processed = true;
    }
    /// <summary>
    /// Creates a new instance of the <see cref="Outbox"/> class with the specified type and payload.
    /// </summary>
    /// <param name="type">The type of the message.</param>
    /// <param name="payload">The content of the message.</param>
    /// <returns>A new instance of the <see cref="Outbox"/> class with the provided type and payload.</returns>
    public static Outbox Create(string type, string payload)
    {
        return new Outbox
        {
            Type = type,
            Payload = payload
        };
    }
}