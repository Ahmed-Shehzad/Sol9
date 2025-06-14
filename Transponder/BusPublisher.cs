using Microsoft.Extensions.Logging;
using Transponder.Core.Abstractions;

namespace Transponder;

internal class BusPublisher : IBusPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusPublisher> _logger;

    public BusPublisher(IServiceProvider serviceProvider, ILogger<BusPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
    {
        var notificationType = typeof(TEvent);

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(notificationType);
        var handlers = (_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType)) as IEnumerable<object>
                        ?? [])
            .Cast<dynamic>()
            .ToList();

        var tasks = handlers.Select(async handler =>
        {
            try
            {
                await handler.HandleAsync((dynamic)@event, cancellationToken);
            }
            catch (Exception ex)
            {
                // Optionally log error somewhere
                _logger.LogError(ex, "Error handling @event {NotificationType}", notificationType.FullName);
                throw;
            }
        }).ToList();

        await Task.WhenAll(tasks);
    }
}