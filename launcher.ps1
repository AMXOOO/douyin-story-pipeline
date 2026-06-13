param(
    [switch]$CheckOnly
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if (-not (Test-Path -LiteralPath ".\urls.txt")) {
    Copy-Item ".\urls.example.txt" ".\urls.txt"
}

if ($CheckOnly) {
    & ".\check-tools.ps1"
    exit $LASTEXITCODE
}

function Pause-Menu {
    Write-Host ""
    Read-Host "按回车键返回主菜单"
}

while ($true) {
    Clear-Host
    Write-Host "========================================"
    Write-Host " 多平台故事素材流水线"
    Write-Host "========================================"
    Write-Host ""
    Write-Host "1. 首次安装或修复工具"
    Write-Host "2. 编辑视频链接"
    Write-Host "3. 开始下载、转写并生成故事卡"
    Write-Host "4. 打开下载的视频"
    Write-Host "5. 打开故事卡"
    Write-Host "6. 检查运行环境"
    Write-Host "0. 退出"
    Write-Host ""

    $choice = Read-Host "请输入数字"
    if ([string]::IsNullOrWhiteSpace($choice) -and [Console]::IsInputRedirected) {
        exit 0
    }

    switch ($choice) {
        "1" {
            & ".\setup-tools.ps1"
            Pause-Menu
        }
        "2" {
            Start-Process notepad.exe -ArgumentList (Resolve-Path ".\urls.txt").Path
            Write-Host "链接清单已经用记事本打开。每行放一个公开视频链接，保存后关闭记事本。"
            Write-Host "支持范围取决于 yt-dlp，例如抖音、TikTok、X/Twitter、小红书、B站、YouTube 等公开链接。"
            Pause-Menu
        }
        "3" {
            & ".\check-tools.ps1"
            & ".\run.ps1"
            Write-Host ""
            Write-Host "处理完成。故事卡位于 data\cards，视频位于 data\raw。"
            Pause-Menu
        }
        "4" {
            New-Item -ItemType Directory -Force ".\data\raw" | Out-Null
            Start-Process explorer.exe -ArgumentList (Resolve-Path ".\data\raw").Path
        }
        "5" {
            New-Item -ItemType Directory -Force ".\data\cards" | Out-Null
            Start-Process explorer.exe -ArgumentList (Resolve-Path ".\data\cards").Path
        }
        "6" {
            & ".\check-tools.ps1"
            Pause-Menu
        }
        "0" {
            exit 0
        }
        default {
            Write-Host "请输入菜单中已有的数字。"
            Start-Sleep -Seconds 1
        }
    }
}
