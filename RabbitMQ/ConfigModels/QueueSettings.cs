namespace RabbitMQ.ConfigModels;

public class QueueSettings
{
    public string StartClassName { get; set; }
    public string QueueName { get; set; }

    public string Exchange { get; set; }
    public string ExchangeRoutingKey { get; set; }
    
    public string ExchangeDelay { get; set; }
    public string ExchangeDelayRoutingKey { get; set; }
    
}