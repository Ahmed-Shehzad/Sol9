using System.Threading.Channels;

namespace Transponder.Abstractions;

public class InMemoryAsyncEventBus : IBusPublisher, IBusConsumer
{
    private readonly Dictionary<Type, object> _channels = new();

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
    {
        var channel = GetOrCreateChannel<TEvent>();
        await channel.Writer.WriteAsync(@event, cancellationToken);
    }

    public IAsyncEnumerable<TEvent> ConsumeAsync<TEvent>(CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
    {
        var channel = GetOrCreateChannel<TEvent>();
        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    private Channel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : IIntegrationEvent
    {
        var type = typeof(TEvent);
        if (_channels.TryGetValue(type, out var obj)) return (Channel<TEvent>)obj;

        var newChannel = Channel.CreateUnbounded<TEvent>();
        _channels[type] = newChannel;
        return newChannel;
    }
}
