using Newtonsoft.Json;

namespace MailParserMicroService.RequestDto;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckDataDto
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
}