using RabbitMQ.Client;

namespace Transponder.RabbitMQ;

public interface IRabbitMqConnectionFactory
{
    
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}

public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly ConnectionFactory _connection;
    private IConnection? _openConnection;
    
    public RabbitMqConnectionFactory(ConnectionFactory connection)
    {
        _connection = connection;
    }
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_openConnection is { IsOpen: true })
        {
            return _openConnection;
        }
        _openConnection = await _connection.CreateConnectionAsync(cancellationToken);
        return _openConnection;
    }
}