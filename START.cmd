@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\launcher.ps1"
if errorlevel 1 (
  echo.
  echo The program stopped because an error occurred.
  pause
)
