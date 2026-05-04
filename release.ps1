param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$plugin = Get-Content (Join-Path $PSScriptRoot "plugin.json") -Raw | ConvertFrom-Json
$version = [string]$plugin.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "plugin.json does not contain a Version value."
}

$distDir = Join-Path $PSScriptRoot "dist"
$packageDir = Join-Path $distDir "PinyinHelper"
$zipPath = Join-Path $distDir "PinyinHelper-v$version.zip"

& (Join-Path $PSScriptRoot "build.ps1") -Configuration $Configuration

if (-not (Test-Path $packageDir)) {
    throw "Package directory was not created: $packageDir"
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path $packageDir -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host ""
Write-Host "Release package created:"
Write-Host $zipPath
Write-Host ""
Write-Host "Install by extracting the PinyinHelper folder into:"
Write-Host "%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins"
