using DataBase.Entities.QrCodeEntities;
using Newtonsoft.Json;

namespace MailParserMicroService.ResponseDto;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckProcessedResponse
{
    [JsonProperty(PropertyName = "dateTime")]
    public DateTime DateTime { get; set; }
    
    [JsonProperty(PropertyName = "name")]
    public string Name  { get; set; }
    
    [JsonProperty(PropertyName = "inn")]
    public long Inn  { get; set; }
    
    [JsonProperty(PropertyName = "retailPlace")]
    public string RetailPlace { get; set; }
    
    [JsonProperty(PropertyName = "retailPlaceAddress")]
    public string RetailPlaceAddress { get; set; }
    
    [JsonProperty(PropertyName = "sellerAddress")]
    public string SellerAddress { get; set; }
    
    [JsonProperty(PropertyName = "checkRawData")]
    public string CheckRawData { get; set; }
    
    public ICollection<CheckProcessedItemResponse> CheckProcessedItems { get; set; } =
        new List<CheckProcessedItemResponse>();

}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckProcessedItemResponse
{
    [JsonProperty(PropertyName = "name")]
    public string Name  { get; set; }
    
    [JsonProperty(PropertyName = "sum")]
    public int Sum  { get; set; }
    
    [JsonProperty(PropertyName = "nds")]
    public int Nds  { get; set; }
    
    [JsonProperty(PropertyName = "price")]
    public int Price  { get; set; }
    
    [JsonProperty(PropertyName = "ndssum")]
    public int NdsSum  { get; set; }
    
    [JsonProperty(PropertyName = "quantity")]
    public double Quantity  { get; set; }
    
    [JsonProperty(PropertyName = "paymenttype")]
    public int Paymenttype  { get; set; }
    
    [JsonProperty(PropertyName = "producttype")]
    public int Producttype  { get; set; }
    
    [JsonProperty(PropertyName = "itemsquantitymeasure")]
    public int Itemsquantitymeasure  { get; set; }
}