namespace RabbitMQ;

public interface IConsumer
{
    void Start(string queueName, string routingKey, Func<string, Task> onReceived);
    void Stop();
}