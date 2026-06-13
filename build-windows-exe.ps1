$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$source = Join-Path $PSScriptRoot "src\WindowsLauncher\Program.cs"
$output = Join-Path $PSScriptRoot "LingJiClipScribe.exe"
$compiler = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path -LiteralPath $compiler)) {
    throw "C# compiler not found: $compiler"
}

if (-not (Test-Path -LiteralPath $source)) {
    throw "Launcher source not found: $source"
}

& $compiler `
    /nologo `
    /target:winexe `
    /platform:x64 `
    /out:$output `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    $source

if ($LASTEXITCODE -ne 0) {
    throw "Failed to build Windows launcher."
}

Write-Host "Built: $output"
