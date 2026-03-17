param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('getpublicjson', 'patchpublicjson', 'scenariomissingcert', 'scenariotimeoutpublic', 'scenariotimeoutpublicxml', 'scenariononsuccesshttp', 'scenariononsuccesshttpxml', 'scenariononsuccesshttps', 'scenariononsuccesshttpsxml', 'runfailurescenarios', 'runfailurescenariosxml')]
    [string]$Mode,

    [Parameter(Mandatory = $true)]
    [string]$Url,

    [string]$Query = '-',

    [string]$Body = '{"status":"Done"}',

    [int]$TimeoutSeconds = 100,

    [int]$DelayMilliseconds = 5000,

    [int]$StatusCode = 503,

    [string]$ApiHeaderName = 'x-api-key',

    [string]$ApiHeaderValue = 'smoke-test',

    [string]$Thumbprint = '0000000000000000000000000000000000000000'
)

$ErrorActionPreference = 'Stop'

function Build-Url {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaseUrl,

        [string]$QueryString = '-'
    )

    if ([string]::IsNullOrWhiteSpace($QueryString) -or $QueryString -eq '-') {
        return $BaseUrl
    }

    $pairs = $QueryString -split '&' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    $encodedPairs = New-Object System.Collections.Generic.List[string]

    foreach ($pair in $pairs) {
        $separatorIndex = $pair.IndexOf('=')
        if ($separatorIndex -lt 0) {
            $encodedPairs.Add([System.Uri]::EscapeDataString($pair) + '=')
            continue
        }

        $key = $pair.Substring(0, $separatorIndex)
        $value = $pair.Substring($separatorIndex + 1)
        $encodedPairs.Add(([System.Uri]::EscapeDataString($key) + '=' + [System.Uri]::EscapeDataString($value)))
    }

    if ($encodedPairs.Count -eq 0) {
        return $BaseUrl
    }

    $joiner = '?'
    if ($BaseUrl.Contains('?')) {
        $joiner = '&'
    }

    return $BaseUrl + $joiner + ($encodedPairs -join '&')
}

if ($Mode -eq 'getpublicjson') {
    $requestUrl = Build-Url -BaseUrl $Url -QueryString $Query
    $response = Invoke-RestMethod -Uri $requestUrl -Method Get -TimeoutSec $TimeoutSeconds

    Write-Host 'PUBLIC GET JSON succeeded.' -ForegroundColor Green
    $response | ConvertTo-Json -Depth 20
    exit 0
}

if ($Mode -eq 'patchpublicjson') {
    $response = Invoke-RestMethod -Uri $Url -Method Patch -ContentType 'application/json' -Body $Body -TimeoutSec $TimeoutSeconds

    Write-Host 'PUBLIC PATCH JSON succeeded.' -ForegroundColor Green
    $response | ConvertTo-Json -Depth 20
    exit 0
}

$projectPath = Join-Path $PSScriptRoot '..\Bham.BizTalk.Rest.SmokeTest\Bham.BizTalk.Rest.SmokeTest.csproj'

if ($Mode -eq 'scenariomissingcert') {
    dotnet run --project $projectPath -- scenariomissingcert $Url $ApiHeaderName $ApiHeaderValue $Thumbprint LocalMachine My $TimeoutSeconds
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariotimeoutpublic') {
    dotnet run --project $projectPath -- scenariotimeoutpublic $DelayMilliseconds $TimeoutSeconds
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariotimeoutpublicxml') {
    dotnet run --project $projectPath -- scenariotimeoutpublicxml $DelayMilliseconds $TimeoutSeconds
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariononsuccesshttp') {
    dotnet run --project $projectPath -- scenariononsuccesshttp $StatusCode $TimeoutSeconds $Body
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariononsuccesshttpxml') {
    dotnet run --project $projectPath -- scenariononsuccesshttpxml $StatusCode $TimeoutSeconds $Body
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariononsuccesshttps') {
    dotnet run --project $projectPath -- scenariononsuccesshttps $Url $TimeoutSeconds
    exit $LASTEXITCODE
}

if ($Mode -eq 'scenariononsuccesshttpsxml') {
    dotnet run --project $projectPath -- scenariononsuccesshttpsxml $Url $TimeoutSeconds
    exit $LASTEXITCODE
}

if ($Mode -eq 'runfailurescenarios') {
    dotnet run --project $projectPath -- runfailurescenarios $Url
    exit $LASTEXITCODE
}

if ($Mode -eq 'runfailurescenariosxml') {
    dotnet run --project $projectPath -- runfailurescenariosxml $Url
    exit $LASTEXITCODE
}
