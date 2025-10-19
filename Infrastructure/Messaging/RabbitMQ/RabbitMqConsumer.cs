using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging.RabbitMQ;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized = false;

    public RabbitMqConsumer(string host, int port, string username, string password, string virtualHost, string exchange)
    {
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
        _exchange = exchange;
    }

    private async Task EnsureConnectionAsync()
    {
        if (_isInitialized && _connection != null && _channel != null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized && _connection != null && _channel != null)
                return;

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: _exchange,
                type: ExchangeType.Fanout,
                durable: true);

            _isInitialized = true;
            Console.WriteLine($"[Consumer] Connection established to RabbitMQ");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async void StartConsuming<T>(Func<T, Task> onMessageReceived, string queuePrefix)
    {
        var eventName = typeof(T).Name;
        var queueName = $"{queuePrefix}.{eventName}.queue";

        try
        {
            await EnsureConnectionAsync();

            if (_channel == null)
            {
                Console.WriteLine($"[Consumer] Error: Channel is null for {eventName}");
                return;
            }

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: queueName,
                exchange: _exchange,
                routingKey: "");

            Console.WriteLine($"[Consumer] Listening for {eventName} on queue {queueName}");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    if (ea.BasicProperties.Type != eventName)
                    {
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message != null)
                    {
                        Console.WriteLine($"[Consumer] Processing {eventName}...");
                        await onMessageReceived(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        Console.WriteLine($"[Consumer] Done processing {eventName}.");
                    }
                    else
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Consumer] Error while processing {eventName}: {ex.Message}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Consumer] Error starting consumer for {eventName}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _initLock?.Dispose();
    }
}