namespace RabbitMQ.ConfigModels;

public class RabbitMQSettings
{
    public bool Start { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
    public string RoutingKeyTimingAPI { get; set; }
    public string RoutingKeyTimingProcessings { get; set; }
    public List<QueueSettings> Queues { get; set; }
}
