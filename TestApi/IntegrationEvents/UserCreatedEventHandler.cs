using Transponder.Abstractions;

namespace TestApi.IntegrationEvents;

public sealed class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedEvent>
{
    private  readonly ILogger<UserCreatedEventHandler> _logger;
    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }
    public Task HandleAsync(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User created: {Name}", notification.Name);
        return Task.CompletedTask;
    }
}