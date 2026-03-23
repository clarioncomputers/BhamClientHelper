# Bham.HelperClient

BizTalk 2016 helper library that adds outbound REST support for HTTP GET and HTTP PATCH with:
- Client certificate from Windows certificate store (thumbprint lookup)
- API key header injection
- Static HttpClient reuse for connection pooling
- Optional request logging via `BizTalkRestClientSettings.Logger`
- Typed request failures via `BizTalkRestClientException`

## Target framework
- .NET Framework 4.6.1 (`net461`)

## Project output
- `Bham.BizTalk.Rest.dll`

## Assembly versioning (BizTalk deployments)
- The project now declares explicit version metadata in [Bham.BizTalk.Rest/Properties/AssemblyInfo.cs](Bham.BizTalk.Rest/Properties/AssemblyInfo.cs).
- Current values:
	- `AssemblyVersion = 1.0.0.0`
	- `AssemblyFileVersion = 1.0.0.0`
	- `AssemblyInformationalVersion = 1.0.0`
- Recommended release practice for BizTalk:
	- Keep `AssemblyVersion` stable for non-breaking updates to reduce binding/GAC churn.
	- Increment `AssemblyFileVersion` (and informational version) for every release.
	- Increment `AssemblyVersion` only for deliberate breaking or side-by-side version changes.

## Build
```powershell
dotnet build .\Bham.HelperClient.sln -c Release
```

One-command local restore, build, and tests:
```powershell
.\scripts\build-and-test.ps1 -Configuration Release
```

## Unit tests
- Test project: `Bham.BizTalk.Rest.Tests` (`net461`, lightweight in-project assertions).
- Current coverage includes:
	- `BizTalkRestClient.BuildUrl(...)` query composition/encoding behavior.
	- URL scheme guard behavior (`http`/`https` only) to reduce SSRF risk.

Run tests locally (after build):
```powershell
[Reflection.Assembly]::LoadFrom("$PWD\Bham.BizTalk.Rest.Tests\bin\Release\Bham.BizTalk.Rest.Tests.dll") | Out-Null
[Bham.BizTalk.Rest.Tests.TestRunner]::RunAll()
```

Build the full solution:
```powershell
dotnet build .\Bham.HelperClient.sln -c Release
```

## Automatic test execution (CI)
- Workflow file: `.github/workflows/ci.yml`.
- Triggers: every `push` and every `pull_request`.
- Steps performed automatically in GitHub Actions:
	1. `nuget restore Bham.HelperClient.sln`
	2. `msbuild Bham.HelperClient.sln /p:Configuration=Release /p:Platform="Any CPU" /m`
	3. Load `Bham.BizTalk.Rest.Tests.dll` and run `[Bham.BizTalk.Rest.Tests.TestRunner]::RunAll()`

This means test failures will fail the CI job and show directly on the commit/PR checks.

## Public helper methods
- `PatchClient.GetJsonWithClientCertAndApiKey(...)`
- `PatchClient.GetXmlWithClientCertAndApiKey(...)`
- `PatchClient.PatchJsonWithClientCertAndApiKey(...)`
- `PatchClient.PatchXmlWithClientCertAndApiKey(...)`
- `PatchClient.GetJsonWithApiKey(...)` (no certificate)
- `PatchClient.GetXmlWithApiKey(...)` (no certificate)
- `PatchClient.PatchJsonWithApiKey(...)` (no certificate)
- `PatchClient.PatchXmlWithApiKey(...)` (no certificate)
- `PatchClient.GetJsonWithClientCertAndApiKeyDefaultStore(...)` (uses `LocalMachine/My`)
- `PatchClient.GetXmlWithClientCertAndApiKeyDefaultStore(...)` (uses `LocalMachine/My`)
- `PatchClient.PatchJsonWithClientCertAndApiKeyDefaultStore(...)` (uses `LocalMachine/My`)
- `PatchClient.PatchXmlWithClientCertAndApiKeyDefaultStore(...)` (uses `LocalMachine/My`)

## BizTalk Expression shape snippets
`samples/ExpressionShapeSnippets.cs` contains ready-to-copy statements covering all method overloads:
- GET JSON / XML, no certificate
- GET JSON / XML, certificate with default store (LocalMachine\My)
- PATCH JSON / XML, no certificate
- PATCH JSON / XML, certificate with default store (LocalMachine\My)

Copy the relevant block directly into a BizTalk Expression shape. Do not add the file to a project — it is reference only.

## Gallagher API examples
- `samples/GallagherApiSamples.cs` maps the requests in `Gallagher API.postman_collection.json` to this helper library.
- `Bham.BizTalk.Rest.GallagherApiClient` is the typed wrapper for those Gallagher endpoints so callers do not need to hand-build URLs and PATCH bodies each time.
- Included GET examples:
	- `GET /api/personal_data_fields?name="ThirdPartyID"`
	- `GET /api/cardholders?pdf_629="IDCARD.12345"`
	- `GET /api/access_groups`
	- `GET /api/access_groups?name="6040-CHAMBERLAIN-B-11703"`
	- `GET /api/access_groups/663/cardholders`
- Included PATCH examples:
	- add access group to cardholder
	- remove cardholder access group membership
	- update cardholder access group date range
- Main typed methods:
	- `GetPersonalDataFields()`
	- `GetPersonalDataFieldsByName(...)`
	- `GetPersonalDataFieldById(...)`
	- `ResolvePersonalDataFieldId(...)`
	- `GetCardholders()`
	- `GetCardholdersByPdfValue(...)`
	- `GetCardholderById(...)`
	- `ResolveCardholderIdByPdfValue(...)`
	- `GetAccessGroups()`
	- `GetAccessGroupById(...)`
	- `FindAccessGroupsByName(...)`
	- `ResolveAccessGroupIdByName(...)`
	- `GetCardholderAccessGroups(...)`
	- `GetAccessGroupCardholders(...)`
	- `ResolveAccessGroupMembershipHref(...)`
	- `AddAccessGroupToCardholder(...)`
	- `RemoveAccessGroupFromCardholder(...)`
	- `UpdateAccessGroupForCardholder(...)`
- Parsing helpers are also available in `GallagherApiResponseParser` for callers that want to work with raw JSON responses and extract ids or membership hrefs directly.
- The Postman collection uses the `Authorization` header for the API key, so set `ApiKeyHeaderName = "Authorization"` or pass `"Authorization"` into the `PatchClient` overloads.
- The quoted query values from Postman, such as `"ThirdPartyID"`, are intentional. If you use `BizTalkRestClient.BuildUrl(...)` or `BizTalkRestClient.GetJson(...)` with query parameters, the helper will URL-encode those quotes for you.
- The smoke-test console now supports Gallagher-specific commands using that wrapper. Example commands:
	- `gallaghergetpdfid https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> ThirdPartyID`
	- `gallaghergetcardholderid https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> IDCARD.12345`
	- `gallaghersearchaccessgroup https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> 6040-CHAMBERLAIN-B-11703`
	- `gallagheraddaccessgroup https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> 653 663 2026-04-01 2026-05-01`
	- `gallagherremoveaccessgroup https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> 653 <membershipHref>`
	- `gallagherupdateaccessgroup https://its-d-cdx-01.adf.bham.ac.uk:8904/api <apiKey> 653 <membershipHref> 2026-04-01T00:00:00Z 2026-04-30T12:00:00Z`
	- `gallagherworkflow --baseUrl https://its-d-cdx-01.adf.bham.ac.uk:8904/api --apiKey <apiKey> --operation add --pdfValue IDCARD.12345 --accessGroupName 6040-CHAMBERLAIN-B-11703 --from 2026-04-01 --until 2026-05-01`
	- `gallagherworkflow .\samples\gallagher-workflow.sample.json`
- `gallagherworkflow` accepts either named arguments or a JSON config file. It can resolve `cardholderId` from `pdfValue`, resolve `accessGroupId` from `accessGroupName`, and resolve `membershipHref` from `accessGroupId` plus `cardholderId` before running add/remove/update.

## Error handling and logging
- GET and PATCH failures now surface as `BizTalkRestClientException`, including the HTTP method, URL, optional status code, and response body when one is available.
- `BizTalkRestClientSettings.Logger` accepts an `Action<BizTalkRestLogEntry>` so callers can forward start/success/failure events into BizTalk tracing, Event Log, or another sink.
- The smoke test now writes library log events to the console automatically.

## Certificate selection behavior
- `certThumbprint` is optional on `GetJsonWithClientCertAndApiKey`, `GetXmlWithClientCertAndApiKey`, `PatchJsonWithClientCertAndApiKey`, and `PatchXmlWithClientCertAndApiKey`.
- When a thumbprint is provided, the helper resolves and attaches that certificate from the configured store (`StoreLocation` + `StoreName`).
- When no thumbprint is provided, the helper sends the request without a client certificate (API-key/header authentication only).
- For BizTalk Expression shapes that cannot pass `null` or `""` cleanly, use the explicit no-certificate overloads: `GetJsonWithApiKey`, `GetXmlWithApiKey`, `PatchJsonWithApiKey`, `PatchXmlWithApiKey`.
- For BizTalk Expression shapes that cannot pass enum values for certificate store/location, use the default-store overloads ending in `DefaultStore`.

## BizTalk compatibility note (.NET Framework 4.6.1)
- The helper attaches client certificates to `HttpClientHandler` using reflection to support BizTalk projects where `ClientCertificates` is not visible at compile time due to `System.Net.Http` API-surface differences.
- Runtime still requires a `System.Net.Http` implementation that supports client certificate attachment.
- If runtime support is missing, the helper throws a clear `InvalidOperationException` describing the reference issue.

## BizTalk 2016 CU9 validation checklist
Use this checklist in a BizTalk 2016 CU9 test environment before production rollout.

1. Runtime and assembly checks
	- Confirm the BizTalk server is on CU9 and the host instance is running under the expected service account.
	- Deploy `Bham.BizTalk.Rest.dll` to the location used by your BizTalk host (or GAC if that is your standard).
	- Verify assembly loading and binding in Event Viewer (no `System.Net.Http` or dependency binding errors).

2. Certificate and permissions checks
	- Import the client certificate into the exact store you configured (`LocalMachine/My` by default).
	- Grant the BizTalk host instance account private key access to that certificate.
	- Validate thumbprint value and ensure no hidden characters/spaces are present.

3. Connectivity and TLS checks
	- Validate outbound DNS and firewall access from the BizTalk server to target endpoints.
	- Confirm TLS/cipher policy on the server supports the endpoint requirements.

4. Smoke and failure validation
	- Run smoke tests from `Bham.BizTalk.Rest.SmokeTest` with representative GET/PATCH requests.
	- Run failure scenarios (`scenariomissingcert`, timeout, non-success HTTP/HTTPS) to verify expected error handling and logging.

5. Orchestration end-to-end validation
	- Execute at least one real orchestration path using this helper.
	- Verify request/response handling, exception behavior, and tracking/log output match expectations.

6. Release readiness gate
	- Keep CI green (workflow checks) and require passing checks on the deployment PR.
	- Promote only after all checklist steps pass in the CU9 environment.

## BizTalk call shape examples
Use an Expression shape or Call Rules shape to invoke the helper and store the response in orchestration string variables.

GET example:
```csharp
strGetResponse =
	Bham.BizTalk.Rest.PatchClient.GetJsonWithClientCertAndApiKey(
		"https://api.example.com/orders?customerId=" + strCustomerId + "&status=Open",
		"x-api-key",
		strApiKey,
		storeLocation: System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
		storeName: System.Security.Cryptography.X509Certificates.StoreName.My,
		timeoutSeconds: 100);
```

GET example (certificate, default store, BizTalk-friendly):
```csharp
strGetResponse =
	Bham.BizTalk.Rest.PatchClient.GetJsonWithClientCertAndApiKeyDefaultStore(
		"https://api.example.com/orders?customerId=" + strCustomerId + "&status=Open",
		"x-api-key",
		strApiKey,
		strCertThumbprint,
		100);
```

GET example (no certificate, BizTalk-friendly):
```csharp
strGetResponse =
	Bham.BizTalk.Rest.PatchClient.GetJsonWithApiKey(
		"https://api.example.com/orders?customerId=" + strCustomerId + "&status=Open",
		"x-api-key",
		strApiKey,
		100);
```

GET XML example:
```csharp
strGetXmlResponse =
	Bham.BizTalk.Rest.PatchClient.GetXmlWithClientCertAndApiKey(
		"https://api.example.com/orders/" + strOrderId,
		"x-api-key",
		strApiKey,
		storeLocation: System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
		storeName: System.Security.Cryptography.X509Certificates.StoreName.My,
		timeoutSeconds: 100);
```

PATCH example:
```csharp
strPatchBody =
	"{"
	+ "\"status\":\"Done\"," 
	+ "\"updatedBy\":\"BizTalk\""
	+ "}";

strPatchResponse =
	Bham.BizTalk.Rest.PatchClient.PatchJsonWithClientCertAndApiKey(
		"https://api.example.com/orders/" + strOrderId,
		strPatchBody,
		"x-api-key",
		strApiKey,
		storeLocation: System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
		storeName: System.Security.Cryptography.X509Certificates.StoreName.My,
		timeoutSeconds: 100);
```

PATCH XML example:
```csharp
strPatchXmlBody =
	"<request>"
	+ "<status>Done</status>"
	+ "<updatedBy>BizTalk</updatedBy>"
	+ "</request>";

strPatchXmlResponse =
	Bham.BizTalk.Rest.PatchClient.PatchXmlWithClientCertAndApiKey(
		"https://api.example.com/orders/" + strOrderId,
		strPatchXmlBody,
		"x-api-key",
		strApiKey,
		storeLocation: System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
		storeName: System.Security.Cryptography.X509Certificates.StoreName.My,
		timeoutSeconds: 100);
```

### BizTalk .odx example (set base URL once)
Use one orchestration variable for the Gallagher base URL, assign it once, then reuse it across Expression shapes.

Expression shape A (initialize variables):
```csharp
// In production, load from SSO/BRE/config instead of hardcoding.
strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
strApiKey = "YOUR_API_KEY";
strPdfValue = "IDCARD.12345";
```

Expression shape B (direct helper call):
```csharp
strResponse =
	Bham.BizTalk.Rest.PatchClient.GetJsonWithApiKey(
		strGallagherBaseUrl + "/cardholders?pdf_629=%22" + strPdfValue + "%22",
		"Authorization",
		strApiKey,
		100);
```

Expression shape C (typed wrapper call):
```csharp
Bham.BizTalk.Rest.BizTalkRestClientSettings settings = null;
Bham.BizTalk.Rest.GallagherApiClient gallagher = null;

settings = new Bham.BizTalk.Rest.BizTalkRestClientSettings();
settings.ApiKeyHeaderName = "Authorization";
settings.ApiKeyHeaderValue = strApiKey;
settings.TimeoutSeconds = 100;

gallagher = new Bham.BizTalk.Rest.GallagherApiClient(strGallagherBaseUrl, settings);
strResponse = gallagher.GetCardholdersByPdfValue(strPdfValue, "pdf_629");
```

Expression shape D (typed GET wrapper examples):
```csharp
// Reuse the same `gallagher` instance from the previous Expression shape.

// GET /api/personal_data_fields
strResponse = gallagher.GetPersonalDataFields();

// GET /api/personal_data_fields?name="ThirdPartyID"
strResponse = gallagher.GetPersonalDataFieldsByName("ThirdPartyID");

// GET /api/cardholders
strResponse = gallagher.GetCardholders();

// GET /api/cardholders?pdf_629="IDCARD.12345"
strResponse = gallagher.GetCardholdersByPdfValue("IDCARD.12345", "pdf_629");

// GET /api/access_groups
strResponse = gallagher.GetAccessGroups();

// GET /api/access_groups?name="6040-CHAMBERLAIN-B-11703"
strResponse = gallagher.FindAccessGroupsByName("6040-CHAMBERLAIN-B-11703");

// GET /api/access_groups/663/cardholders
strResponse = gallagher.GetAccessGroupCardholders("663");
```

Expression shape E (typed GET by-id wrapper examples):
```csharp
// GET /api/personal_data_fields/629
strResponse = gallagher.GetPersonalDataFieldById("629");

// GET /api/cardholders/653
strResponse = gallagher.GetCardholderById("653");

// GET /api/access_groups/663
strResponse = gallagher.GetAccessGroupById("663");

// GET /api/cardholders/653/access_groups
strResponse = gallagher.GetCardholderAccessGroups("653");
```

Expression shape F (certificate thumbprint example, BizTalk-friendly):
```csharp
// Set up settings with certificate thumbprint from SSO/config
Bham.BizTalk.Rest.BizTalkRestClientSettings settings = null;
settings = new Bham.BizTalk.Rest.BizTalkRestClientSettings();
settings.ApiKeyHeaderName = "Authorization";
settings.ApiKeyHeaderValue = strApiKey;
settings.CertThumbprint = strCertThumbprint;  // e.g., "ABC123DEF456..." from SSO
settings.TimeoutSeconds = 100;

// Call directly via PatchClient with thumbprint
strGetResponse =
	Bham.BizTalk.Rest.PatchClient.GetJsonWithClientCertAndApiKeyDefaultStore(
		strGallagherBaseUrl + "/cardholders/" + strCardholderId,
		"Authorization",
		strApiKey,
		strCertThumbprint,
		100);
```

Typical orchestration variables:
- `strGetResponse`
- `strGetXmlResponse`
- `strPatchResponse`
- `strPatchBody`
- `strPatchXmlResponse`
- `strPatchXmlBody`
- `strApiKey`
- `strCustomerId`
- `strOrderId`

Practical notes:
- Wrap the call in a Scope with an Exception Handler if you want to catch `BizTalkRestClientException`.
- Ensure the helper assembly is deployed where the BizTalk host can load it.
- Ensure the certificate exists in the store used by the BizTalk host instance account.

## Failure smoke-test scenarios
- `scenariomissingcert` verifies that a missing thumbprint is wrapped as `BizTalkRestClientException` with the original certificate lookup failure underneath.
- `scenariotimeoutpublic` and `scenariotimeoutpublicxml` start a local HTTP listener that intentionally delays the response so the client times out.
- `scenariononsuccesshttp` and `scenariononsuccesshttpxml` start a local HTTP listener that returns a configurable non-success status code.
- `scenariononsuccesshttps` and `scenariononsuccesshttpsxml` call a configurable HTTPS endpoint that is expected to return a non-success status code. The default is `https://httpstat.us/503`.
- `runfailurescenarios` and `runfailurescenariosxml` run all failure scenarios in sequence.

Examples:
```powershell
.\scripts\SmokeTest.ps1 -Mode scenariomissingcert
.\scripts\SmokeTest.ps1 -Mode scenariotimeoutpublic -DelayMilliseconds 5000 -TimeoutSeconds 1
.\scripts\SmokeTest.ps1 -Mode scenariotimeoutpublicxml -DelayMilliseconds 5000 -TimeoutSeconds 1
.\scripts\SmokeTest.ps1 -Mode scenariononsuccesshttp -StatusCode 503 -TimeoutSeconds 30
.\scripts\SmokeTest.ps1 -Mode scenariononsuccesshttpxml -StatusCode 503 -TimeoutSeconds 30 -Body "<error>forced-http-failure</error>"
.\scripts\SmokeTest.ps1 -Mode scenariononsuccesshttps -Url https://httpstat.us/503 -TimeoutSeconds 30
.\scripts\SmokeTest.ps1 -Mode scenariononsuccesshttpsxml -Url https://httpstat.us/503 -TimeoutSeconds 30
.\scripts\SmokeTest.ps1 -Mode runfailurescenarios -Url https://httpstat.us/503
.\scripts\SmokeTest.ps1 -Mode runfailurescenariosxml -Url https://httpstat.us/503
```

Example:
```csharp
var client = new BizTalkRestClient(
	new BizTalkRestClientSettings
	{
		ApiKeyHeaderName = "x-api-key",
		ApiKeyHeaderValue = "secret",
		Logger = entry => Console.WriteLine(
			$"[{entry.TimestampUtc:O}] [{entry.Level}] [{entry.Operation}] {entry.Message}")
	});

try
{
	var json = client.GetJson("https://example/api/orders");
}
catch (BizTalkRestClientException ex)
{
	Console.Error.WriteLine(ex.Message);
}
```

## JSON response parsing
`GallagherApiResponseParser` provides dependency-free methods to extract data from Gallagher API responses without external JSON libraries (works on .NET 4.6.1).

Example: extract cardholder ID from GetCardholders response:
```csharp
var cardholderJson = client.GetJson("https://api.example.com/cardholders?pdf_629=%22" + pdfValue + "%22");
var cardholderId = Bham.BizTalk.Rest.GallagherApiResponseParser.GetEntityIdByName(cardholderJson, null);
```

Example: find access group membership href by cardholder name and date range:
```csharp
var membershipJson = client.GetJson("https://api.example.com/access_groups/663/cardholders");
var found = Bham.BizTalk.Rest.GallagherApiResponseParser.TryGetAccessGroupMembershipHrefByNameAndDates(
	membershipJson,
	"John Smith",         // cardholder name (exact match, case-insensitive)
	"2026-04-01",         // from date (optional, null to skip)
	"2026-05-01",         // until date (optional, null to skip)
	out string href);     // membership href output

if (found)
{
	Console.WriteLine("Found membership href: " + href);
}
else
{
	Console.WriteLine("No matching membership found");
}
```

Example: parse date-flexible membership search in BizTalk Expression shape:
```csharp
// strMembershipJson = GetAccessGroupCardholders response containing multiple cardholder records
// strCardholderName = "John Smith" (to search for)
// strFromDate = "2026-04-01" (optional; can be null to skip date filtering)
// strUntilDate = "2026-05-01" (optional; can be null to skip date filtering)

boolean bFound = false;
string strMembershipHref = "";

if (Bham.BizTalk.Rest.GallagherApiResponseParser.TryGetAccessGroupMembershipHrefByNameAndDates(
	strMembershipJson,
	strCardholderName,
	strFromDate,
	strUntilDate,
	out strMembershipHref))
{
	bFound = true;
}
else
{
	// Handle no match found
}
```

Parser methods available:
- `GetFirstEntityId(json)` – extracts the first `id` field from nested objects
- `GetEntityIdByName(json, name)` – finds first object with matching `name` field, returns its `id`
- `GetAccessGroupMembershipHrefForCardholder(json, cardholderId)` – searches for membership with matching `cardholderId`, returns `href`
- `GetAccessGroupMembershipHrefByNameAndDates(json, cardholderName, fromDate, untilDate)` – finds membership by name and optional date range, returns `href`
- `TryGetAccessGroupMembershipHrefByNameAndDates(json, cardholderName, fromDate, untilDate, out href)` – Try-pattern version, returns bool

## Strong name and GAC (when you are ready)
1. Create an SNK file in the project folder:
```powershell
sn -k .\Bham.BizTalk.Rest\Bham.BizTalk.Rest.snk
```
2. Rebuild the project:
```powershell
dotnet build .\Bham.HelperClient.sln -c Release
```
3. Verify the assembly is strong-named:
```powershell
sn -vf .\Bham.BizTalk.Rest\bin\Release\Bham.BizTalk.Rest.dll
```
4. Add to GAC on your BizTalk server using your deployment process.

Note: project signing is configured to turn on automatically when `Bham.BizTalk.Rest.snk` exists.
