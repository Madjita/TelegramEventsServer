using Newtonsoft.Json;

namespace MailParserMicroService.RequestDto;


[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class SaveQrDataRequest
{
    [JsonProperty(PropertyName = "qrdata")]
    public string QrData { get; set; }
}