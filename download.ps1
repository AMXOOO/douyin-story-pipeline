param(
    [string]$UrlFile = "$PSScriptRoot\urls.txt",
    [string]$OutputRoot = "$PSScriptRoot\data\raw",
    [int]$BatchSize = 5,
    [int]$MinSleepSeconds = 15,
    [int]$MaxSleepSeconds = 35
)

$ErrorActionPreference = "Stop"
$archive = "$PSScriptRoot\data\archive.txt"

function Find-Tool {
    param(
        [string]$LocalPath,
        [string]$CommandName
    )

    if (Test-Path -LiteralPath $LocalPath) {
        return (Resolve-Path -LiteralPath $LocalPath).Path
    }

    $command = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "Missing tool: $CommandName. Run check-tools.ps1 for details."
}

$tool = Find-Tool "$PSScriptRoot\.tools\yt-dlp.exe" "yt-dlp"
$ffmpeg = Find-Tool "$PSScriptRoot\.tools\ffmpeg.exe" "ffmpeg"

if (-not (Test-Path -LiteralPath $UrlFile)) {
    throw "URL list not found: $UrlFile"
}

$urls = Get-Content -LiteralPath $UrlFile -Encoding UTF8 |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith("#") }

if (-not $urls) {
    throw "No video URLs found in urls.txt"
}

if ($BatchSize -lt 1 -or $BatchSize -gt 20) {
    throw "BatchSize must be between 1 and 20"
}

if ($MinSleepSeconds -lt 5 -or $MaxSleepSeconds -lt $MinSleepSeconds) {
    throw "Use a sleep range of at least 5 seconds"
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $archive) | Out-Null

$downloadArgs = @(
    "--abort-on-error",
    "--continue",
    "--no-overwrites",
    "--ffmpeg-location", (Split-Path $ffmpeg),
    "--download-archive", $archive,
    "--max-downloads", $BatchSize,
    "--sleep-requests", "3",
    "--sleep-interval", $MinSleepSeconds,
    "--max-sleep-interval", $MaxSleepSeconds,
    "--retries", "2",
    "--extractor-retries", "2",
    "--fragment-retries", "2",
    "--write-info-json",
    "--write-description",
    "--write-thumbnail",
    "--no-write-comments",
    "--windows-filenames",
    "--merge-output-format", "mp4",
    "--format", "bv*+ba/b",
    "--output", "$OutputRoot\%(uploader_id,uploader|unknown)s\%(upload_date|unknown_date)s_%(id)s\%(id)s.%(ext)s"
)

& $tool @downloadArgs -- $urls
if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 101) {
    throw "Download failed with exit code: $LASTEXITCODE"
}

Write-Host "Batch complete. Maximum new downloads: $BatchSize"
Write-Host "Downloaded IDs are recorded in: $archive"
