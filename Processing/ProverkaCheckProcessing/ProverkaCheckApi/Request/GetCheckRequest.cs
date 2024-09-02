namespace Processing.ProverkaCheckApi.Request;

public class GetCheckRequest : BaseRequsetClientProcessing
{
    public override string RelativeUrl => "api/v1/check/get";
    
    public GetCheckRequest(string qrraw)
    {
        QueryData.Add("qrraw",qrraw);
        QueryData.Add("token","27100.UndioKB9vxulpD1hU");
    }
}