using MyLoggerNamespace;
using Processing.ProverkaCheckApi.Response;

namespace Processing.ProverkaCheckApi;

public class ProverkaCheckApiClient : BaseProcessingClient
{
    public readonly string _apiToken;
    
    public ProverkaCheckApiClient(IMyLogger logger) : base(logger)
    {
        _host = new Uri("https://proverkacheka.com/", UriKind.Absolute);
    }
    
    public async Task<ProverkaCheckApiResponseDto?> SendAsync(BaseRequsetClientProcessing request)
    {
        var url = new Uri(_host, request.RelativeUrl);

        var headers = new Dictionary<string, string>();
        return await GetAsync<ProverkaCheckApiResponseDto>(request, url, headers);
    }
    
    public async Task<ProverkaCheckApiResponseDto?> SendPostAsync(BaseRequsetClientProcessing request)
    {
        var url = new Uri(_host, request.RelativeUrl);

        var headers = new Dictionary<string, string>();
        // headers.Add("User-Agent","PostmanRuntime/7.39.0");
        // headers.Add("Accept-Encoding","gzip, deflate, br");
        // headers.Add("Connection","keep-alive");
        //headers.Add("Host","proverkacheka.com");
        
        var response = await PostAsync<ProverkaCheckApiResponseDto>(request, url, headers);;
        return response.Item1;
    }
}