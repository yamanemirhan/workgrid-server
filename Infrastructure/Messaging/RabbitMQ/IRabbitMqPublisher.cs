namespace Infrastructure.Messaging.RabbitMQ;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T message);
    Task PublishToExchangeAsync<T>(T message, string exchange);
}
