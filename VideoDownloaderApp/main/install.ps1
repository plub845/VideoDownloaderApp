$ErrorActionPreference = "Stop"

# VideoDownloaderApp PowerShell Launcher
# Downloads the official installer from GitHub Releases and runs it.

$InstallerUrl = "https://github.com/plub845/VideoDownloaderApp/releases/download/v1.0.0/VideoDownloaderApp_Installer.exe"
$InstallerPath = Join-Path $env:TEMP "VideoDownloaderApp_Installer.exe"

Write-Host "========================================"
Write-Host " VideoDownloaderApp Installer Launcher"
Write-Host " Publisher: plub845"
Write-Host "========================================"
Write-Host ""

function Test-IsAdmin {
    $currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdmin)) {
    Write-Host "[ADMIN] Restarting this script as administrator..."
    Start-Process powershell.exe -Verb RunAs -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`""
    exit
}

Write-Host "[DOWNLOAD] Downloading installer..."
Write-Host "[URL] $InstallerUrl"
Write-Host "[SAVE] $InstallerPath"

Invoke-WebRequest -Uri $InstallerUrl -OutFile $InstallerPath -UseBasicParsing

if (!(Test-Path $InstallerPath)) {
    throw "Installer download failed."
}

if ((Get-Item $InstallerPath).Length -le 0) {
    throw "Installer file is empty."
}

Write-Host ""
Write-Host "[RUN] Starting installer as administrator..."
Start-Process -FilePath $InstallerPath -Verb RunAs -Wait

Write-Host ""
Write-Host "Done."
