
using Intercessor.Abstractions;
using Microsoft.Extensions.Logging;

namespace Intercessor;

internal class Publisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Publisher> _logger;

    public Publisher(IServiceProvider serviceProvider, ILogger<Publisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var notificationType = typeof(TNotification);
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var handlers = (_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType)) as IEnumerable<object>
                        ?? [])
            .Cast<dynamic>()
            .ToList();

        var tasks = handlers.Select(async handler =>
        {
            try
            {
                await handler.HandleAsync((dynamic)notification, cancellationToken);
            }
            catch (Exception ex)
            {
                // Optionally log error somewhere
                _logger.LogError(ex, "Error handling notification {NotificationType}", notificationType.FullName);
                throw;
            }
        }).ToList();

        await Task.WhenAll(tasks);
    }
}