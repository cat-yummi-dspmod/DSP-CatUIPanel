# Thunderstore package: YummiUIPanel.dll + manifest + README + CHANGELOG + icon
$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$Out = Join-Path $Root 'bin\Release'
$Dll = Join-Path $Root 'bin\Release\net472\YummiUIPanel.dll'
$Zip = Join-Path $Root 'bin\Release\YummiUIPanel.zip'

if (-not (Test-Path $Dll)) {
    dotnet build (Join-Path $Root 'YummiUIPanel.csproj') -c Release
}

$Staging = Join-Path $env:TEMP ('yummi-uipanel-ts-' + [guid]::NewGuid().ToString('n'))
New-Item -ItemType Directory -Path $Staging | Out-Null
Copy-Item $Dll $Staging
Copy-Item (Join-Path $Root 'manifest.json') $Staging
Copy-Item (Join-Path $Root 'README.md') $Staging
Copy-Item (Join-Path $Root 'CHANGELOG.md') $Staging
Copy-Item (Join-Path $Root 'icon.png') $Staging
if (Test-Path $Zip) { Remove-Item $Zip -Force }
Compress-Archive -Path (Join-Path $Staging '*') -DestinationPath $Zip
Remove-Item $Staging -Recurse -Force
Write-Host "Wrote $Zip"
