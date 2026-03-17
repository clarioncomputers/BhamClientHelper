using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Bham.BizTalk.Rest
{
    public sealed class BizTalkRestClientSettings
    {
        public string ApiKeyHeaderName { get; set; }

        public string ApiKeyHeaderValue { get; set; }

        public string CertThumbprint { get; set; }

        public StoreLocation StoreLocation { get; set; } = StoreLocation.LocalMachine;

        public StoreName StoreName { get; set; } = StoreName.My;

        public int TimeoutSeconds { get; set; } = 100;
    }

    /// <summary>
    /// Thin BizTalk-facing wrapper that centralizes URL/query composition and
    /// common JSON/XML GET and PATCH calls.
    /// </summary>
    public sealed class BizTalkRestClient
    {
        private readonly BizTalkRestClientSettings _settings;

        public BizTalkRestClient(BizTalkRestClientSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(_settings.ApiKeyHeaderName)) throw new ArgumentNullException(nameof(settings.ApiKeyHeaderName));
            if (string.IsNullOrWhiteSpace(_settings.ApiKeyHeaderValue)) throw new ArgumentNullException(nameof(settings.ApiKeyHeaderValue));
            if (string.IsNullOrWhiteSpace(_settings.CertThumbprint)) throw new ArgumentNullException(nameof(settings.CertThumbprint));
        }

        public string GetJson(string baseUrl, IDictionary<string, string> queryParameters = null)
        {
            var url = BuildUrl(baseUrl, queryParameters);
            return PatchClient.GetWithClientCertAndApiKey(
                url,
                _settings.ApiKeyHeaderName,
                _settings.ApiKeyHeaderValue,
                _settings.CertThumbprint,
                "application/json",
                _settings.StoreLocation,
                _settings.StoreName,
                _settings.TimeoutSeconds);
        }

        public string GetXml(string baseUrl, IDictionary<string, string> queryParameters = null)
        {
            var url = BuildUrl(baseUrl, queryParameters);
            return PatchClient.GetWithClientCertAndApiKey(
                url,
                _settings.ApiKeyHeaderName,
                _settings.ApiKeyHeaderValue,
                _settings.CertThumbprint,
                "application/xml",
                _settings.StoreLocation,
                _settings.StoreName,
                _settings.TimeoutSeconds);
        }

        public string PatchJson(string url, string jsonBody)
        {
            return PatchClient.PatchWithClientCertAndApiKey(
                url,
                jsonBody,
                _settings.ApiKeyHeaderName,
                _settings.ApiKeyHeaderValue,
                _settings.CertThumbprint,
                "application/json",
                "application/json",
                _settings.StoreLocation,
                _settings.StoreName,
                _settings.TimeoutSeconds);
        }

        public string PatchXml(string url, string xmlBody)
        {
            return PatchClient.PatchWithClientCertAndApiKey(
                url,
                xmlBody,
                _settings.ApiKeyHeaderName,
                _settings.ApiKeyHeaderValue,
                _settings.CertThumbprint,
                "application/xml",
                "application/xml",
                _settings.StoreLocation,
                _settings.StoreName,
                _settings.TimeoutSeconds);
        }

        public static string BuildUrl(string baseUrl, IDictionary<string, string> queryParameters)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));
            if (queryParameters == null || queryParameters.Count == 0) return baseUrl;

            var query = string.Join(
                "&",
                queryParameters
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                    .Select(kvp =>
                        Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value ?? string.Empty)));

            if (string.IsNullOrWhiteSpace(query)) return baseUrl;
            return baseUrl.Contains("?") ? baseUrl + "&" + query : baseUrl + "?" + query;
        }
    }
}
