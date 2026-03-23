# Bham.HelperClient

BizTalk 2016 helper library for outbound REST GET and PATCH calls, with Gallagher-specific wrappers.

## 1. What this library gives a BizTalk developer

- Static HTTP helper methods for GET and PATCH.
- Optional client certificate (thumbprint lookup in Windows certificate store).
- API key header support.
- Typed Gallagher wrapper logic for cardholders and access groups.
- BizTalk-safe static Gallagher facade so you can use wrapper logic without holding non-serializable objects in orchestration state.
- JSON parsing helpers for common Gallagher response extraction.

Target framework:
- .NET Framework 4.6.1 (net461)

Main output:
- Bham.BizTalk.Rest.dll

## 2. Quick start for BizTalk (recommended path)

Use the static facade class in orchestration Expression shapes:
- Bham.BizTalk.Rest.GallagherApiFacade

This avoids the BizTalk serialization problem you get if you declare GallagherApiClient as an orchestration variable.

### Minimal orchestration setup

Define orchestration string variables (example):
- strGallagherBaseUrl
- strApiKey
- strCertThumbprint
- strPdfValue
- strPdfFieldKey
- strResponse
- strCardholderId
- strAccessGroupId
- strMembershipHref

Expression shape A (initialize once):

    strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
    strApiKey = "YOUR_API_KEY";
    strCertThumbprint = "YOUR_CERT_THUMBPRINT";
    strPdfFieldKey = "pdf_629";

Expression shape B (find cardholder by PDF value via facade):

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetCardholdersByPdfValue(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strPdfValue,
            strPdfFieldKey,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Expression shape C (resolve membership href via facade):

    strMembershipHref =
        Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupMembershipHref(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupId,
            strCardholderId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

## 3. Which API surface to use

Use this decision guide:

1. Use GallagherApiFacade when:
- You are in BizTalk orchestration code.
- You want Gallagher-specific methods (cardholders, access groups, membership href resolution).
- You want to avoid non-serializable wrapper instances.

2. Use PatchClient when:
- You need a raw URL call and do not need Gallagher wrapper behavior.
- You want the simplest direct static GET/PATCH calls.

3. Use GallagherApiClient directly only when:
- You are in regular .NET code (or inside BizTalk Atomic scope).
- You are comfortable creating wrapper instances.

## 4. BizTalk serialization rule (important)

BizTalk persists orchestration state. Non-serializable CLR object variables cannot be stored in normal orchestration scope.

This will fail in normal scope:
- Bham.BizTalk.Rest.GallagherApiClient
- Bham.BizTalk.Rest.BizTalkRestClientSettings

Your options:

1. Preferred for orchestrations: use static GallagherApiFacade methods.
2. Alternative: use GallagherApiClient only inside an Atomic Scope.
3. Alternative: wrap logic in external static helper methods and call those.

## 5. Certificate thumbprint behavior

- certThumbprint is optional.
- If provided, the certificate is looked up and attached from StoreLocation + StoreName.
- If omitted/null, request is sent without client certificate.

BizTalk-friendly overloads:
- PatchClient.GetJsonWithClientCertAndApiKeyDefaultStore
- PatchClient.GetXmlWithClientCertAndApiKeyDefaultStore
- PatchClient.PatchJsonWithClientCertAndApiKeyDefaultStore
- PatchClient.PatchXmlWithClientCertAndApiKeyDefaultStore

No-certificate overloads:
- PatchClient.GetJsonWithApiKey
- PatchClient.GetXmlWithApiKey
- PatchClient.PatchJsonWithApiKey
- PatchClient.PatchXmlWithApiKey

## 6. Gallagher facade methods

Common methods on GallagherApiFacade:

Read/query:
- GetPersonalDataFields
- GetPersonalDataFieldsByName
- GetPersonalDataFieldById
- GetCardholders
- GetCardholdersByPdfValue
- GetCardholderById
- GetAccessGroups
- GetAccessGroupById
- FindAccessGroupsByName
- GetAccessGroupCardholders
- GetCardholderAccessGroups

Resolve IDs/hrefs:
- ResolvePersonalDataFieldId
- ResolveCardholderIdByPdfValue
- ResolveAccessGroupIdByName
- ResolveAccessGroupMembershipHref

Update membership:
- AddAccessGroupToCardholder
- RemoveAccessGroupFromCardholder
- UpdateAccessGroupForCardholder

## 7. JSON parser helpers (for multi-record responses)

Use GallagherApiResponseParser when you already have response JSON and need extraction logic.

Example: find membership href by cardholder name and optional dates:

    string strMembershipHref = "";

    if (Bham.BizTalk.Rest.GallagherApiResponseParser.TryGetAccessGroupMembershipHrefByNameAndDates(
        strMembershipJson,
        strCardholderName,
        strFromDate,
        strUntilDate,
        out strMembershipHref))
    {
        // Match found
    }
    else
    {
        // No match found
    }

Available parser methods:
- GetFirstEntityId(json)
- GetEntityIdByName(json, name)
- GetAccessGroupMembershipHrefForCardholder(json, cardholderId)
- GetAccessGroupMembershipHrefByNameAndDates(json, cardholderName, fromDate, untilDate)
- TryGetAccessGroupMembershipHrefByNameAndDates(json, cardholderName, fromDate, untilDate, out href)

## 8. Error handling and logging

- Library throws BizTalkRestClientException for HTTP failures.
- Exception includes HTTP method, URL, status code (if available), and response body (if available).
- Optional logger hook: BizTalkRestClientSettings.Logger (Action<BizTalkRestLogEntry>).

## 9. Build and test

Build solution:

    dotnet build .\Bham.HelperClient.sln -c Release

Build + test script:

    .\scripts\build-and-test.ps1 -Configuration Release

Run tests directly:

    [Reflection.Assembly]::LoadFrom("$PWD\Bham.BizTalk.Rest.Tests\bin\Release\Bham.BizTalk.Rest.Tests.dll") | Out-Null
    [Bham.BizTalk.Rest.Tests.TestRunner]::RunAll()

## 10. Smoke test commands

Gallagher workflow sample:

    .\scripts\SmokeTest.ps1 -Mode gallagherworkflow -ConfigPath .\samples\gallagher-workflow.sample.json

Failure scenarios:

    .\scripts\SmokeTest.ps1 -Mode scenariomissingcert
    .\scripts\SmokeTest.ps1 -Mode scenariotimeoutpublic -DelayMilliseconds 5000 -TimeoutSeconds 1
    .\scripts\SmokeTest.ps1 -Mode scenariononsuccesshttp -StatusCode 503 -TimeoutSeconds 30

## 11. Deployment notes for BizTalk

- Deploy Bham.BizTalk.Rest.dll where BizTalk host can load it (or GAC, per your standards).
- Ensure certificate is in expected store and BizTalk host account has private key access.
- Keep AssemblyVersion stable for non-breaking updates to minimize binding churn.
- Increment AssemblyFileVersion each release.

Current version metadata is declared in:
- Bham.BizTalk.Rest/Properties/AssemblyInfo.cs

## 12. Strong name and GAC (optional)

Create key:

    sn -k .\Bham.BizTalk.Rest\Bham.BizTalk.Rest.snk

Build:

    dotnet build .\Bham.HelperClient.sln -c Release

Verify strong name:

    sn -vf .\Bham.BizTalk.Rest\bin\Release\Bham.BizTalk.Rest.dll

## 13. Related sample files

- samples/ExpressionShapeSnippets.cs
- samples/GallagherApiSamples.cs
- Gallagher API.postman_collection.json
