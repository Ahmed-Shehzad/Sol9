using System.Text.Json;
using Transponder.Core.Abstractions;
using Transponder.Core.Types;

namespace Transponder.Storage.Outbox;

public class OutboxMessage : SoftDeletableEntity<Ulid>
{
    public OutboxMessage() : base(Ulid.NewUlid())
    {
    }
    
    public static OutboxMessage Create<T>(T data) where T : IIntegrationEvent
    {
        var message = new OutboxMessage();
        message.SetMessage(data);

        return message;
    }
 
    public string Type { get; private set; }
    public JsonElement Data { get; private set; }
    
    public DateOnly? PublishedDateUtcAt { get; private set; }
    public TimeOnly? PublishedTimeUtcAt { get; private set; }
    
    public string? Error { get; private set; }

    public void MarkAsPublished()
    {
        var utcNow = DateTime.UtcNow;

        PublishedDateUtcAt = DateOnly.FromDateTime(utcNow);
        PublishedTimeUtcAt = TimeOnly.FromDateTime(utcNow);
        Error = null;
    }
    
    public void MarkAsFailed(Exception exception)
    {
        PublishedDateUtcAt = null;
        PublishedTimeUtcAt = null;
        Error = exception.Message;
    }
    
    public bool IsUnprocessed()
    {
        return !PublishedDateUtcAt.HasValue && !PublishedTimeUtcAt.HasValue && string.IsNullOrWhiteSpace(Error);
    }
    
    public bool IsPublished()
    {
        return PublishedDateUtcAt.HasValue && PublishedTimeUtcAt.HasValue && string.IsNullOrWhiteSpace(Error);
    }
    
    public bool IsFailed()
    {
        return !IsPublished();
    }
    
    public T? GetMessage<T>() where T : IIntegrationEvent
    {
        return JsonSerializer.Deserialize<T>(Data.GetRawText());
    }
    
    public void SetMessage<T>(T data) where T : IIntegrationEvent
    {
        var type = typeof(T).FullName;
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        
        Type = type;
        Data = JsonSerializer.SerializeToElement(data);
    }
}