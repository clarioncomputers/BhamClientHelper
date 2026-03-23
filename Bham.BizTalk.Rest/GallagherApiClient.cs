using System;
using System.Collections.Generic;
using System.Text;

namespace Bham.BizTalk.Rest
{
    /// <summary>
    /// Gallagher-specific wrapper over the generic BizTalk REST helper.
    /// Keeps endpoint paths, quoted filter values, and PATCH body shapes in one place.
    /// </summary>
    public sealed class GallagherApiClient
    {
        private readonly string _baseUrl;
        private readonly BizTalkRestClient _client;

        public GallagherApiClient(string baseUrl, BizTalkRestClientSettings settings)
            : this(baseUrl, CreateClient(settings))
        {
        }

        public GallagherApiClient(string baseUrl, BizTalkRestClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            ValidateAbsoluteHttpUrl(baseUrl, nameof(baseUrl));

            _baseUrl = baseUrl.TrimEnd('/');
            _client = client;
        }

        public string GetPersonalDataFieldsByName(string fieldName)
        {
            return _client.GetJson(
                CombineUrl("personal_data_fields"),
                new Dictionary<string, string>
                {
                    { "name", BuildQuotedQueryValue(fieldName) }
                });
        }

        public string GetPersonalDataFields()
        {
            return _client.GetJson(CombineUrl("personal_data_fields"));
        }

        public string GetPersonalDataFieldById(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw new ArgumentNullException(nameof(fieldId));
            return _client.GetJson(CombineUrl("personal_data_fields/" + EncodePathSegment(fieldId)));
        }

        public string GetCardholdersByPdfValue(string pdfValue, string pdfFieldKey = "pdf_629")
        {
            if (string.IsNullOrWhiteSpace(pdfFieldKey)) throw new ArgumentNullException(nameof(pdfFieldKey));

            return _client.GetJson(
                CombineUrl("cardholders"),
                new Dictionary<string, string>
                {
                    { pdfFieldKey, BuildQuotedQueryValue(pdfValue) }
                });
        }

        public string GetCardholders()
        {
            return _client.GetJson(CombineUrl("cardholders"));
        }

        public string GetCardholderById(string cardholderId)
        {
            if (string.IsNullOrWhiteSpace(cardholderId)) throw new ArgumentNullException(nameof(cardholderId));
            return _client.GetJson(CombineUrl("cardholders/" + EncodePathSegment(cardholderId)));
        }

        public string GetAccessGroups()
        {
            return _client.GetJson(CombineUrl("access_groups"));
        }

        public string GetAccessGroupById(string accessGroupId)
        {
            if (string.IsNullOrWhiteSpace(accessGroupId)) throw new ArgumentNullException(nameof(accessGroupId));
            return _client.GetJson(CombineUrl("access_groups/" + EncodePathSegment(accessGroupId)));
        }

        public string FindAccessGroupsByName(string accessGroupName)
        {
            return _client.GetJson(
                CombineUrl("access_groups"),
                new Dictionary<string, string>
                {
                    { "name", BuildQuotedQueryValue(accessGroupName) }
                });
        }

        public string GetAccessGroupCardholders(string accessGroupId)
        {
            if (string.IsNullOrWhiteSpace(accessGroupId)) throw new ArgumentNullException(nameof(accessGroupId));
            return _client.GetJson(CombineUrl("access_groups/" + EncodePathSegment(accessGroupId) + "/cardholders"));
        }

        public string GetCardholderAccessGroups(string cardholderId)
        {
            if (string.IsNullOrWhiteSpace(cardholderId)) throw new ArgumentNullException(nameof(cardholderId));
            return _client.GetJson(CombineUrl("cardholders/" + EncodePathSegment(cardholderId) + "/access_groups"));
        }

        public string ResolvePersonalDataFieldId(string fieldName)
        {
            return GallagherApiResponseParser.GetEntityIdByName(GetPersonalDataFieldsByName(fieldName), fieldName);
        }

        public string ResolveCardholderIdByPdfValue(string pdfValue, string pdfFieldKey = "pdf_629")
        {
            return GallagherApiResponseParser.GetFirstEntityId(GetCardholdersByPdfValue(pdfValue, pdfFieldKey));
        }

        public string ResolveAccessGroupIdByName(string accessGroupName)
        {
            return GallagherApiResponseParser.GetEntityIdByName(FindAccessGroupsByName(accessGroupName), accessGroupName);
        }

        public string ResolveAccessGroupMembershipHref(string accessGroupId, string cardholderId)
        {
            return GallagherApiResponseParser.GetAccessGroupMembershipHrefForCardholder(GetAccessGroupCardholders(accessGroupId), cardholderId);
        }

        public string AddAccessGroupToCardholder(string cardholderId, string accessGroupId, string fromDate, string untilDate)
        {
            var url = CombineUrl("cardholders/" + EncodePathSegment(cardholderId));
            var body = BuildAddAccessGroupPatchBody(CombineUrl("access_groups/" + EncodePathSegment(accessGroupId)), fromDate, untilDate);
            return _client.PatchJson(url, body);
        }

        public string RemoveAccessGroupFromCardholder(string cardholderId, string membershipHref)
        {
            var url = CombineUrl("cardholders/" + EncodePathSegment(cardholderId));
            var body = BuildRemoveAccessGroupPatchBody(membershipHref);
            return _client.PatchJson(url, body);
        }

        public string UpdateAccessGroupForCardholder(string cardholderId, string membershipHref, string fromUtc, string untilUtc)
        {
            var url = CombineUrl("cardholders/" + EncodePathSegment(cardholderId));
            var body = BuildUpdateAccessGroupPatchBody(membershipHref, fromUtc, untilUtc);
            return _client.PatchJson(url, body);
        }

        public static string BuildQuotedQueryValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            return "\"" + value.Trim() + "\"";
        }

        public static string BuildAddAccessGroupPatchBody(string accessGroupHref, string fromDate, string untilDate)
        {
            ValidateAbsoluteHttpUrl(accessGroupHref, nameof(accessGroupHref));
            if (string.IsNullOrWhiteSpace(fromDate)) throw new ArgumentNullException(nameof(fromDate));
            if (string.IsNullOrWhiteSpace(untilDate)) throw new ArgumentNullException(nameof(untilDate));

            return "{"
                + "\"accessGroups\":{"
                + "\"add\":[{"
                + "\"accessGroup\":{\"href\":\"" + EscapeJson(accessGroupHref) + "\"},"
                + "\"from\":\"" + EscapeJson(fromDate.Trim()) + "\","
                + "\"until\":\"" + EscapeJson(untilDate.Trim()) + "\""
                + "}]}"
                + "}";
        }

        public static string BuildRemoveAccessGroupPatchBody(string membershipHref)
        {
            ValidateAbsoluteHttpUrl(membershipHref, nameof(membershipHref));

            return "{"
                + "\"accessGroups\":{"
                + "\"remove\":[{"
                + "\"href\":\"" + EscapeJson(membershipHref) + "\""
                + "}]}"
                + "}";
        }

        public static string BuildUpdateAccessGroupPatchBody(string membershipHref, string fromUtc, string untilUtc)
        {
            ValidateAbsoluteHttpUrl(membershipHref, nameof(membershipHref));
            if (string.IsNullOrWhiteSpace(fromUtc)) throw new ArgumentNullException(nameof(fromUtc));
            if (string.IsNullOrWhiteSpace(untilUtc)) throw new ArgumentNullException(nameof(untilUtc));

            return "{"
                + "\"accessGroups\":{"
                + "\"update\":[{"
                + "\"href\":\"" + EscapeJson(membershipHref) + "\","
                + "\"from\":\"" + EscapeJson(fromUtc.Trim()) + "\","
                + "\"until\":\"" + EscapeJson(untilUtc.Trim()) + "\""
                + "}]}"
                + "}";
        }

        private string CombineUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            return _baseUrl + "/" + relativePath.TrimStart('/');
        }

        private static BizTalkRestClient CreateClient(BizTalkRestClientSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return new BizTalkRestClient(settings);
        }

        private static string EncodePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            return Uri.EscapeDataString(value.Trim());
        }

        private static void ValidateAbsoluteHttpUrl(string url, string parameterName)
        {
            Uri parsed;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsed) ||
                (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("URL must be an absolute http or https URI.", parameterName);
            }
        }

        private static string EscapeJson(string value)
        {
            var builder = new StringBuilder(value.Length + 8);

            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                switch (current)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (current < 32)
                        {
                            builder.Append("\\u");
                            builder.Append(((int)current).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(current);
                        }

                        break;
                }
            }

            return builder.ToString();
        }
    }
}