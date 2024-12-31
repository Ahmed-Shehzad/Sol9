namespace BuildingBlocks.Domain.Aggregates.Entities;

/// <summary>
/// Represents an outbox entity that holds messages to be processed outside the domain.
/// </summary>
public class Outbox : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Outbox"/> class with the specified type and payload.
    /// </summary>
    /// <param name="type">The type of the message. This parameter is used to categorize the message.</param>
    /// <param name="payload">The content of the message. This parameter holds the data to be processed outside the domain.</param>
    private Outbox(string type, string payload) : base(Ulid.NewUlid())
    {
        Type = type;
        Payload = payload;
    }

    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    public string Payload { get; private set; }

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
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        
        return new Outbox(type, payload);
    }
}