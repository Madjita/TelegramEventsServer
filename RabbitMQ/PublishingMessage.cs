namespace RabbitMQ;

public class PublishingMessage
{
    public string Message { get; private set; }
    public string RoutingKey { get; private set; }
    public PublishingMessageOptions Options { get; private set; }

    public PublishingMessage(string message, string routingKey, PublishingMessageOptions options = null)
    {
        Message = message;
        Options = options;
        RoutingKey = routingKey;
    }
}

public class PublishingMessageOptions
{
    /// <summary>
    ///  Header: "x-delay" 
    /// </summary>
    public int Delay { get; set; }

    public Dictionary<string, object> ToDictionary()
    {
        if (Delay > 0)
        {
            return new Dictionary<string, object> { { "x-delay", Delay } };
        }

        return null;
    }
}