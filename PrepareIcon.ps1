param(
    [string]$InputPath = (Join-Path $PSScriptRoot "Assets\PinyinHelperIcon.png"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "Assets\\PowerToysIcon.png")
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $InputPath)) {
    throw "Icon source not found: $InputPath"
}

$inputItem = Get-Item $InputPath
if (Test-Path $OutputPath) {
    $outputItem = Get-Item $OutputPath
    if ($outputItem.LastWriteTimeUtc -ge $inputItem.LastWriteTimeUtc) {
        Write-Host "Icon already up to date: $OutputPath"
        exit 0
    }
}

Add-Type -AssemblyName System.Drawing

$resolvedInput = (Resolve-Path $InputPath).Path
$resolvedOutput = $OutputPath
$outputDir = Split-Path -Parent $resolvedOutput
if (-not [string]::IsNullOrWhiteSpace($outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$source = [System.Drawing.Bitmap]::FromFile($resolvedInput)
$working = New-Object System.Drawing.Bitmap($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($working)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.DrawImage($source, 0, 0, $source.Width, $source.Height)
$graphics.Dispose()
$source.Dispose()

$width = $working.Width
$height = $working.Height
$visited = New-Object 'bool[]' ($width * $height)
$queue = [System.Collections.Generic.Queue[object]]::new()

function Get-Offset([int]$x, [int]$y, [int]$canvasWidth) {
    return ($y * $canvasWidth) + $x
}

function Test-BackgroundPixel([System.Drawing.Color]$color) {
    if ($color.A -eq 0) {
        return $true
    }

    $max = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
    $min = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
    return ($max -ge 228) -and ($min -ge 228) -and (($max - $min) -le 30)
}

function Enqueue-IfBackground(
    [System.Drawing.Bitmap]$bitmap,
    [bool[]]$seen,
    [System.Collections.Generic.Queue[object]]$items,
    [int]$canvasWidth,
    [int]$canvasHeight,
    [int]$x,
    [int]$y
) {
    if ($x -lt 0 -or $y -lt 0 -or $x -ge $canvasWidth -or $y -ge $canvasHeight) {
        return
    }

    $offset = Get-Offset $x $y $canvasWidth
    if ($seen[$offset]) {
        return
    }

    if (-not (Test-BackgroundPixel $bitmap.GetPixel($x, $y))) {
        return
    }

    $seen[$offset] = $true
    $items.Enqueue(@($x, $y))
}

for ($x = 0; $x -lt $width; $x++) {
    Enqueue-IfBackground $working $visited $queue $width $height $x 0
    Enqueue-IfBackground $working $visited $queue $width $height $x ($height - 1)
}

for ($y = 0; $y -lt $height; $y++) {
    Enqueue-IfBackground $working $visited $queue $width $height 0 $y
    Enqueue-IfBackground $working $visited $queue $width $height ($width - 1) $y
}

while ($queue.Count -gt 0) {
    $point = $queue.Dequeue()
    $x = [int]$point[0]
    $y = [int]$point[1]
    $pixel = $working.GetPixel($x, $y)
    $working.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, $pixel.R, $pixel.G, $pixel.B))

    Enqueue-IfBackground $working $visited $queue $width $height ($x + 1) $y
    Enqueue-IfBackground $working $visited $queue $width $height ($x - 1) $y
    Enqueue-IfBackground $working $visited $queue $width $height $x ($y + 1)
    Enqueue-IfBackground $working $visited $queue $width $height $x ($y - 1)
}

$left = $width
$top = $height
$right = -1
$bottom = -1

for ($y = 0; $y -lt $height; $y++) {
    for ($x = 0; $x -lt $width; $x++) {
        if ($working.GetPixel($x, $y).A -eq 0) {
            continue
        }

        if ($x -lt $left) { $left = $x }
        if ($y -lt $top) { $top = $y }
        if ($x -gt $right) { $right = $x }
        if ($y -gt $bottom) { $bottom = $y }
    }
}

if ($right -lt $left -or $bottom -lt $top) {
    $working.Dispose()
    throw "Prepared icon ended up fully transparent."
}

$boundsWidth = ($right - $left) + 1
$boundsHeight = ($bottom - $top) + 1
$padding = [Math]::Max(8, [int]([Math]::Min($boundsWidth, $boundsHeight) / 72))

$cropX = [Math]::Max(0, $left - $padding)
$cropY = [Math]::Max(0, $top - $padding)
$cropRight = [Math]::Min($width, $right + $padding + 1)
$cropBottom = [Math]::Min($height, $bottom + $padding + 1)
$cropWidth = $cropRight - $cropX
$cropHeight = $cropBottom - $cropY

$cropped = New-Object System.Drawing.Bitmap($cropWidth, $cropHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$cropGraphics = [System.Drawing.Graphics]::FromImage($cropped)
$cropGraphics.Clear([System.Drawing.Color]::Transparent)
$destinationRect = New-Object System.Drawing.Rectangle(0, 0, $cropWidth, $cropHeight)
$sourceRect = New-Object System.Drawing.Rectangle($cropX, $cropY, $cropWidth, $cropHeight)
$cropGraphics.DrawImage($working, $destinationRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
$cropGraphics.Dispose()
$working.Dispose()

$cropped.Save($resolvedOutput, [System.Drawing.Imaging.ImageFormat]::Png)
$cropped.Dispose()

Write-Host "Prepared icon:" $resolvedOutput
