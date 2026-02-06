[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$headers = @{
    'X-User-Id' = '44444444-4444-4444-4444-444444444444'
    'X-User-Role' = 'systemAdmin'
}

# Get base-single-door assembly
$assemblies = Invoke-RestMethod -Uri 'http://localhost:5008/assemblies' -Headers $headers
$target = $assemblies.data | Where-Object { $_.type -eq 'base-single-door' }
$detail = Invoke-RestMethod -Uri "http://localhost:5008/assemblies/$($target.id)" -Headers $headers
$json = $detail | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText('C:\Users\kalmy\RiderProjects\Domeo\scripts\example-assembly.json', $json, [System.Text.Encoding]::UTF8)
Write-Host "Saved"
