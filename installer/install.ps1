# ==============================================================================
# Video Downloader Pro - Windows Installer Script (PowerShell)
# Automatically downloads and launches the latest installer from GitHub Releases
# Usage:
#   irm https://github.com/plub845/VideoDownloaderApp/releases/latest/download/install.ps1 | iex
# ==============================================================================

[CmdletBinding()]
param (
    [switch]$Silent
)

$ErrorActionPreference = 'Stop'
$repo = "plub845/VideoDownloaderApp"

Write-Host ""
Write-Host " ================================================= " -ForegroundColor DarkYellow
Write-Host "   Video Downloader Pro - Windows Setup Installer   " -ForegroundColor Yellow
Write-Host " ================================================= " -ForegroundColor DarkYellow
Write-Host ""

# 1. Check latest release from GitHub API
Write-Host "[1/3] Checking for latest release from GitHub..." -ForegroundColor Cyan

$downloadUrl = $null
$versionTag = "latest"

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13

    $apiUrl = "https://api.github.com/repos/$repo/releases/latest"
    $release = Invoke-RestMethod -Uri $apiUrl -Headers @{ "User-Agent" = "VideoDownloaderApp-Installer" }
    $versionTag = $release.tag_name

    # Find VideoDownloaderApp installer in Release Assets (exclude dotnet-sdk)
    $exeAsset = $release.assets | Where-Object { 
        ($_.name -like "*VideoDownloaderApp*.exe" -or $_.name -like "*Setup*.exe") -and 
        $_.name -notlike "*dotnet*" 
    } | Select-Object -First 1
    if ($exeAsset) {
        $downloadUrl = $exeAsset.browser_download_url
        Write-Host "  -> Found version: $versionTag ($($exeAsset.name))" -ForegroundColor Green
    }
}
catch {
    Write-Host "  -> Could not query GitHub API (rate limit or offline), using direct fallback..." -ForegroundColor Yellow
}

# Fallback direct download link if GitHub API is unreachable
if (-not $downloadUrl) {
    try {
        $req = [System.Net.HttpWebRequest]::Create("https://github.com/$repo/releases/latest")
        $req.AllowAutoRedirect = $true
        $req.UserAgent = "VideoDownloaderApp-Installer"
        $resp = $req.GetResponse()
        $finalUri = $resp.ResponseUri.AbsoluteUri
        $resp.Close()
        if ($finalUri -match '/releases/tag/([^/]+)') {
            $tag = $matches[1]
            $downloadUrl = "https://github.com/$repo/releases/download/$tag/VideoDownloaderApp_Setup_${tag}.exe"
        }
    } catch { }

    if (-not $downloadUrl) {
        $downloadUrl = "https://github.com/$repo/releases/latest/download/VideoDownloaderApp_Setup_v1.1.0.exe"
    }
    Write-Host "  -> Fallback download URL: $downloadUrl" -ForegroundColor Gray
}

# 2. Download installer executable
$tempDir = [System.IO.Path]::GetTempPath()
$tempExe = Join-Path $tempDir "VideoDownloaderApp_Setup.exe"

if (Test-Path $tempExe) {
    Remove-Item $tempExe -Force -ErrorAction SilentlyContinue
}

Write-Host "[2/3] Downloading installer..." -ForegroundColor Cyan

try {
    $wc = New-Object System.Net.WebClient
    $wc.Headers.Add("User-Agent", "VideoDownloaderApp-Installer")
    $wc.DownloadFile($downloadUrl, $tempExe)
    Write-Host "  -> Download completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "  [Error] Failed to download installer: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Launch installer
Write-Host "[3/3] Launching Video Downloader Pro installer..." -ForegroundColor Cyan

$processArgs = @{
    FilePath = $tempExe
    Wait     = $true
}

if ($Silent) {
    $processArgs.ArgumentList = "/VERYSILENT /NORESTART"
    Write-Host "  -> Running in silent background mode..." -ForegroundColor Yellow
}

try {
    Start-Process @processArgs
    Write-Host ""
    Write-Host " ================================================= " -ForegroundColor DarkGreen
    Write-Host "   Installation completed! Ready to use.            " -ForegroundColor Green
    Write-Host " ================================================= " -ForegroundColor DarkGreen
}
finally {
    # Remove temporary installer file
    if (Test-Path $tempExe) {
        Remove-Item $tempExe -Force -ErrorAction SilentlyContinue
    }
}
