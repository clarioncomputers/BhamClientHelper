using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bham.BizTalk.Rest
{
    public static class PatchClient
    {
        // Cache HttpClient instances by certificate + store + timeout so sockets are reused safely.
        private static readonly ConcurrentDictionary<string, Lazy<HttpClient>> ClientCache =
            new ConcurrentDictionary<string, Lazy<HttpClient>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sends a GET request with client certificate and API key header.
        /// Returns response body as string; throws on non-success HTTP codes.
        /// </summary>
        public static string GetJsonWithClientCertAndApiKey(
            string url,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100)
        {
            return GetWithClientCertAndApiKey(
                url,
                apiKeyHeaderName,
                apiKeyHeaderValue,
                certThumbprint,
                "application/json",
                storeLocation,
                storeName,
                timeoutSeconds);
        }

        /// <summary>
        /// Sends a PATCH request with JSON body, client certificate, and API key header.
        /// Returns response body as string; throws on non-success HTTP codes.
        /// </summary>
        public static string PatchJsonWithClientCertAndApiKey(
            string url,
            string jsonBody,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100)
        {
            return PatchWithClientCertAndApiKey(
                url,
                jsonBody,
                apiKeyHeaderName,
                apiKeyHeaderValue,
                certThumbprint,
                "application/json",
                "application/json",
                storeLocation,
                storeName,
                timeoutSeconds);
        }

        /// <summary>
        /// Sends a GET request with client certificate and API key header.
        /// Returns response body as string; throws on non-success HTTP codes.
        /// </summary>
        public static string GetWithClientCertAndApiKey(
            string url,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint,
            string acceptMediaType,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(apiKeyHeaderName)) throw new ArgumentNullException(nameof(apiKeyHeaderName));
            if (string.IsNullOrWhiteSpace(apiKeyHeaderValue)) throw new ArgumentNullException(nameof(apiKeyHeaderValue));
            if (string.IsNullOrWhiteSpace(certThumbprint)) throw new ArgumentNullException(nameof(certThumbprint));
            if (string.IsNullOrWhiteSpace(acceptMediaType)) throw new ArgumentNullException(nameof(acceptMediaType));

            var client = GetOrCreateClient(certThumbprint, storeLocation, storeName, timeoutSeconds);

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptMediaType));
                request.Headers.Add(apiKeyHeaderName, apiKeyHeaderValue);

                using (var response = client.SendAsync(request).GetAwaiter().GetResult())
                {
                    var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            string.Format(
                                "GET failed: {0} ({1}). Response: {2}",
                                (int)response.StatusCode,
                                response.StatusCode,
                                responseText));
                    }

                    return responseText;
                }
            }
        }

        /// <summary>
        /// Sends a PATCH request with configurable body and accept media types,
        /// client certificate, and API key header.
        /// Returns response body as string; throws on non-success HTTP codes.
        /// </summary>
        public static string PatchWithClientCertAndApiKey(
            string url,
            string body,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint,
            string contentMediaType,
            string acceptMediaType,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(apiKeyHeaderName)) throw new ArgumentNullException(nameof(apiKeyHeaderName));
            if (string.IsNullOrWhiteSpace(apiKeyHeaderValue)) throw new ArgumentNullException(nameof(apiKeyHeaderValue));
            if (string.IsNullOrWhiteSpace(certThumbprint)) throw new ArgumentNullException(nameof(certThumbprint));
            if (string.IsNullOrWhiteSpace(contentMediaType)) throw new ArgumentNullException(nameof(contentMediaType));
            if (string.IsNullOrWhiteSpace(acceptMediaType)) throw new ArgumentNullException(nameof(acceptMediaType));

            var client = GetOrCreateClient(certThumbprint, storeLocation, storeName, timeoutSeconds);

            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), url))
            {
                request.Content = new StringContent(body ?? string.Empty, Encoding.UTF8, contentMediaType);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptMediaType));
                request.Headers.Add(apiKeyHeaderName, apiKeyHeaderValue);

                using (var response = client.SendAsync(request).GetAwaiter().GetResult())
                {
                    var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            string.Format(
                                "PATCH failed: {0} ({1}). Response: {2}",
                                (int)response.StatusCode,
                                response.StatusCode,
                                responseText));
                    }

                    return responseText;
                }
            }
        }

        private static HttpClient GetOrCreateClient(
            string certThumbprint,
            StoreLocation storeLocation,
            StoreName storeName,
            int timeoutSeconds)
        {
            var normalizedThumbprint = certThumbprint.Replace(" ", string.Empty).ToUpperInvariant();
            var key = string.Format(
                "{0}|{1}|{2}|{3}",
                normalizedThumbprint,
                storeLocation,
                storeName,
                timeoutSeconds);

            var lazyClient = ClientCache.GetOrAdd(
                key,
                _ => new Lazy<HttpClient>(
                    () => CreateClient(normalizedThumbprint, storeLocation, storeName, timeoutSeconds),
                    true));

            return lazyClient.Value;
        }

        private static HttpClient CreateClient(
            string thumbprint,
            StoreLocation storeLocation,
            StoreName storeName,
            int timeoutSeconds)
        {
            var cert = FindCertificateByThumbprint(thumbprint, storeLocation, storeName);

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler, true);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            return client;
        }

        private static X509Certificate2 FindCertificateByThumbprint(
            string thumbprint,
            StoreLocation storeLocation,
            StoreName storeName)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                var found = store.Certificates
                    .Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (found == null || found.Count == 0)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Certificate not found in {0}\\{1} for thumbprint {2}.",
                            storeLocation,
                            storeName,
                            thumbprint));
                }

                var withPrivateKey = found.Cast<X509Certificate2>().FirstOrDefault(c => c.HasPrivateKey);
                return withPrivateKey ?? found[0];
            }
        }
    }
}
