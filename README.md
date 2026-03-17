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

## Build
```powershell
dotnet build .\Bham.HelperClient.sln -c Release
```

## Public helper methods
- `PatchClient.GetJsonWithClientCertAndApiKey(...)`
- `PatchClient.GetXmlWithClientCertAndApiKey(...)`
- `PatchClient.PatchJsonWithClientCertAndApiKey(...)`
- `PatchClient.PatchXmlWithClientCertAndApiKey(...)`

## Error handling and logging
- GET and PATCH failures now surface as `BizTalkRestClientException`, including the HTTP method, URL, optional status code, and response body when one is available.
- `BizTalkRestClientSettings.Logger` accepts an `Action<BizTalkRestLogEntry>` so callers can forward start/success/failure events into BizTalk tracing, Event Log, or another sink.
- The smoke test now writes library log events to the console automatically.

## Certificate selection behavior
- `certThumbprint` is optional on `GetJsonWithClientCertAndApiKey`, `GetXmlWithClientCertAndApiKey`, `PatchJsonWithClientCertAndApiKey`, and `PatchXmlWithClientCertAndApiKey`.
- When a thumbprint is provided, the helper resolves and attaches that certificate from the configured store (`StoreLocation` + `StoreName`).
- When no thumbprint is provided, the helper sends the request without a client certificate (API-key/header authentication only).

## BizTalk compatibility note (.NET Framework 4.6.1)
- The helper attaches client certificates to `HttpClientHandler` using reflection to support BizTalk projects where `ClientCertificates` is not visible at compile time due to `System.Net.Http` API-surface differences.
- Runtime still requires a `System.Net.Http` implementation that supports client certificate attachment.
- If runtime support is missing, the helper throws a clear `InvalidOperationException` describing the reference issue.

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
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariomissingcert
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariotimeoutpublic 5000 1
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariotimeoutpublicxml 5000 1
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariononsuccesshttp 503 30
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariononsuccesshttpxml 503 30 "<error>forced-http-failure</error>"
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariononsuccesshttps https://httpstat.us/503 30
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- scenariononsuccesshttpsxml https://httpstat.us/503 30
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- runfailurescenarios https://httpstat.us/503
dotnet run --project .\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj -- runfailurescenariosxml https://httpstat.us/503
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
sn -vf .\Bham.BizTalk.Rest\bin\Release\net46\Bham.BizTalk.Rest.dll
```
4. Add to GAC on your BizTalk server using your deployment process.

Note: project signing is configured to turn on automatically when `Bham.BizTalk.Rest.snk` exists.
