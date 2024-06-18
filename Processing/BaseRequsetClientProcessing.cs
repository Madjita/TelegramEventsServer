namespace Processing;

public class BaseRequsetClientProcessing
{
    public Dictionary<string, string> QueryData { get; protected set; } = new Dictionary<string, string>();
    public Dictionary<string, string> BodyData { get; protected set; } = new Dictionary<string, string>();
    public virtual string  RelativeUrl { get; protected set; }
    public  string AppId { get; set; }

    public BaseRequsetClientProcessing()
    {
    }
}