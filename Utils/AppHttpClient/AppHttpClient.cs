using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace Utils
{
    public class HttpClientFactory
    {
        private const string DEFAULT_NAME = "default";

        private static ConcurrentDictionary<string, IHttpClientFactory> _httpClientFactories = new ConcurrentDictionary<string, IHttpClientFactory>();
        private static ConcurrentDictionary<string, object> _httpClientFactoriesLock = new ConcurrentDictionary<string, object>();

        private static AsyncRetryPolicy _retryPolicy = null;

        static HttpClientFactory()
        {
            _retryPolicy = Policy.Handle<Exception>(
                  ex =>
                    ex is HttpRequestException &&
                    ex.FromHierarchy(e => e.InnerException)
                        .Any(e => e.GetType().Name.Equals(typeof(System.ComponentModel.Win32Exception).Name))
              )
              .WaitAndRetryAsync(new[]
              {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
              });

            _httpClientFactories[DEFAULT_NAME] = CreateHttpClientFactory(DEFAULT_NAME, new HttpClientHandler());
        }

        private static IHttpClientFactory CreateHttpClientFactory(string name, HttpClientHandler handler)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient(name)
                .AddPolicyHandler(_retryPolicy.AsAsyncPolicy<HttpResponseMessage>())
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var h = new HttpClientHandler
                    {
                        UseCookies = handler.UseCookies,
                        SslProtocols = handler.SslProtocols,
                        ServerCertificateCustomValidationCallback = handler.ServerCertificateCustomValidationCallback,
                        Proxy = handler.Proxy,
                        PreAuthenticate = handler.PreAuthenticate,
                        MaxResponseHeadersLength = handler.MaxResponseHeadersLength,
                        MaxRequestContentBufferSize = handler.MaxRequestContentBufferSize,
                        MaxConnectionsPerServer = handler.MaxConnectionsPerServer,
                        MaxAutomaticRedirections = handler.MaxAutomaticRedirections,
                        DefaultProxyCredentials = handler.DefaultProxyCredentials,
                        Credentials = handler.Credentials,
                        CookieContainer = handler.CookieContainer,
                        ClientCertificateOptions = handler.ClientCertificateOptions,
                        CheckCertificateRevocationList = handler.CheckCertificateRevocationList,
                        AutomaticDecompression = handler.AutomaticDecompression,
                        AllowAutoRedirect = handler.AllowAutoRedirect,
                        UseDefaultCredentials = handler.UseDefaultCredentials,
                        UseProxy = false,
                    };

                    foreach (var cert in handler.ClientCertificates)
                        h.ClientCertificates.Add(cert);

                    foreach (var prop in handler.Properties)
                        h.Properties.Add(prop);

                    return h;
                });
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetService<IHttpClientFactory>();
        }

        private static IHttpClientFactory CreateHttpClientFactory(string name, Action<HttpClientHandler> configure)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient(name)
                .AddPolicyHandler(_retryPolicy.AsAsyncPolicy<HttpResponseMessage>())
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var h = new HttpClientHandler { UseProxy = false };
                    configure(h);
                    return h;
                });
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetService<IHttpClientFactory>();
        }

        public static HttpClient Get(Action<HttpRequestHeaders> setHeaders = null)
        {
            var client = _httpClientFactories[DEFAULT_NAME].CreateClient(DEFAULT_NAME);
            setHeaders?.Invoke(client.DefaultRequestHeaders);
            return client;
        }

        public static HttpClient Get(string name)
        {
            if (_httpClientFactories.ContainsKey(name))
                return _httpClientFactories[name].CreateClient(name);

            return null;
        }

        public static HttpClient Create(string name, HttpClientHandler handler)
        {
            if (_httpClientFactories.ContainsKey(name))
                return _httpClientFactories[name].CreateClient(name);

            if (!_httpClientFactoriesLock.ContainsKey(name))
                _httpClientFactoriesLock.TryAdd(name, new object());

            lock (_httpClientFactoriesLock[name])
            {
                if (_httpClientFactories.ContainsKey(name))
                    return _httpClientFactories[name].CreateClient(name);

                _httpClientFactories.TryAdd(name, CreateHttpClientFactory(name, handler));
                return _httpClientFactories[name].CreateClient(name);
            }
        }

        public static HttpClient Create(string name, Action<HttpClientHandler> configure)
        {
            if (_httpClientFactories.ContainsKey(name))
                return _httpClientFactories[name].CreateClient(name);

            if (!_httpClientFactoriesLock.ContainsKey(name))
                _httpClientFactoriesLock.TryAdd(name, new object());

            lock (_httpClientFactoriesLock[name])
            {
                if (_httpClientFactories.ContainsKey(name))
                    return _httpClientFactories[name].CreateClient(name);

                _httpClientFactories.TryAdd(name, CreateHttpClientFactory(name, configure));
                return _httpClientFactories[name].CreateClient(name);
            }
        }
    }
}
