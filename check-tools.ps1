$ErrorActionPreference = "Stop"

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

    return $null
}

$checks = @(
    [PSCustomObject]@{
        Name = "yt-dlp"
        Path = Find-Tool "$PSScriptRoot\.tools\yt-dlp.exe" "yt-dlp"
        RequiredFor = "download"
    },
    [PSCustomObject]@{
        Name = "FFmpeg"
        Path = Find-Tool "$PSScriptRoot\.tools\ffmpeg.exe" "ffmpeg"
        RequiredFor = "download and transcription"
    },
    [PSCustomObject]@{
        Name = "whisper.cpp"
        Path = Find-Tool "$PSScriptRoot\.tools\whisper-cli.exe" "whisper-cli"
        RequiredFor = "transcription"
    },
    [PSCustomObject]@{
        Name = "Whisper model"
        Path = if (Test-Path "$PSScriptRoot\.tools\models\ggml-small-q5_1.bin") {
            (Resolve-Path "$PSScriptRoot\.tools\models\ggml-small-q5_1.bin").Path
        } else {
            $null
        }
        RequiredFor = "transcription"
    },
    [PSCustomObject]@{
        Name = "Silero VAD model"
        Path = if (Test-Path "$PSScriptRoot\.tools\models\ggml-silero-v6.2.0.bin") {
            (Resolve-Path "$PSScriptRoot\.tools\models\ggml-silero-v6.2.0.bin").Path
        } else {
            $null
        }
        RequiredFor = "transcription"
    }
)

$checks | Format-Table Name, @{ Label = "Status"; Expression = { if ($_.Path) { "OK" } else { "MISSING" } } }, RequiredFor, Path -AutoSize

if ($checks.Where({ -not $_.Path }).Count -gt 0) {
    Write-Host ""
    Write-Host "Some tools are missing. See README.md for installation links."
    exit 1
}
