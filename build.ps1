param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "PinyinHelper.csproj"
$targetFramework = "net9.0-windows10.0.22621.0"
$outputDir = Join-Path $PSScriptRoot "bin\$Configuration\$targetFramework"
$packageDir = Join-Path $PSScriptRoot "dist\PinyinHelper"

& (Join-Path $PSScriptRoot "PrepareIcon.ps1")
dotnet build $project -c $Configuration

if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}

New-Item -ItemType Directory -Path $packageDir | Out-Null

$filesToCopy = @(
    "Community.PowerToys.Run.Plugin.PinyinHelper.dll",
    "Community.PowerToys.Run.Plugin.PinyinHelper.deps.json",
    "PinYinConverterCore.dll",
    "plugin.json"
)

foreach ($file in $filesToCopy) {
    $source = Join-Path $outputDir $file
    if (Test-Path $source) {
        Copy-Item $source $packageDir -Force
    }
}

$imageSource = Join-Path $outputDir "Images"
if (Test-Path $imageSource) {
    Copy-Item $imageSource $packageDir -Recurse -Force
}

Write-Host ""
Write-Host "Packaged plugin files to:"
Write-Host $packageDir
Write-Host ""
Write-Host "Use .\\install-plugin.ps1 to copy the build into the PowerToys plugins folder."
