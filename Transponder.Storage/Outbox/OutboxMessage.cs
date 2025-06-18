using System.Text.Json;
using Transponder.Core.Abstractions;
using Transponder.Core.Types;

namespace Transponder.Storage.Outbox;

public class OutboxMessage : SoftDeletableEntity<Ulid>
{
    public OutboxMessage() : base(Ulid.NewUlid())
    {
    }

    /// <summary>
    /// Creates a new <see cref="OutboxMessage"/> instance from the specified integration event data.
    /// </summary>
    /// <typeparam name="T">The type of the integration event.</typeparam>
    /// <param name="data">The integration event data.</param>
    /// <returns>A new <see cref="OutboxMessage"/> containing the event data.</returns>
    public static OutboxMessage Create<T>(T data) where T : IIntegrationEvent
    {
        var message = new OutboxMessage();
        message.SetMessage(data);

        return message;
    }

    /// <summary>
    /// Gets the type name of the integration event contained in this message.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the serialized data of the integration event as a <see cref="JsonElement"/>.
    /// </summary>
    public JsonElement Data { get; private set; }

    /// <summary>
    /// Gets the UTC date when the message was published, or <c>null</c> if not published.
    /// </summary>
    public DateOnly? PublishedDateUtcAt { get; private set; }

    /// <summary>
    /// Gets the UTC time when the message was published, or <c>null</c> if not published.
    /// </summary>
    public TimeOnly? PublishedTimeUtcAt { get; private set; }

    /// <summary>
    /// Gets the error message if the message failed to be published; otherwise, <c>null</c>.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Marks the message as published by setting the published date and time to the current UTC time and clearing any error.
    /// </summary>
    public void MarkAsPublished()
    {
        var utcNow = DateTime.UtcNow;

        PublishedDateUtcAt = DateOnly.FromDateTime(utcNow);
        PublishedTimeUtcAt = TimeOnly.FromDateTime(utcNow);
        Error = null;
    }

    /// <summary>
    /// Marks the message as failed and records the exception message as the error.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public void MarkAsFailed(Exception exception)
    {
        PublishedDateUtcAt = null;
        PublishedTimeUtcAt = null;
        Error = exception.Message;
    }

    /// <summary>
    /// Determines whether the message is unprocessed (not published and no error).
    /// </summary>
    /// <returns><c>true</c> if the message is unprocessed; otherwise, <c>false</c>.</returns>
    public bool IsUnprocessed()
    {
        return !PublishedDateUtcAt.HasValue && !PublishedTimeUtcAt.HasValue && string.IsNullOrWhiteSpace(Error);
    }

    /// <summary>
    /// Determines whether the message has been published successfully.
    /// </summary>
    /// <returns><c>true</c> if the message is published and has no error; otherwise, <c>false</c>.</returns>
    public bool IsPublished()
    {
        return PublishedDateUtcAt.HasValue && PublishedTimeUtcAt.HasValue && string.IsNullOrWhiteSpace(Error);
    }

    /// <summary>
    /// Determines whether the message has failed to be published.
    /// </summary>
    /// <returns><c>true</c> if the message is not published; otherwise, <c>false</c>.</returns>
    public bool IsFailed()
    {
        return !IsPublished();
    }

    /// <summary>
    /// Deserializes and returns the integration event message of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the integration event.</typeparam>
    /// <returns>The deserialized integration event, or <c>null</c> if deserialization fails.</returns>
    public T? GetMessage<T>() where T : IIntegrationEvent
    {
        return JsonSerializer.Deserialize<T>(Data.GetRawText());
    }

    /// <summary>
    /// Serializes and sets the integration event message and its type.
    /// </summary>
    /// <typeparam name="T">The type of the integration event.</typeparam>
    /// <param name="data">The integration event data to set.</param>
    public void SetMessage<T>(T data) where T : IIntegrationEvent
    {
        var type = typeof(T).FullName;
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        Type = type;
        Data = JsonSerializer.SerializeToElement(data);
    }
}