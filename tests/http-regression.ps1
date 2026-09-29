$ErrorActionPreference = 'Stop'
$url = 'http://127.0.0.1:5188'
$project = Join-Path $PSScriptRoot '..\src\NetTrello.Api\NetTrello.Api.csproj'
$log = Join-Path $env:TEMP 'nettrello-http-regression.log'
$errorLog = Join-Path $env:TEMP 'nettrello-http-regression.err.log'
$env:DOTNET_ROLL_FORWARD = 'Major'
$process = Start-Process -FilePath 'dotnet' -ArgumentList @('run', '--project', $project, '--no-build', '--no-launch-profile', '--', '--urls', $url) -WindowStyle Hidden -RedirectStandardOutput $log -RedirectStandardError $errorLog -PassThru

function Invoke-ApiPatch([string]$uri, [string]$json) {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Method Patch -Uri $uri -ContentType 'application/json' -Body $json
        return @{ Status = [int]$response.StatusCode; Body = $response.Content }
    } catch {
        $response = $_.Exception.Response
        $reader = New-Object IO.StreamReader($response.GetResponseStream())
        try { $body = $reader.ReadToEnd() } finally { $reader.Dispose() }
        return @{ Status = [int]$response.StatusCode; Body = $body }
    }
}

try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-WebRequest -UseBasicParsing -Uri "$url/health"
            if ($health.StatusCode -eq 200) { $ready = $true; break }
        } catch { }
    }
    if (-not $ready) { throw "API did not become healthy. See $log" }

    $board = (Invoke-WebRequest -UseBasicParsing -Uri "$url/api/boards/78e3d235-44e3-4668-aa9b-81583d4a5fd5").Content | ConvertFrom-Json
    $card = $board.columns[0].cards[0]
    $originalDescription = $card.description

    $titleResponse = Invoke-ApiPatch "$url/api/boards/78e3d235-44e3-4668-aa9b-81583d4a5fd5/cards/$($card.id)" '{"title":"HTTP title only"}'
    if ($titleResponse.Status -ne 200) { throw "title-only PATCH returned $($titleResponse.Status)" }
    $titleBody = $titleResponse.Body | ConvertFrom-Json
    $updatedCard = $titleBody.columns[0].cards | Where-Object id -eq $card.id
    if ($updatedCard.title -ne 'HTTP title only' -or $updatedCard.description -ne $originalDescription) { throw 'title-only PATCH did not preserve omitted fields' }

    $badResponse = Invoke-ApiPatch "$url/api/boards/78e3d235-44e3-4668-aa9b-81583d4a5fd5/cards/$($card.id)" '{"priority":"Imaginary"}'
    $badBody = $badResponse.Body | ConvertFrom-Json
    if ($badResponse.Status -ne 400 -or $badBody.code -ne 'card.priority_invalid') { throw 'invalid priority did not return the contract problem response' }

    $emptyResponse = Invoke-ApiPatch "$url/api/boards/78e3d235-44e3-4668-aa9b-81583d4a5fd5/cards/$($card.id)" '{}'
    $emptyBody = $emptyResponse.Body | ConvertFrom-Json
    if ($emptyResponse.Status -ne 400 -or $emptyBody.code -ne 'card.patch_empty') { throw 'empty PATCH did not return the no-op contract response' }

    Write-Output 'NetTrello HTTP checks passed: serialized title-only PATCH, invalid enum problem, empty patch problem.'
}
finally {
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    Remove-Item -LiteralPath $log -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $errorLog -Force -ErrorAction SilentlyContinue
}
