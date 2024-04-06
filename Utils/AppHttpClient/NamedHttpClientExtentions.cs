using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("AspNetServer")]
namespace Utils
{
    public static class NamedHttpClientExtentions
    {
        public static HttpClientHandler AddCertificate(this HttpClientHandler handler, X509Certificate2 cert)
        {
            if (cert == null)
                throw new ArgumentException($"{nameof(cert)} is null");

            if (handler == null)
                throw new ArgumentException($"{nameof(handler)} is null");

            handler.ClientCertificates.Add(cert);

            return handler;
        }

        public static HttpClientHandler AddNetworkCredentials(this HttpClientHandler handler, string login, string password)
        {
            if (handler == null)
                throw new ArgumentException($"{nameof(handler)} is null");

            handler.Credentials = new NetworkCredential(login, password);

            return handler;
        }

        public static HttpClientHandler AllowSelfSignedCertificate(this HttpClientHandler handler, bool onlyDebug = true)
        {
            if (handler == null)
                throw new ArgumentException($"{nameof(handler)} is null");

            if (onlyDebug)
                AllowSelfSignedCertificateDebug(handler);
            else
                AllowSelfSignedCertificateAlways(handler);

            return handler;
        }

        [Conditional("DEBUG")]
        public static void AllowSelfSignedCertificateDebug(HttpClientHandler handler)
        {
            AllowSelfSignedCertificateAlways(handler);
        }

        public static void AllowSelfSignedCertificateAlways(HttpClientHandler handler)
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
        }

        public static HttpClientHandler SetSslProtocols(this HttpClientHandler handler, System.Security.Authentication.SslProtocols protocols)
        {
            if (handler == null)
                throw new ArgumentException($"{nameof(handler)} is null");

            handler.SslProtocols = protocols;
            return handler;
        }
    }
}
