using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ActivityService.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(string host, int port, string username, string password, string virtualHost, string exchange)
    {
        _exchange = exchange;
        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
            VirtualHost = virtualHost
        };
    }

    public void StartConsuming<T>(Func<T, Task> onMessageReceived, string? queuePrefix = null)
    {
        Task.Run(async () =>
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(_exchange, ExchangeType.Fanout, durable: true);

                var eventName = typeof(T).Name;
                var serviceName = queuePrefix ?? "activity";
                var queueName = $"{serviceName}.{eventName}";

                await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
                await _channel.QueueBindAsync(queueName, _exchange, routingKey: "");

                Console.WriteLine($"[ActivityConsumer] Listening {eventName} on queue: {queueName}");

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
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
                            Console.WriteLine($"[ActivityConsumer] Processing {eventName} from queue {queueName}.");
                            await onMessageReceived(message);
                            Console.WriteLine($"[ActivityConsumer] Successfully processed {eventName}.");
                            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ActivityConsumer] Error processing {eventName} on {queueName}: {ex.Message}");
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ActivityConsumer] Error starting consumer for {typeof(T).Name}: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}