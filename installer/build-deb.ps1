# ==============================================================================
# Build Linux .deb Package on Windows (PowerShell)
# Outputs: installer\video-downloader-pro_1.1.0_amd64.deb
# ==============================================================================

[CmdletBinding()]
param (
    [string]$Version = "1.1.0",
    [string]$Arch = "amd64"
)

$ErrorActionPreference = "Stop"
$appId = "io.github.plub845.VideoDownloaderApp"
$pkgName = "video-downloader-pro"
$debName = "${pkgName}_${Version}_${Arch}.deb"
$repoRoot = (Get-Item $PSScriptRoot).Parent.FullName
$outputDeb = Join-Path $PSScriptRoot $debName
$publishLinux = Join-Path $repoRoot "publish\linux"
$buildTemp = Join-Path $PSScriptRoot "packaging_temp"

Write-Host ""
Write-Host "=================================================" -ForegroundColor DarkYellow
Write-Host "   Building Linux Debian Package (.deb)          " -ForegroundColor Yellow
Write-Host "   Package: $debName" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor DarkYellow
Write-Host ""

if (-not (Test-Path $publishLinux)) {
    Write-Host "[!] Error: publish\linux directory not found at: $publishLinux" -ForegroundColor Red
    exit 1
}

if (Test-Path $buildTemp) { Remove-Item -Recurse -Force $buildTemp }
New-Item -ItemType Directory -Path "$buildTemp\data\opt\$appId\app" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\data\opt\$appId\tools" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\data\usr\bin" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\data\usr\share\applications" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\data\usr\share\icons\hicolor\256x256\apps" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\data\usr\share\metainfo" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildTemp\control" -Force | Out-Null

function Write-UnixFile($path, $content) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($content.Replace("`r`n", "`n"))
    [System.IO.File]::WriteAllBytes($path, $bytes)
}

Write-Host "[1/5] Copying binaries and assets..." -ForegroundColor Cyan
Copy-Item -Path "$publishLinux\*" -Destination "$buildTemp\data\opt\$appId\app" -Recurse -Force

# Icon
$iconSrc = Join-Path $repoRoot "VideoDownloaderApp.Linux\Assets\app_icon.png"
if (-not (Test-Path $iconSrc)) {
    $ffmpegExe = Join-Path $repoRoot "VideoDownloaderApp\main\ffmpeg.exe"
    $icoSrc = Join-Path $repoRoot "VideoDownloaderApp.Linux\Assets\VDapp.icon.ico"
    if ((Test-Path $ffmpegExe) -and (Test-Path $icoSrc)) {
        & $ffmpegExe -y -i $icoSrc -frames:v 1 $iconSrc | Out-Null
    }
}
if (Test-Path $iconSrc) {
    Copy-Item $iconSrc "$buildTemp\data\usr\share\icons\hicolor\256x256\apps\$appId.png" -Force
}

Write-Host "[2/5] Writing desktop integrations and wrappers..." -ForegroundColor Cyan
Write-UnixFile "$buildTemp\data\opt\$appId\tools\yt-dlp" @"
#!/usr/bin/env bash
export PYTHONPATH="/opt/$appId/tools${PYTHONPATH:+:$PYTHONPATH}"
if command -v yt-dlp >/dev/null 2>&1; then
    exec yt-dlp "`$@"
elif python3 -m yt_dlp --version >/dev/null 2>&1; then
    exec /usr/bin/python3 -m yt_dlp "`$@"
else
    exec /usr/bin/python3 -m yt_dlp "`$@"
fi
"@

Write-UnixFile "$buildTemp\data\usr\bin\$appId" @"
#!/usr/bin/env bash
export PATH="/opt/$appId/tools:`$PATH"
exec /opt/$appId/app/VideoDownloaderApp.Linux "`$@"
"@

Write-UnixFile "$buildTemp\data\usr\bin\video-downloader" @"
#!/usr/bin/env bash
export PATH="/opt/$appId/tools:`$PATH"
exec /opt/$appId/app/VideoDownloaderApp.Linux "`$@"
"@

Write-UnixFile "$buildTemp\data\usr\share\applications\$appId.desktop" @"
[Desktop Entry]
Type=Application
Version=1.0
Name=Video Downloader Pro
Name[th]=วิดีโอดาวน์โหลดโปร
Comment=Download video or audio as MP4 and MP3
Comment[th]=ดาวน์โหลดวิดีโอหรือเสียงเป็น MP4 และ MP3
Exec=$appId
TryExec=/usr/bin/$appId
Icon=$appId
Terminal=false
Categories=AudioVideo;
Keywords=video;audio;download;youtube;yt-dlp;mp4;mp3;
StartupNotify=true
"@

Write-UnixFile "$buildTemp\data\usr\share\metainfo\$appId.metainfo.xml" @"
<?xml version="1.0" encoding="UTF-8"?>
<component type="desktop-application">
  <id>$appId</id>
  <name>Video Downloader Pro</name>
  <name xml:lang="th">วิดีโอดาวน์โหลดโปร</name>
  <summary>Download public video and audio as MP4 or MP3</summary>
  <summary xml:lang="th">ดาวน์โหลดวิดีโอและเสียงสาธารณะเป็น MP4 หรือ MP3</summary>
  <metadata_license>CC0-1.0</metadata_license>
  <project_license>GPL-3.0-or-later</project_license>
  <description><p>A graphical downloader powered by yt-dlp and FFmpeg. It supports MP4 video, MP3 audio, custom yt-dlp options, download progress, and cancellation.</p></description>
  <launchable type="desktop-id">$appId.desktop</launchable>
  <icon type="stock">$appId</icon>
  <categories><category>AudioVideo</category></categories>
  <provides><binary>$appId</binary></provides>
  <developer id="io.github.plub845"><name>plub845</name></developer>
  <url type="homepage">https://github.com/plub845/VideoDownloaderApp</url>
  <url type="bugtracker">https://github.com/plub845/VideoDownloaderApp/issues</url>
  <releases>
    <release version="$Version" date="2026-06-21"><description><p>Added complete Linux desktop integration, application metadata, bundled runtime, and yt-dlp.</p></description></release>
  </releases>
  <content_rating type="oars-1.1"/>
</component>
"@

Write-Host "[3/5] Generating package control files..." -ForegroundColor Cyan
Write-UnixFile "$buildTemp\control\control" @"
Package: $pkgName
Version: $Version
Section: video
Priority: optional
Architecture: $Arch
Depends: ffmpeg, python3
Maintainer: plub845
Homepage: https://github.com/plub845/VideoDownloaderApp
Vcs-Browser: https://github.com/plub845/VideoDownloaderApp
Description: Graphical video and audio downloader
 Downloads public media as MP4 or MP3 using yt-dlp and FFmpeg.
"@

Write-UnixFile "$buildTemp\control\postinst" @"
#!/bin/sh
set -e
chmod 0755 /opt/$appId/app/VideoDownloaderApp.Linux 2>/dev/null || true
chmod 0755 /opt/$appId/tools/yt-dlp 2>/dev/null || true
chmod 0755 /usr/bin/$appId 2>/dev/null || true
chmod 0755 /usr/bin/video-downloader 2>/dev/null || true
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database -q /usr/share/applications || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor || true
exit 0
"@

Write-UnixFile "$buildTemp\control\postrm" @"
#!/bin/sh
set -e
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database -q /usr/share/applications || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor || true
exit 0
"@

# md5sums
$md5Lines = @()
$dataRoot = (Resolve-Path "$buildTemp\data").Path
Get-ChildItem -Path "$buildTemp\data" -Recurse -File | ForEach-Object {
    $relPath = $_.FullName.Substring($dataRoot.Length + 1).Replace("\", "/")
    $hash = (Get-FileHash -Path $_.FullName -Algorithm MD5).Hash.ToLower()
    $md5Lines += "$hash  $relPath"
}
Write-UnixFile "$buildTemp\control\md5sums" (($md5Lines -join "`n") + "`n")

Write-Host "[4/5] Compressing internal archives (control.tar.gz and data.tar.gz)..." -ForegroundColor Cyan
tar.exe -czf "$buildTemp\control.tar.gz" -C "$buildTemp\control" .
tar.exe -czf "$buildTemp\data.tar.gz" -C "$buildTemp\data" .

Write-Host "[5/5] Assembling ar archive into $outputDeb..." -ForegroundColor Cyan
if (Test-Path $outputDeb) { Remove-Item -Force $outputDeb }

$fs = [System.IO.File]::Create($outputDeb)
$magic = [System.Text.Encoding]::ASCII.GetBytes("!<arch>`n")
$fs.Write($magic, 0, $magic.Length)

function Write-ArMember($stream, $name, [byte[]]$bytes) {
    $hdrStr = $name.PadRight(16) + "1719000000  " + "0     " + "0     " + "100644  " + ("$($bytes.Length)".PadRight(10))
    $hdr = [System.Text.Encoding]::ASCII.GetBytes($hdrStr)
    $stream.Write($hdr, 0, $hdr.Length)
    $stream.WriteByte(0x60)
    $stream.WriteByte(0x0A)
    $stream.Write($bytes, 0, $bytes.Length)
    if ($bytes.Length % 2 -ne 0) {
        $stream.WriteByte(0x0A)
    }
}

Write-ArMember $fs "debian-binary" ([System.Text.Encoding]::ASCII.GetBytes("2.0`n"))

$ctrlBytes = [System.IO.File]::ReadAllBytes("$buildTemp\control.tar.gz")
Write-ArMember $fs "control.tar.gz" $ctrlBytes

$dataBytes = [System.IO.File]::ReadAllBytes("$buildTemp\data.tar.gz")
Write-ArMember $fs "data.tar.gz" $dataBytes

$fs.Close()
Remove-Item -Recurse -Force $buildTemp

Write-Host ""
Write-Host "=================================================" -ForegroundColor DarkGreen
Write-Host "   Build completed successfully!                 " -ForegroundColor Green
Write-Host "   Output: $outputDeb" -ForegroundColor Green
Write-Host "   Size: $([math]::Round((Get-Item $outputDeb).Length / 1MB, 2)) MB" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor DarkGreen
Write-Host ""
