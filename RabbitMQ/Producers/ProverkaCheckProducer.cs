using MyLoggerNamespace.Enums;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQ.ConfigModels;

namespace Workers.Producers;

public class ProverkaCheckProducer : IProducer
{
    private readonly IModel _channel;
    private readonly MyLoggerNamespace.Logger _logger;
    private readonly QueueSettings _queueSettings;

    public Action<byte[]> OnFail { get; set; }
    public Action<ulong> OnSuccess { get; set; }

    public ProverkaCheckProducer()
    {
        _logger = new MyLoggerNamespace.Logger(nameof(ProverkaCheckProducer));
    }
    public ProverkaCheckProducer(IConnection connection,  QueueSettings queueSettings) : this()
    {
        _channel = connection.CreateModel();
        _queueSettings = queueSettings;

        try
        {
            // Обычный обменник
            _channel.ExchangeDeclare(exchange: _queueSettings.Exchange, type: "direct", durable: true);

            // Отложенный обменник
            _channel.ExchangeDeclare(exchange: _queueSettings.ExchangeDelay, type: "x-delayed-message", durable: true, arguments: new Dictionary<string, object>
            {
                { "x-delayed-type", "direct" }
            });
            
            _channel.QueueDeclare(queue: _queueSettings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            _channel.QueueBind(queue: _queueSettings.QueueName,
                exchange: _queueSettings.Exchange,
                routingKey: _queueSettings.ExchangeRoutingKey);
            
            _channel.QueueBind(queue: _queueSettings.QueueName,
                exchange: _queueSettings.ExchangeDelay,
                routingKey: _queueSettings.ExchangeDelayRoutingKey);

            _channel.BasicAcks += OnSuccessHandler;
        }
        catch (Exception ex)
        {
            _logger.WriteLine(ex, $"[{nameof(ProverkaCheckProducer)}] Ошибка при публикации сообщения"); 
        }
    }

    public void Publish(string msg)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(msg);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: _queueSettings.Exchange,
                routingKey: _queueSettings.ExchangeRoutingKey,
                basicProperties: properties,
                body: body);
            
            _logger.WriteLine(MessageType.Info, $"[{nameof(ProverkaCheckProducer)}] Опубликовано сообщение в обмен '{_queueSettings.Exchange}', ключ маршрутизации '{ _queueSettings.ExchangeRoutingKey}'");
        }
        catch (Exception ex)
        {
            OnFail?.Invoke(Encoding.UTF8.GetBytes(ex.Message));
            _logger.WriteLine(ex, $"[{nameof(ProverkaCheckProducer)}] Ошибка при публикации сообщения");
        }
    }
    
    public void PublishWithDelay(string msg, int delayInMilliseconds)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(msg);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Headers = new Dictionary<string, object>
            {
                { "x-delay", delayInMilliseconds }
            };

            _channel.BasicPublish(exchange: _queueSettings.ExchangeDelay,
                routingKey: _queueSettings.ExchangeDelayRoutingKey,
                basicProperties: properties,
                body: body);
        
            _logger.WriteLine(MessageType.Info, $"[{nameof(ProverkaCheckProducer)}] Опубликовано сообщение в обмен 'my-delayed-exchange', ключ маршрутизации '{_queueSettings.ExchangeDelayRoutingKey}', с задержкой {delayInMilliseconds} мс");
        }
        catch (Exception ex)
        {
            OnFail?.Invoke(Encoding.UTF8.GetBytes(ex.Message));
            _logger.WriteLine(ex, $"[{nameof(ProverkaCheckProducer)}] Ошибка при публикации сообщения с задержкой");
        }
    }


    public void Quit()
    {
        _channel.Dispose();
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
    
    private void OnSuccessHandler(object sender, RabbitMQ.Client.Events.BasicAckEventArgs e)
    {
        OnSuccess?.Invoke(e.DeliveryTag);
    }
}
