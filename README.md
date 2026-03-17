# Bham.HelperClient

BizTalk 2016 helper library that adds outbound REST support for HTTP GET and HTTP PATCH with:
- Client certificate from Windows certificate store (thumbprint lookup)
- API key header injection
- Static HttpClient reuse for connection pooling

## Target framework
- .NET Framework 4.6 (`net46`)

## Project output
- `Bham.BizTalk.Rest.dll`

## Build
```powershell
dotnet build .\Bham.HelperClient.sln -c Release
```

## Public helper methods
- `PatchClient.GetJsonWithClientCertAndApiKey(...)`
- `PatchClient.PatchJsonWithClientCertAndApiKey(...)`

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
