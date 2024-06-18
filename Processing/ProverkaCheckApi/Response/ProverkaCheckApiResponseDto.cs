using Newtonsoft.Json;

namespace Processing.ProverkaCheckApi.Response;

public interface IProverkaCheckApiApiResponseDto
{

}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class ErrorProverkaCheckApiResponseDto : IProverkaCheckApiApiResponseDto
{
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }
    
    [JsonProperty(PropertyName = "data")]
    public string? Data { get; set; }
}


[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class ProverkaCheckApiResponseDto : IProverkaCheckApiApiResponseDto
{
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }
    
    [JsonProperty(PropertyName = "first")]
    public string? First { get; set; }
    
    [JsonProperty(PropertyName = "data")]
    public DataProverkaCheckApiDto? Data { get; set; }
    
    [JsonProperty(PropertyName = "request")]
    public RequestProverkaCheckApiDto? Request { get; set; }
    
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class DataProverkaCheckApiDto
{
    [JsonProperty(PropertyName = "json")]
    public DataJsonProverkaCheckApiDto Json { get; set; }
    
    [JsonProperty(PropertyName = "html")]
    public string Html { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class DataJsonProverkaCheckApiDto
{
    [JsonProperty(PropertyName = "code")]
    public string Code { get; set; }
    
    [JsonProperty(PropertyName = "user")]
    public string User { get; set; }
    
    [JsonProperty(PropertyName = "items")]
    public List<CheckItem> CheckItems { get; set; }
    
    [JsonProperty(PropertyName = "nds10")]
    public string Nds10 { get; set; }
    
    [JsonProperty(PropertyName = "nds18")]
    public string Nds18 { get; set; }
    
    [JsonProperty(PropertyName = "fnsUrl")]
    public string FnsUrl { get; set; }
    
    [JsonProperty(PropertyName = "region")]
    public string Region { get; set; }
    
    [JsonProperty(PropertyName = "userInn")]
    public string UserInn { get; set; }
    
    [JsonProperty(PropertyName = "dateTime")]
    public string DateTime { get; set; }
    
    [JsonProperty(PropertyName = "kktRegId")]
    public string KktRegId { get; set; }
    
    //metadata
    
    [JsonProperty(PropertyName = "operator")]
    public string Operator { get; set; }
    
    [JsonProperty(PropertyName = "totalSum")]
    public string TotalSum { get; set; }
    
    [JsonProperty(PropertyName = "creditSum")]
    public string CreditSum { get; set; }
    
    [JsonProperty(PropertyName = "numberKkt")]
    public string NumberKkt { get; set; }
    
    [JsonProperty(PropertyName = "fiscalSign")]
    public string FiscalSign { get; set; }
    
    [JsonProperty(PropertyName = "prepaidSum")]
    public string PrepaidSum { get; set; }
    
    [JsonProperty(PropertyName = "retailPlace")]
    public string RetailPlace { get; set; }
    
    [JsonProperty(PropertyName = "shiftNumber")]
    public string ShiftNumber { get; set; }
    
    [JsonProperty(PropertyName = "cashTotalSum")]
    public string CashTotalSum { get; set; }
    
    [JsonProperty(PropertyName = "provisionSum")]
    public string ProvisionSum { get; set; }
    
    [JsonProperty(PropertyName = "ecashTotalSum")]
    public string EcashTotalSum { get; set; }
    
    [JsonProperty(PropertyName = "operationType")]
    public string OperationType { get; set; }
    
    [JsonProperty(PropertyName = "redefine_mask")]
    public string Redefine_mask { get; set; }
    
    [JsonProperty(PropertyName = "requestNumber")]
    public string RequestNumber { get; set; }

    [JsonProperty(PropertyName = "sellerAddress")]
    public string SellerAddress { get; set; }
    
    [JsonProperty(PropertyName = "fiscalDriveNumber")]
    public string FiscalDriveNumber { get; set; }
    
    [JsonProperty(PropertyName = "messageFiscalSign")]
    public string MessageFiscalSign { get; set; }
    
    [JsonProperty(PropertyName = "retailPlaceAddress")]
    public string RetailPlaceAddress { get; set; }
    
    [JsonProperty(PropertyName = "appliedTaxationType")]
    public string AppliedTaxationType { get; set; }
    
    [JsonProperty(PropertyName = "buyerPhoneOrAddress")]
    public string BuyerPhoneOrAddress { get; set; }
    
    [JsonProperty(PropertyName = "fiscalDocumentNumber")]
    public string FiscalDocumentNumber { get; set; }
    
    [JsonProperty(PropertyName = "fiscalDocumentFormatVer")]
    public string FiscalDocumentFormatVer { get; set; }
    
    [JsonProperty(PropertyName = "checkingLabeledProdResult")]
    public string CheckingLabeledProdResult { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class CheckItem
{
    [JsonProperty(PropertyName = "nds")]
    public int Nds { get; set; }
    
    [JsonProperty(PropertyName = "sum")]
    public int Sum { get; set; }
    
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
    
    [JsonProperty(PropertyName = "price")]
    public int Price { get; set; }
    
    [JsonProperty(PropertyName = "ndsSum")]
    public int NdsSum { get; set; }
    
    [JsonProperty(PropertyName = "quantity")]
    public double Quantity { get; set; }
    
    [JsonProperty(PropertyName = "paymentType")]
    public int PaymentType { get; set; }
    
    [JsonProperty(PropertyName = "productType")]
    public int ProductType { get; set; }
    
    [JsonProperty(PropertyName = "itemsQuantityMeasure")]
    public int ItemsQuantityMeasure { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class RequestProverkaCheckApiDto
{
    [JsonProperty(PropertyName = "qrurl")]
    public string Qrurl { get; set; }
    
    [JsonProperty(PropertyName = "qrfile")]
    public string Qrfile { get; set; }
    
    [JsonProperty(PropertyName = "qrraw")]
    public string Qrraw { get; set; }
    
    [JsonProperty(PropertyName = "manual")]
    public Dictionary<string,string> Manual { get; set; }
}