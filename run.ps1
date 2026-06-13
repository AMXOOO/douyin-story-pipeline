$ErrorActionPreference = "Stop"

& "$PSScriptRoot\download.ps1"
& "$PSScriptRoot\transcribe.ps1"
& "$PSScriptRoot\build-queue.ps1"

Write-Host "Pipeline complete. Ask Codex to analyze data\cards."
