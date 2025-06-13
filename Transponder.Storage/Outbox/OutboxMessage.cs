using System.Text.Json;
using Transponder.Core.Types;

namespace Transponder.Storage.Outbox;

public class OutboxMessage : SoftDeletableEntity<Ulid>
{
    public OutboxMessage() : base(Ulid.NewUlid())
    {
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
    }
    
    public void MarkAsFailed(Exception exception)
    {
        Error = exception.Message;
    }
    
    public bool IsPublished()
    {
        return PublishedDateUtcAt.HasValue && PublishedTimeUtcAt.HasValue;
    }
    
    public T? GetMessage<T>()
    {
        return JsonSerializer.Deserialize<T>(Data.GetRawText());
    }
    
    public void SetMessage<T>(T data)
    {
        var type = typeof(T).FullName;
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        
        Type = type;
        Data = JsonSerializer.SerializeToElement(data);
    }
}