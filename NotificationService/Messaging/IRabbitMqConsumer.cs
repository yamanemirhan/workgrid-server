namespace NotificationService.Messaging;

public interface IRabbitMqConsumer
{
    void StartConsuming<T>(Func<T, Task> onMessageReceived, string? queuePrefix = null);
}