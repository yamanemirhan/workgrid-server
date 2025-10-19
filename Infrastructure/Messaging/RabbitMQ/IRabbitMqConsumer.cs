namespace Infrastructure.Messaging.RabbitMQ;

public interface IRabbitMqConsumer
{
    void StartConsuming<T>(Func<T, Task> onMessageReceived, string queuePrefix);
}