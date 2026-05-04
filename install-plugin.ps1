param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "PinyinHelper.csproj"
$targetFramework = "net9.0-windows10.0.22621.0"
$outputDir = Join-Path $PSScriptRoot "bin\$Configuration\$targetFramework"
$pluginRoot = Join-Path $env:LOCALAPPDATA "Microsoft\PowerToys\PowerToys Run\Plugins"
$pluginDir = Join-Path $pluginRoot "PinyinHelper"
$launcherProcess = Get-Process PowerToys.PowerLauncher -ErrorAction SilentlyContinue | Select-Object -First 1
$launcherPath = $launcherProcess.Path
$stoppedLauncher = $false

& (Join-Path $PSScriptRoot "PrepareIcon.ps1")
dotnet build $project -c $Configuration

if ($launcherProcess) {
    Write-Host "Stopping PowerToys Run so plugin files can be replaced..."
    Stop-Process -Id $launcherProcess.Id -Force
    Start-Sleep -Seconds 1
    $stoppedLauncher = $true
}

if (-not (Test-Path $pluginRoot)) {
    New-Item -ItemType Directory -Path $pluginRoot | Out-Null
}

if (Test-Path $pluginDir) {
    Remove-Item $pluginDir -Recurse -Force
}

New-Item -ItemType Directory -Path $pluginDir | Out-Null

$filesToCopy = @(
    "Community.PowerToys.Run.Plugin.PinyinHelper.dll",
    "Community.PowerToys.Run.Plugin.PinyinHelper.deps.json",
    "PinYinConverterCore.dll",
    "plugin.json"
)

foreach ($file in $filesToCopy) {
    $source = Join-Path $outputDir $file
    if (Test-Path $source) {
        Copy-Item $source $pluginDir -Force
    }
}

$imageSource = Join-Path $outputDir "Images"
if (Test-Path $imageSource) {
    Copy-Item $imageSource $pluginDir -Recurse -Force
}

$imageCache = Join-Path $env:LOCALAPPDATA "Microsoft\PowerToys\PowerToys Run\Settings\ImageCache.json"
$imageCacheVersion = Join-Path $env:LOCALAPPDATA "Microsoft\PowerToys\PowerToys Run\Settings\ImageCache_version.txt"
foreach ($cacheFile in @($imageCache, $imageCacheVersion)) {
    if (Test-Path $cacheFile) {
        Remove-Item $cacheFile -Force
    }
}

if ($stoppedLauncher -and -not [string]::IsNullOrWhiteSpace($launcherPath) -and (Test-Path $launcherPath)) {
    Start-Process -FilePath $launcherPath -WindowStyle Hidden
}

Write-Host ""
Write-Host "Installed plugin to:"
Write-Host $pluginDir
Write-Host ""
Write-Host "Next:"
Write-Host "1. Restart PowerToys or toggle PowerToys Run off and on."
Write-Host "2. Open PowerToys Run and type: ! 今天 or ! su3 cl3"
