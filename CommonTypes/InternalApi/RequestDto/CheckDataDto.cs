using CommonTypes.RabbitDto;
using DataBase.Entities.QrCodeEntities;
using Newtonsoft.Json;

namespace MailParserMicroService.RequestDto;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckDataDto : IBaseRabbitDto
{
    [JsonProperty(PropertyName = "t")]
    public string T { get; set; }
    
    [JsonProperty(PropertyName = "s")]
    public string S { get; set; }
    
    [JsonProperty(PropertyName = "fn")]
    public string Fn { get; set; }
    
    [JsonProperty(PropertyName = "i")]
    public string I { get; set; }
    
    [JsonProperty(PropertyName = "fp")]
    public string Fp { get; set; }
    
    [JsonProperty(PropertyName = "n")]
    public string N { get; set; }
    
    public static CheckDataDto? FromString(string data)
    {
        var task = JsonConvert.DeserializeObject<CheckDataDto>(data);
        return task;
    }
    
    public string GetRawStr()
    {
        return $"t={T}&s={S}&fn={Fn}&i={I}&fp={Fp}&n={N}";
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}