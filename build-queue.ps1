param(
    [string]$RawRoot = "$PSScriptRoot\data\raw",
    [string]$TranscriptRoot = "$PSScriptRoot\data\transcripts",
    [string]$CardRoot = "$PSScriptRoot\data\cards",
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$templatePath = "$PSScriptRoot\card-template.md"

if (-not (Test-Path -LiteralPath $templatePath)) {
    throw "Card template not found: $templatePath"
}

$template = Get-Content -Raw -Encoding UTF8 -LiteralPath $templatePath
New-Item -ItemType Directory -Force -Path $CardRoot | Out-Null

$infoFiles = Get-ChildItem -LiteralPath $RawRoot -Recurse -Filter "*.info.json" -File

foreach ($infoFile in $infoFiles) {
    $info = Get-Content -Raw -Encoding UTF8 -LiteralPath $infoFile.FullName | ConvertFrom-Json
    $id = [string]$info.id
    if (-not $id) {
        continue
    }

    $card = Join-Path $CardRoot "$id.md"
    $transcriptFile = Join-Path (Join-Path $TranscriptRoot $id) "$id.txt"
    $transcript = if (Test-Path -LiteralPath $transcriptFile) {
        Get-Content -Raw -Encoding UTF8 -LiteralPath $transcriptFile
    } else {
        "[TRANSCRIPT_PENDING]"
    }

    $sourceUrl = if ($info.webpage_url) { $info.webpage_url } else { $info.original_url }
    $title = if ($info.title) { $info.title } else { "Untitled video $id" }
    $description = if ($info.description) { $info.description } else { "" }

    $content = $template.
        Replace("{{TITLE}}", [string]$title).
        Replace("{{ID}}", $id).
        Replace("{{UPLOADER}}", [string]$info.uploader).
        Replace("{{UPLOAD_DATE}}", [string]$info.upload_date).
        Replace("{{SOURCE_URL}}", [string]$sourceUrl).
        Replace("{{DURATION}}", [string]$info.duration).
        Replace("{{DESCRIPTION}}", [string]$description).
        Replace("{{TRANSCRIPT}}", [string]$transcript)

    if ($Force -or -not (Test-Path -LiteralPath $card)) {
        Set-Content -LiteralPath $card -Value $content -Encoding UTF8
        continue
    }

    $existing = Get-Content -Raw -Encoding UTF8 -LiteralPath $card
    if ($existing.Contains("[TRANSCRIPT_PENDING]") -and -not $transcript.Contains("[TRANSCRIPT_PENDING]")) {
        Set-Content -LiteralPath $card -Value $content -Encoding UTF8
    }
}

Write-Host "Analysis cards generated: $CardRoot"
