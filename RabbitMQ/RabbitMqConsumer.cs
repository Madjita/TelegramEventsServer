using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.ConfigModels;

namespace RabbitMQ;

public class RabbitMqConsumer : IConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private string _queueName;
    private EventingBasicConsumer _consumer;
    private EventHandler<string> _onReceived;
    
    public RabbitMqConsumer(IConnection connection, RabbitMQSettings settings)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
    }
    
    public void Start(string queueName, string routingKey, Func<string, Task> onReceived)
    {
        _queueName = queueName;
        
        // Объявляем очередь, если она еще не объявлена
        _channel.QueueDeclare(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Объявляем обмен, если это необходимо
        _channel.ExchangeDeclare(exchange: routingKey, type: "direct");

        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await onReceived(message);
        };

        _channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: _consumer);
    }

    public void Stop()
    {
        _channel.BasicCancel(_queueName);
        _consumer = null;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
