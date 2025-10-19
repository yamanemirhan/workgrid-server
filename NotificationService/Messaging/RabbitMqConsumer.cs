using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(string host, int port, string username, string password, string virtualHost, string exchange)
    {
        _factory = new ConnectionFactory()
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
            VirtualHost = virtualHost
        };
        _exchange = exchange;
    }

    public void StartConsuming<T>(Func<T, Task> onMessageReceived, string? queuePrefix = null)
    {
        Task.Run(async () =>
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Fanout
                await _channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true);

                var eventName = typeof(T).Name;
                var serviceName = queuePrefix ?? "notification";
                var queueName = $"{serviceName}.{eventName}";

                await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                await _channel.QueueBindAsync(queue: queueName, exchange: _exchange, routingKey: "");

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        if (ea.BasicProperties.Type != eventName)
                        {
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                            return;
                        }

                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var deserializedMessage = JsonSerializer.Deserialize<T>(message);

                        if (deserializedMessage != null)
                        {
                            Console.WriteLine($"[NotificationConsumer] Processing {eventName} from queue {queueName}");
                            await onMessageReceived(deserializedMessage);
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                            Console.WriteLine($"[NotificationConsumer] Successfully processed {eventName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NotificationConsumer] Error processing {eventName}: {ex.Message}");
                        // Reject message and don't requeue
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                Console.WriteLine($"[NotificationConsumer] Started consuming {eventName} messages from queue {queueName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationConsumer] Error starting consumer for {typeof(T).Name}: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}