#!/usr/bin/env bash
set -euo pipefail

APP_ID="io.github.plub845.VideoDownloaderApp"
PACKAGE_NAME="video-downloader-pro"
VERSION="${VERSION:-1.0.3}"
ARCH="amd64"
RELEASE_DATE="${RELEASE_DATE:-2026-06-21}"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGING_DIR="$PROJECT_DIR/packaging"
PUBLISH_DIR="$PACKAGING_DIR/publish"
PACKAGE_ROOT="$PACKAGING_DIR/root"
OUTPUT_DIR="$PACKAGING_DIR/dist"
INSTALL_DIR="$PACKAGE_ROOT/opt/$APP_ID"

for tool in dotnet dpkg-deb ffmpeg python3 desktop-file-validate appstreamcli; do
  command -v "$tool" >/dev/null
 done
YT_DLP_MODULE_DIR="$(python3 -c 'import pathlib, yt_dlp; print(pathlib.Path(yt_dlp.__file__).parent)')"

rm -rf "$PUBLISH_DIR" "$PACKAGE_ROOT"
mkdir -p "$INSTALL_DIR/app" "$INSTALL_DIR/tools" "$PACKAGE_ROOT/DEBIAN" \
  "$PACKAGE_ROOT/usr/bin" "$PACKAGE_ROOT/usr/share/applications" \
  "$PACKAGE_ROOT/usr/share/icons/hicolor/256x256/apps" \
  "$PACKAGE_ROOT/usr/share/metainfo" "$OUTPUT_DIR"

dotnet publish "$PROJECT_DIR/VideoDownloaderApp.Linux.csproj" -c Release -r linux-x64 \
  --self-contained true -p:PublishSingleFile=false -o "$PUBLISH_DIR"
cp -a "$PUBLISH_DIR/." "$INSTALL_DIR/app/"
cp -a "$YT_DLP_MODULE_DIR" "$INSTALL_DIR/tools/yt_dlp"
ffmpeg -hide_banner -loglevel error -i "$PROJECT_DIR/Assets/VDapp.icon.ico" -frames:v 1 \
  "$PACKAGE_ROOT/usr/share/icons/hicolor/256x256/apps/$APP_ID.png"

cat >"$PACKAGE_ROOT/DEBIAN/control" <<CONTROL
Package: $PACKAGE_NAME
Version: $VERSION
Section: video
Priority: optional
Architecture: $ARCH
Depends: ffmpeg, python3
Maintainer: plub845
Homepage: https://github.com/plub845/VideoDownloaderApp
Vcs-Browser: https://github.com/plub845/VideoDownloaderApp
Description: Graphical video and audio downloader
 Downloads public media as MP4 or MP3 using yt-dlp and FFmpeg.
CONTROL

cat >"$INSTALL_DIR/tools/yt-dlp" <<'TOOL'
#!/usr/bin/env bash
export PYTHONPATH="/opt/io.github.plub845.VideoDownloaderApp/tools${PYTHONPATH:+:$PYTHONPATH}"
exec /usr/bin/python3 -m yt_dlp "$@"
TOOL

cat >"$PACKAGE_ROOT/usr/bin/$APP_ID" <<'LAUNCHER'
#!/usr/bin/env bash
export PATH="/opt/io.github.plub845.VideoDownloaderApp/tools:$PATH"
exec /opt/io.github.plub845.VideoDownloaderApp/app/VideoDownloaderApp.Linux "$@"
LAUNCHER

cat >"$PACKAGE_ROOT/usr/share/applications/$APP_ID.desktop" <<DESKTOP
[Desktop Entry]
Type=Application
Version=1.0
Name=Video Downloader Pro
Name[th]=วิดีโอดาวน์โหลดโปร
Comment=Download video or audio as MP4 and MP3
Comment[th]=ดาวน์โหลดวิดีโอหรือเสียงเป็น MP4 และ MP3
Exec=$APP_ID
TryExec=/usr/bin/$APP_ID
Icon=$APP_ID
Terminal=false
Categories=AudioVideo;
Keywords=video;audio;download;youtube;yt-dlp;mp4;mp3;
StartupNotify=true
DESKTOP

cat >"$PACKAGE_ROOT/usr/share/metainfo/$APP_ID.metainfo.xml" <<METAINFO
<?xml version="1.0" encoding="UTF-8"?>
<component type="desktop-application">
  <id>$APP_ID</id>
  <name>Video Downloader Pro</name>
  <name xml:lang="th">วิดีโอดาวน์โหลดโปร</name>
  <summary>Download public video and audio as MP4 or MP3</summary>
  <summary xml:lang="th">ดาวน์โหลดวิดีโอและเสียงสาธารณะเป็น MP4 หรือ MP3</summary>
  <metadata_license>CC0-1.0</metadata_license>
  <project_license>GPL-3.0-or-later</project_license>
  <description><p>A graphical downloader powered by yt-dlp and FFmpeg. It supports MP4 video, MP3 audio, custom yt-dlp options, download progress, and cancellation.</p></description>
  <launchable type="desktop-id">$APP_ID.desktop</launchable>
  <icon type="stock">$APP_ID</icon>
  <categories><category>AudioVideo</category></categories>
  <provides><binary>$APP_ID</binary></provides>
  <developer id="io.github.plub845"><name>plub845</name></developer>
  <url type="homepage">https://github.com/plub845/VideoDownloaderApp</url>
  <url type="bugtracker">https://github.com/plub845/VideoDownloaderApp/issues</url>
  <releases>
    <release version="$VERSION" date="$RELEASE_DATE"><description><p>Added complete Linux desktop integration, application metadata, bundled runtime, and yt-dlp.</p></description></release>
  </releases>
  <content_rating type="oars-1.1"/>
</component>
METAINFO

cat >"$PACKAGE_ROOT/DEBIAN/postinst" <<'POSTINST'
#!/bin/sh
set -e
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database -q /usr/share/applications || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor || true
exit 0
POSTINST

cat >"$PACKAGE_ROOT/DEBIAN/postrm" <<'POSTRM'
#!/bin/sh
set -e
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database -q /usr/share/applications || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor || true
exit 0
POSTRM

find "$PACKAGE_ROOT" -type d -exec chmod 0755 {} +
find "$PACKAGE_ROOT" -type f -exec chmod 0644 {} +
chmod 0755 "$INSTALL_DIR/app/VideoDownloaderApp.Linux" "$INSTALL_DIR/tools/yt-dlp" \
  "$PACKAGE_ROOT/usr/bin/$APP_ID" "$PACKAGE_ROOT/DEBIAN/postinst" "$PACKAGE_ROOT/DEBIAN/postrm"

desktop-file-validate "$PACKAGE_ROOT/usr/share/applications/$APP_ID.desktop"
(cd "$PACKAGE_ROOT" && find opt usr -type f -print0 | sort -z | xargs -0 md5sum > DEBIAN/md5sums)
chmod 0644 "$PACKAGE_ROOT/DEBIAN/md5sums"
appstreamcli validate --no-net "$PACKAGE_ROOT/usr/share/metainfo/$APP_ID.metainfo.xml"
dpkg-deb --root-owner-group --build "$PACKAGE_ROOT" "$OUTPUT_DIR/${PACKAGE_NAME}_${VERSION}_${ARCH}.deb"
echo "$OUTPUT_DIR/${PACKAGE_NAME}_${VERSION}_${ARCH}.deb"
