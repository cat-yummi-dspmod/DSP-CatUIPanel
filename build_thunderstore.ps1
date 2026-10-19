# Thunderstore package: CatUIPanel.dll + manifest + README + CHANGELOG + icon
$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$Out = Join-Path $Root 'bin\Release'
$Dll = Join-Path $Root 'bin\Release\net472\CatUIPanel.dll'
$Zip = Join-Path $Root 'bin\Release\CatUIPanel.zip'

if (-not (Test-Path $Dll)) {
    dotnet build (Join-Path $Root 'CatUIPanel.csproj') -c Release
}

Add-Type -AssemblyName System.Drawing
$iconPath = Join-Path $Root 'icon.png'
$iconImg = [System.Drawing.Image]::FromFile($iconPath)
if ($iconImg.Width -ne 256 -or $iconImg.Height -ne 256) {
    $bad = "{0}x{1}" -f $iconImg.Width, $iconImg.Height
    $iconImg.Dispose()
    throw "icon.png must be exactly 256x256 PNG (Thunderstore); got $bad."
}
$iconImg.Dispose()
$iconSig = [System.IO.File]::ReadAllBytes($iconPath)[0..7]
if ($iconSig[0] -ne 0x89 -or $iconSig[1] -ne 0x50) { throw 'icon.png must be PNG format (Thunderstore).' }

$Staging = Join-Path $env:TEMP ('cat-uipanel-ts-' + [guid]::NewGuid().ToString('n'))
New-Item -ItemType Directory -Path $Staging | Out-Null
Copy-Item $Dll $Staging
Copy-Item (Join-Path $Root 'manifest.json') $Staging
Copy-Item (Join-Path $Root 'README.md') $Staging
Copy-Item (Join-Path $Root 'CHANGELOG.md') $Staging
Copy-Item (Join-Path $Root 'icon.png') $Staging
$Screens = Join-Path $Root 'screenshots'
if (Test-Path $Screens) {
    Copy-Item $Screens (Join-Path $Staging 'screenshots') -Recurse
}
if (Test-Path $Zip) { Remove-Item $Zip -Force }
Compress-Archive -Path (Join-Path $Staging '*') -DestinationPath $Zip
Remove-Item $Staging -Recurse -Force
Write-Host "Wrote $Zip"
