namespace RabbitMQ;

public interface IProducer: IDisposable
{
    void Publish(string msg);

    void PublishWithDelay(string msg, int delayInMilliseconds);
    Action<byte[]> OnFail { get; set; }
    Action<ulong> OnSuccess { get; set; }
    void Quit();
}