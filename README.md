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

Postman alignment:
- The wrapper/facade now includes alias methods whose names read closer to the imported Postman requests.
- Existing methods are still available.
- Use whichever naming is clearer for your team.

### Minimal orchestration setup

Terminology used in this README:
- `pdfFieldId` = Gallagher Personal Data Field numeric id, for example `629`
- `pdfFieldKey` = Gallagher query key built from the field id, for example `pdf_629`
- `cardholderId` = the value stored in that field and used for lookup, for example `IDCARD.12345`
- `gallagherCardholderId` = Gallagher's own cardholder record id, for example `653`

Define orchestration string variables (example):
- strGallagherBaseUrl
- strApiKey
- strCertThumbprint
- strPdfFieldId
- strPdfFieldKey
- strCardholderId
- strResponse
- strGallagherCardholderId
- strAccessGroupId
- strMembershipHref

Expression shape A (initialize once):

    strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
    strApiKey = "YOUR_API_KEY";
    strCertThumbprint = "YOUR_CERT_THUMBPRINT";
    strPdfFieldId = "629";
    strPdfFieldKey = "pdf_" + strPdfFieldId;
    strCardholderId = "IDCARD.12345";

Expression shape B (find cardholder by PDF value via facade):

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetCardholdersByPdfValue(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strCardholderId,
            strPdfFieldKey,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Expression shape C (resolve Gallagher numeric cardholder id from the JSON response):

    strGallagherCardholderId =
        Bham.BizTalk.Rest.GallagherApiResponseParser.GetFirstEntityId(strResponse);

Expression shape D (resolve membership href via facade):

    strMembershipHref =
        Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupMembershipHref(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupId,
            strGallagherCardholderId,
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

Important naming note:
- Methods such as `GetCardholdersByPdfValue(...)` still keep the original method name for backward compatibility.
- The lookup argument now represents the external `cardholderId` value stored in the Personal Data Field, for example `IDCARD.12345`.
- That is different from the Gallagher numeric cardholder id, for example `653`.

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

Postman-aligned aliases:
- ResolvePdfFieldId
- ResolveGallagherCardholderId
- SearchAccessGroupsByName
- RemoveCardholderFromAccessGroup
- UpdateCardholderAccessGroup

### Postman request to method mapping

Imported Postman request names map to these facade methods:

1. `PDF ID`
- `GallagherApiFacade.ResolvePdfFieldId(...)`
- or `GallagherApiFacade.ResolvePersonalDataFieldId(...)`

2. `Cardholder ID`
- `GallagherApiFacade.ResolveGallagherCardholderId(...)`
- or `GallagherApiFacade.ResolveCardholderIdByPdfValue(...)`

3. `Access Group ID`
- `GallagherApiFacade.GetAccessGroups(...)`

4. `Access Group ID Search`
- `GallagherApiFacade.SearchAccessGroupsByName(...)`
- or `GallagherApiFacade.FindAccessGroupsByName(...)`

5. `Access Group Cardholders`
- `GallagherApiFacade.GetAccessGroupCardholders(...)`

6. `Add Access Group to Cardholder`
- `GallagherApiFacade.AddAccessGroupToCardholder(...)`

7. `Remove Cardholder from Access Group`
- `GallagherApiFacade.RemoveCardholderFromAccessGroup(...)`
- or `GallagherApiFacade.RemoveAccessGroupFromCardholder(...)`

8. `Update Cardholder Access Group`
- `GallagherApiFacade.UpdateCardholderAccessGroup(...)`
- or `GallagherApiFacade.UpdateAccessGroupForCardholder(...)`

### End-to-end BizTalk examples for each common Gallagher step

These examples assume the following orchestration string variables already exist:
- strGallagherBaseUrl
- strApiKey
- strCertThumbprint
- strPdfFieldId
- strPdfFieldKey
- strCardholderId
- strGallagherCardholderId
- strAccessGroupName
- strAccessGroupId
- strMembershipHref
- strResponse
- strFromDate
- strUntilDate
- strFromUtc
- strUntilUtc

Example setup:

    strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
    strApiKey = "YOUR_API_KEY";
    strCertThumbprint = "YOUR_CERT_THUMBPRINT";
    strPdfFieldId = "629";
    strPdfFieldKey = "pdf_" + strPdfFieldId;
    strCardholderId = "IDCARD.12345";
    strAccessGroupName = "6040-CHAMBERLAIN-B-11703";
    strFromDate = "2026-04-01";
    strUntilDate = "2026-05-01";
    strFromUtc = "2026-04-01T00:00:00Z";
    strUntilUtc = "2026-04-30T12:00:00Z";

#### 1. Get PDF field id by field name

If you do not already know the Personal Data Field id for the environment, resolve it first.

    strPdfFieldId =
        Bham.BizTalk.Rest.GallagherApiFacade.ResolvePersonalDataFieldId(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            "ThirdPartyID",
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

    strPdfFieldKey = "pdf_" + strPdfFieldId;

#### 2. Get cardholder JSON by external cardholder id

This performs the query:
- GET /api/cardholders?pdf_629="IDCARD.12345"

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetCardholdersByPdfValue(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strCardholderId,
            strPdfFieldKey,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

#### 3. Resolve Gallagher numeric cardholder id from the JSON

    strGallagherCardholderId =
        Bham.BizTalk.Rest.GallagherApiResponseParser.GetFirstEntityId(strResponse);

#### 4. Resolve access group id by name

    strAccessGroupId =
        Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupIdByName(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupName,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

#### 5. Add an access group to a cardholder

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.AddAccessGroupToCardholder(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strGallagherCardholderId,
            strAccessGroupId,
            strFromDate,
            strUntilDate,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

#### 6. Get membership href for an existing access-group assignment

You need the membership href before update or remove.

    strMembershipHref =
        Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupMembershipHref(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupId,
            strGallagherCardholderId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

#### 7. Update an existing access-group assignment

Use UTC timestamps for update operations.

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.UpdateAccessGroupForCardholder(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strGallagherCardholderId,
            strMembershipHref,
            strFromUtc,
            strUntilUtc,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

#### 8. Remove an access-group assignment

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.RemoveAccessGroupFromCardholder(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strGallagherCardholderId,
            strMembershipHref,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

### Compact production flows

#### Add flow

    strPdfFieldId = Bham.BizTalk.Rest.GallagherApiFacade.ResolvePdfFieldId(strGallagherBaseUrl, "Authorization", strApiKey, "ThirdPartyID", strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strPdfFieldKey = "pdf_" + strPdfFieldId;
    strGallagherCardholderId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveGallagherCardholderId(strGallagherBaseUrl, "Authorization", strApiKey, strCardholderId, strPdfFieldKey, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strAccessGroupId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupIdByName(strGallagherBaseUrl, "Authorization", strApiKey, strAccessGroupName, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strResponse = Bham.BizTalk.Rest.GallagherApiFacade.AddAccessGroupToCardholder(strGallagherBaseUrl, "Authorization", strApiKey, strGallagherCardholderId, strAccessGroupId, strFromDate, strUntilDate, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);

#### Update flow

    strPdfFieldId = Bham.BizTalk.Rest.GallagherApiFacade.ResolvePdfFieldId(strGallagherBaseUrl, "Authorization", strApiKey, "ThirdPartyID", strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strPdfFieldKey = "pdf_" + strPdfFieldId;
    strGallagherCardholderId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveGallagherCardholderId(strGallagherBaseUrl, "Authorization", strApiKey, strCardholderId, strPdfFieldKey, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strAccessGroupId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupIdByName(strGallagherBaseUrl, "Authorization", strApiKey, strAccessGroupName, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strMembershipHref = Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupMembershipHref(strGallagherBaseUrl, "Authorization", strApiKey, strAccessGroupId, strGallagherCardholderId, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strResponse = Bham.BizTalk.Rest.GallagherApiFacade.UpdateCardholderAccessGroup(strGallagherBaseUrl, "Authorization", strApiKey, strGallagherCardholderId, strMembershipHref, strFromUtc, strUntilUtc, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);

#### Remove flow

    strPdfFieldId = Bham.BizTalk.Rest.GallagherApiFacade.ResolvePdfFieldId(strGallagherBaseUrl, "Authorization", strApiKey, "ThirdPartyID", strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strPdfFieldKey = "pdf_" + strPdfFieldId;
    strGallagherCardholderId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveGallagherCardholderId(strGallagherBaseUrl, "Authorization", strApiKey, strCardholderId, strPdfFieldKey, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strAccessGroupId = Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupIdByName(strGallagherBaseUrl, "Authorization", strApiKey, strAccessGroupName, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strMembershipHref = Bham.BizTalk.Rest.GallagherApiFacade.ResolveAccessGroupMembershipHref(strGallagherBaseUrl, "Authorization", strApiKey, strAccessGroupId, strGallagherCardholderId, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);
    strResponse = Bham.BizTalk.Rest.GallagherApiFacade.RemoveCardholderFromAccessGroup(strGallagherBaseUrl, "Authorization", strApiKey, strGallagherCardholderId, strMembershipHref, strCertThumbprint, System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, 100);

#### 9. Optional raw reads for troubleshooting

Get all personal data fields:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetPersonalDataFields(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Get one personal data field by id:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetPersonalDataFieldById(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strPdfFieldId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Get all access groups:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetAccessGroups(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Get one access group by id:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetAccessGroupById(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Get access-group cardholders:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetAccessGroupCardholders(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strAccessGroupId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

Get cardholder access groups:

    strResponse =
        Bham.BizTalk.Rest.GallagherApiFacade.GetCardholderAccessGroups(
            strGallagherBaseUrl,
            "Authorization",
            strApiKey,
            strGallagherCardholderId,
            strCertThumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
            System.Security.Cryptography.X509Certificates.StoreName.My,
            100);

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

Gallagher workflow sample using a known Gallagher numeric cardholder id directly:

    .\scripts\SmokeTest.ps1 -Mode gallagherworkflow -ConfigPath .\samples\gallagher-workflow-direct-gallagher-cardholder-id.sample.json

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
- samples/gallagher-workflow.sample.json
- samples/gallagher-workflow-direct-gallagher-cardholder-id.sample.json
- Gallagher API.postman_collection.json
