using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Helpers;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Processing.ProverkaCheckApi.Response;
using Utils;

namespace Processing;

public class BaseProcessingClient
{
    protected readonly IMyLogger _logger;

    protected const string ContentType = "application/json";
    protected static readonly JsonSerializerSettings JsonSerializerSettings = new();
    protected Uri? _host;
    protected readonly HttpClient _client;

    X509Certificate2? _certificate;

    public BaseProcessingClient(IMyLogger logger)
    {
        _client = HttpClientFactory.Get();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(BaseRequsetClientProcessing request, Uri url, Dictionary<string, string>? headers = null) where T : class
    {
        var queryParamsString = request.QueryData?.ConvertToKeyValuesStringForQuery() ?? string.Empty;
        var messageRequest = new HttpRequestMessage(HttpMethod.Get, $"{url}?{queryParamsString}");

        if(headers is not null)
        {
            foreach( var header in headers )
            {
                messageRequest.Headers.Add(header.Key, header.Value);
            }
        }

        _logger.WriteLine(MessageType.Info, $"[BaseProcessingClient] Sending {request.GetType().Name} to {url}, queryParamsString=[{queryParamsString}  messageRequest.Headers=[{messageRequest.Headers}]");


        string? content = null;
        HttpStatusCode status = 0;
        try
        {
            var response = await _client.SendAsync(messageRequest);
            status = response.StatusCode;
            if (response.StatusCode < HttpStatusCode.InternalServerError)
            {
                content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK || content?.Length > 0)
                {
                    _logger.WriteLine(MessageType.Info, $"[BaseProcessingClient][{request.GetType().Name}] [{url}]. Status:{status} Content:{content}.");
                    T responseDto = null;

                    try
                    {
                        responseDto = JsonHelper.Deserialize<T>(content);
                    }
                    catch (Exception ex)
                    {
                        responseDto = (T)Activator.CreateInstance(type: typeof(T), new object[0] { });
                    }

                    return responseDto;
                }

                _logger.WriteLine(MessageType.Error, $"[BaseProcessingClient][{request.GetType().Name}] Empty response from {url}. Status:{status}");
            }
            else
            {
                _logger.WriteLine(MessageType.Error, $"[BaseProcessingClient][{request.GetType().Name}] Unexpected response from {url}. Status:{status}");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.WriteLine(ex, $"[BankOfGeorgiaProcessingClient][{request.GetType().Name}] Unexpected response from {url}. Status:{status} Content:`{content}`");
            return null;
        }
    }
    
    public async Task<(T?, string ContentResponse)> PostAsync<T>(BaseRequsetClientProcessing request, Uri url, Dictionary<string, string>? headers = null) where T : class
    {
        var queryParamsString = request.QueryData?.ConvertToKeyValuesStringForQuery() ?? string.Empty;
        
        var contentQueryData = new FormUrlEncodedContent(request.QueryData);
        
        var messageRequest = new HttpRequestMessage(HttpMethod.Post, $"{url}")
        {
            Content = contentQueryData
        };

        if(headers is not null)
        {
            foreach( var header in headers )
            {
                messageRequest.Headers.Add(header.Key, header.Value);
            }
        }

        _logger.WriteLine(MessageType.Info, $"[BaseProcessingClient] Sending {request.GetType().Name} to {url}, queryParamsString=[{queryParamsString}  messageRequest.Headers=[{messageRequest.Headers}]");


        string? content = null;
        HttpStatusCode status = 0;
        try
        {
            var response = await _client.SendAsync(messageRequest);
            status = response.StatusCode;
            if (response.StatusCode < HttpStatusCode.InternalServerError)
            {
                content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK || content?.Length > 0)
                {
                    _logger.WriteLine(MessageType.Info, $"[BaseProcessingClient][{request.GetType().Name}] [{url}]. Status:{status} Content:{content}.");
                    T? responseDto = null;

                    try
                    {
                        responseDto = JsonHelper.Deserialize<T>(content);
                    }
                    catch (Exception ex)
                    {
                        responseDto = (T)Activator.CreateInstance(type: typeof(T), new object[0] { });
                    }

                    return (responseDto, content);
                }

                _logger.WriteLine(MessageType.Error, $"[BaseProcessingClient][{request.GetType().Name}] Empty response from {url}. Status:{status}");
            }
            else
            {
                _logger.WriteLine(MessageType.Error, $"[BaseProcessingClient][{request.GetType().Name}] Unexpected response from {url}. Status:{status}");
            }

            return (null, content);
        }
        catch (Exception ex)
        {
            _logger.WriteLine(ex, $"[BankOfGeorgiaProcessingClient][{request.GetType().Name}] Unexpected response from {url}. Status:{status} Content:`{content}`");
            return (null, string.Empty);
        }
    }
}