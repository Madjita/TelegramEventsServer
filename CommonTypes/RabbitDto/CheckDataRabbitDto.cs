using DataBase.Entities.QrCodeEntities;
using MailParserMicroService.RequestDto;
using Newtonsoft.Json;

namespace CommonTypes.RabbitDto;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckDataRabbitDto : CheckDataDto
{
    [JsonProperty(PropertyName = "processed")]
    public CheckProcessed Processed  { get; set; }
    
    [JsonProperty(PropertyName = "repeatCount")]
    public int RepeatCount { get; set; }
    
    [JsonProperty(PropertyName = "repeatDelayMs")]
    public int RepeatDelayMs { get; set; }
    
    public static CheckDataRabbitDto? FromString(string data)
    {
        var task = JsonConvert.DeserializeObject<CheckDataRabbitDto>(data);
        return task;
    }
}