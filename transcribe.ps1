param(
    [string]$RawRoot = "$PSScriptRoot\data\raw",
    [string]$TranscriptRoot = "$PSScriptRoot\data\transcripts"
)

$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    # whisper.cpp on Windows can fail on non-ASCII absolute paths.
    $ffmpeg = ".tools\ffmpeg.exe"
    $whisper = ".tools\whisper-cli.exe"
    $model = ".tools\models\ggml-small-q5_1.bin"
    $vadModel = ".tools\models\ggml-silero-v6.2.0.bin"

    foreach ($required in @($ffmpeg, $whisper, $model, $vadModel)) {
        if (-not (Test-Path -LiteralPath $required)) {
            throw "Missing transcription dependency: $required"
        }
    }

    New-Item -ItemType Directory -Force -Path $TranscriptRoot | Out-Null

    $videos = Get-ChildItem -LiteralPath $RawRoot -Recurse -File |
        Where-Object { $_.Extension -in ".mp4", ".mkv", ".webm", ".mov" }

    foreach ($video in $videos) {
        $id = $video.BaseName
        $targetDir = Join-Path $TranscriptRoot $id
        $audio = Join-Path $targetDir "$id.wav"
        $outputBase = Join-Path $targetDir $id
        $transcript = "$outputBase.txt"

        if (Test-Path -LiteralPath $transcript) {
            Write-Host "Skipping existing transcript: $id"
            continue
        }

        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

        $videoArg = Resolve-Path -LiteralPath $video.FullName -Relative
        $targetDirArg = Resolve-Path -LiteralPath $targetDir -Relative
        $audioArg = Join-Path $targetDirArg "$id.wav"
        $outputBaseArg = Join-Path $targetDirArg $id

        & $ffmpeg -hide_banner -loglevel error -y -i $videoArg `
            -vn -ac 1 -ar 16000 -c:a pcm_s16le $audioArg
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Audio extraction failed: $($video.FullName)"
            continue
        }

        & $whisper -m $model -f $audioArg -l zh -t 6 --vad -vm $vadModel -sns -otxt -of $outputBaseArg
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Transcription failed: $($video.FullName)"
            continue
        }

        Remove-Item -LiteralPath $audio -Force
    }

    Write-Host "Transcription complete: $TranscriptRoot"
}
finally {
    Pop-Location
}
