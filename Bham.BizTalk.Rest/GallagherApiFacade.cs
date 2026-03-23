using System;
using System.Security.Cryptography.X509Certificates;

namespace Bham.BizTalk.Rest
{
    /// <summary>
    /// BizTalk-safe static facade over GallagherApiClient.
    /// Callers pass primitive arguments and do not hold wrapper instances in orchestration state.
    /// </summary>
    public static class GallagherApiFacade
    {
        public static string GetPersonalDataFields(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetPersonalDataFields();
        }

        public static string GetPersonalDataFieldsByName(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string fieldName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetPersonalDataFieldsByName(fieldName);
        }

        public static string GetPersonalDataFieldById(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string fieldId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetPersonalDataFieldById(fieldId);
        }

        public static string GetCardholders(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetCardholders();
        }

        public static string GetCardholdersByPdfValue(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string pdfFieldKey = "pdf_629",
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetCardholdersByPdfValue(cardholderId, pdfFieldKey);
        }

        public static string GetCardholderById(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetCardholderById(cardholderId);
        }

        public static string GetAccessGroups(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetAccessGroups();
        }

        public static string GetAccessGroupById(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetAccessGroupById(accessGroupId);
        }

        public static string FindAccessGroupsByName(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .FindAccessGroupsByName(accessGroupName);
        }

        public static string GetAccessGroupCardholders(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetAccessGroupCardholders(accessGroupId);
        }

        public static string GetCardholderAccessGroups(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .GetCardholderAccessGroups(cardholderId);
        }

        public static string ResolvePersonalDataFieldId(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string fieldName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolvePersonalDataFieldId(fieldName);
        }

        public static string ResolvePdfFieldId(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string fieldName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolvePdfFieldId(fieldName);
        }

        public static string ResolveCardholderIdByPdfValue(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string pdfFieldKey = "pdf_629",
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolveCardholderIdByPdfValue(cardholderId, pdfFieldKey);
        }

        public static string ResolveGallagherCardholderId(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string pdfFieldKey = "pdf_629",
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolveGallagherCardholderId(cardholderId, pdfFieldKey);
        }

        public static string ResolveAccessGroupIdByName(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolveAccessGroupIdByName(accessGroupName);
        }

        public static string SearchAccessGroupsByName(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupName,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .SearchAccessGroupsByName(accessGroupName);
        }

        public static string ResolveAccessGroupMembershipHref(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string accessGroupId,
            string cardholderId,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .ResolveAccessGroupMembershipHref(accessGroupId, cardholderId);
        }

        public static string AddAccessGroupToCardholder(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string accessGroupId,
            string fromDate,
            string untilDate,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .AddAccessGroupToCardholder(cardholderId, accessGroupId, fromDate, untilDate);
        }

        public static string RemoveAccessGroupFromCardholder(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string membershipHref,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .RemoveAccessGroupFromCardholder(cardholderId, membershipHref);
        }

        public static string RemoveCardholderFromAccessGroup(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string membershipHref,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .RemoveCardholderFromAccessGroup(cardholderId, membershipHref);
        }

        public static string UpdateAccessGroupForCardholder(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string membershipHref,
            string fromUtc,
            string untilUtc,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .UpdateAccessGroupForCardholder(cardholderId, membershipHref, fromUtc, untilUtc);
        }

        public static string UpdateCardholderAccessGroup(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string cardholderId,
            string membershipHref,
            string fromUtc,
            string untilUtc,
            string certThumbprint = null,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            StoreName storeName = StoreName.My,
            int timeoutSeconds = 100,
            Action<BizTalkRestLogEntry> logger = null)
        {
            return CreateClient(baseUrl, apiKeyHeaderName, apiKeyHeaderValue, certThumbprint, storeLocation, storeName, timeoutSeconds, logger)
                .UpdateCardholderAccessGroup(cardholderId, membershipHref, fromUtc, untilUtc);
        }

        private static GallagherApiClient CreateClient(
            string baseUrl,
            string apiKeyHeaderName,
            string apiKeyHeaderValue,
            string certThumbprint,
            StoreLocation storeLocation,
            StoreName storeName,
            int timeoutSeconds,
            Action<BizTalkRestLogEntry> logger)
        {
            return new GallagherApiClient(
                baseUrl,
                new BizTalkRestClientSettings
                {
                    ApiKeyHeaderName = apiKeyHeaderName,
                    ApiKeyHeaderValue = apiKeyHeaderValue,
                    CertThumbprint = certThumbprint,
                    StoreLocation = storeLocation,
                    StoreName = storeName,
                    TimeoutSeconds = timeoutSeconds,
                    Logger = logger
                });
        }
    }
}