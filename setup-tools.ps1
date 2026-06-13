$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$toolRoot = Join-Path $PSScriptRoot ".tools"
$modelRoot = Join-Path $toolRoot "models"
$downloadRoot = Join-Path $toolRoot "downloads"
$extractRoot = Join-Path $toolRoot "extract"

New-Item -ItemType Directory -Force $toolRoot, $modelRoot, $downloadRoot, $extractRoot | Out-Null

function Download-File {
    param(
        [string]$Url,
        [string]$Destination,
        [string]$Label
    )

    if (Test-Path -LiteralPath $Destination) {
        Write-Host "[已有] $Label"
        return
    }

    Write-Host "[下载] $Label"
    $partial = "$Destination.partial"
    Remove-Item -LiteralPath $partial -Force -ErrorAction SilentlyContinue

    if (Get-Command curl.exe -ErrorAction SilentlyContinue) {
        & curl.exe -L --fail --retry 3 --connect-timeout 30 -o $partial $Url
        if ($LASTEXITCODE -ne 0) {
            throw "下载失败：$Label"
        }
    } else {
        Invoke-WebRequest -UseBasicParsing -Uri $Url -OutFile $partial
    }

    Move-Item -LiteralPath $partial -Destination $Destination -Force
}

function Get-LatestRelease {
    param([string]$Repository)

    Invoke-RestMethod `
        -Headers @{ "User-Agent" = "douyin-story-pipeline" } `
        -Uri "https://api.github.com/repos/$Repository/releases/latest"
}

Write-Host ""
Write-Host "正在准备本地工具。首次运行大约需要下载 400MB，时间取决于网络速度。"
Write-Host ""

$ytDlpRelease = Get-LatestRelease "yt-dlp/yt-dlp"
$ytDlpAsset = $ytDlpRelease.assets | Where-Object { $_.name -eq "yt-dlp.exe" } | Select-Object -First 1
if (-not $ytDlpAsset) {
    throw "没有找到 yt-dlp Windows 版本。"
}
Download-File $ytDlpAsset.browser_download_url (Join-Path $toolRoot "yt-dlp.exe") "yt-dlp"

if (-not (Test-Path (Join-Path $toolRoot "ffmpeg.exe"))) {
    $ffmpegRelease = Get-LatestRelease "BtbN/FFmpeg-Builds"
    $ffmpegAsset = $ffmpegRelease.assets |
        Where-Object { $_.name -match "^ffmpeg-.*-win64-gpl\.zip$" } |
        Select-Object -First 1
    if (-not $ffmpegAsset) {
        throw "没有找到 FFmpeg Windows x64 版本。"
    }

    $ffmpegZip = Join-Path $downloadRoot "ffmpeg.zip"
    Download-File $ffmpegAsset.browser_download_url $ffmpegZip "FFmpeg"
    $ffmpegExtract = Join-Path $extractRoot "ffmpeg"
    Remove-Item -LiteralPath $ffmpegExtract -Recurse -Force -ErrorAction SilentlyContinue
    Expand-Archive -LiteralPath $ffmpegZip -DestinationPath $ffmpegExtract -Force

    $ffmpegExe = Get-ChildItem $ffmpegExtract -Recurse -Filter "ffmpeg.exe" | Select-Object -First 1
    $ffprobeExe = Get-ChildItem $ffmpegExtract -Recurse -Filter "ffprobe.exe" | Select-Object -First 1
    if (-not $ffmpegExe -or -not $ffprobeExe) {
        throw "FFmpeg 压缩包中缺少程序文件。"
    }
    Copy-Item $ffmpegExe.FullName (Join-Path $toolRoot "ffmpeg.exe") -Force
    Copy-Item $ffprobeExe.FullName (Join-Path $toolRoot "ffprobe.exe") -Force
}
else {
    Write-Host "[已有] FFmpeg"
}

if (-not (Test-Path (Join-Path $toolRoot "whisper-cli.exe"))) {
    $whisperRelease = Get-LatestRelease "ggml-org/whisper.cpp"
    $whisperAsset = $whisperRelease.assets |
        Where-Object { $_.name -eq "whisper-bin-x64.zip" } |
        Select-Object -First 1
    if (-not $whisperAsset) {
        throw "没有找到 whisper.cpp Windows x64 版本。"
    }

    $whisperZip = Join-Path $downloadRoot "whisper.zip"
    Download-File $whisperAsset.browser_download_url $whisperZip "whisper.cpp"
    $whisperExtract = Join-Path $extractRoot "whisper"
    Remove-Item -LiteralPath $whisperExtract -Recurse -Force -ErrorAction SilentlyContinue
    Expand-Archive -LiteralPath $whisperZip -DestinationPath $whisperExtract -Force

    $whisperExe = Get-ChildItem $whisperExtract -Recurse -Filter "whisper-cli.exe" | Select-Object -First 1
    if (-not $whisperExe) {
        throw "whisper.cpp 压缩包中缺少 whisper-cli.exe。"
    }
    Get-ChildItem $whisperExe.DirectoryName -File | Copy-Item -Destination $toolRoot -Force
}
else {
    Write-Host "[已有] whisper.cpp"
}

Download-File `
    "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small-q5_1.bin" `
    (Join-Path $modelRoot "ggml-small-q5_1.bin") `
    "Whisper 中文转写模型"

Download-File `
    "https://huggingface.co/ggml-org/whisper-vad/resolve/main/ggml-silero-v6.2.0.bin" `
    (Join-Path $modelRoot "ggml-silero-v6.2.0.bin") `
    "Silero 语音检测模型"

Write-Host ""
& "$PSScriptRoot\check-tools.ps1"
Write-Host ""
Write-Host "工具安装完成。以后双击 START.cmd，选择 2 添加链接，再选择 3 开始处理。"
