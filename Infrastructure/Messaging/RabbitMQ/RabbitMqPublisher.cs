using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging.RabbitMQ;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _defaultExchange;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private bool _isInitialized = false;

    public RabbitMqPublisher(string host, int port, string username, string password, string virtualHost, string exchange)
    {
        _defaultExchange = exchange;
        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
            VirtualHost = virtualHost,
            Ssl = new SslOption
            {
                Enabled = port == 5671,
                ServerName = host,
                AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
            }
        };
    }

    private async Task EnsureConnectionAsync()
    {
        if (_isInitialized && _connection != null && _channel != null)
            return;

        await _channelLock.WaitAsync();
        try
        {
            if (_isInitialized && _connection != null && _channel != null)
                return;

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _isInitialized = true;

            Console.WriteLine($"[Publisher] Connection established to RabbitMQ");
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public Task PublishAsync<T>(T message)
    {
        return PublishToExchangeAsync(message, _defaultExchange);
    }

    public async Task PublishToExchangeAsync<T>(T message, string exchange)
    {
        try
        {
            await EnsureConnectionAsync();

            if (_channel == null)
            {
                throw new InvalidOperationException("Channel is not initialized");
            }

            await _channelLock.WaitAsync();
            try
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Fanout,
                    durable: true);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var eventName = typeof(T).Name;
                var props = new BasicProperties
                {
                    Type = eventName,
                    DeliveryMode = DeliveryModes.Persistent
                };

                await _channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: "",
                    mandatory: false,
                    basicProperties: props,
                    body: body);

                Console.WriteLine($"[Publisher] Successfully published {eventName} to exchange {exchange}");
            }
            finally
            {
                _channelLock.Release();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Publisher] ERROR: Failed to publish {typeof(T).Name} to exchange {exchange}. Error: {ex.Message}");
            throw new InvalidOperationException($"Failed to publish message to RabbitMQ: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channelLock?.Dispose();
    }
}