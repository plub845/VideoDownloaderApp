#!/usr/bin/env bash
# ==============================================================================
# Video Downloader Pro - Linux Installer Script (Bash)
# Automatically fetches and installs the latest .deb package from GitHub Releases
# Usage:
#   curl -fsSL https://github.com/plub845/VideoDownloaderApp/releases/latest/download/install.sh | bash
# ==============================================================================

set -e

REPO="plub845/VideoDownloaderApp"
APP_NAME="Video Downloader Pro"
APP_ID="io.github.plub845.VideoDownloaderApp"
BIN_LINK="/usr/local/bin/video-downloader"

# Terminal Color Styles
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

echo -e ""
echo -e "${YELLOW}=================================================${NC}"
echo -e "${BOLD}${YELLOW}   Video Downloader Pro - Linux Installer        ${NC}"
echo -e "${YELLOW}=================================================${NC}"
echo -e ""

# 1. Check root / sudo privileges
SUDO=""
if [ "$EUID" -ne 0 ]; then
    if command -v sudo >/dev/null 2>&1; then
        SUDO="sudo"
        echo -e "${CYAN}[*] Root permissions required for installation. Running via sudo...${NC}"
    else
        echo -e "${RED}[!] Please run as root or install sudo before running this script.${NC}"
        exit 1
    fi
fi

# 2. Check architecture
ARCH=$(uname -m)
if [ "$ARCH" != "x86_64" ] && [ "$ARCH" != "amd64" ]; then
    echo -e "${YELLOW}[!] Warning: Current system architecture is '$ARCH'. Pre-built packages are built for x86_64 / amd64.${NC}"
fi

# 3. Check required tools (curl)
echo -e "${CYAN}[1/4] Checking prerequisites...${NC}"
if ! command -v curl >/dev/null 2>&1; then
    echo -e "${YELLOW}[*] 'curl' not found. Installing...${NC}"
    if command -v apt-get >/dev/null 2>&1; then
        $SUDO apt-get update -qq && $SUDO apt-get install -y curl
    elif command -v dnf >/dev/null 2>&1; then
        $SUDO dnf install -y curl
    elif command -v pacman >/dev/null 2>&1; then
        $SUDO pacman -Sy --noconfirm curl
    fi
fi

# Check for dpkg / apt support
if ! command -v dpkg >/dev/null 2>&1 && ! command -v apt-get >/dev/null 2>&1; then
    echo -e "${RED}[!] This installer requires a Debian-based Linux distribution (with dpkg/apt).${NC}"
    echo -e "${YELLOW}For other distributions, please build from source using VideoDownloaderApp.Linux.${NC}"
    exit 1
fi
echo -e "${GREEN}  -> Prerequisites verified.${NC}"

# 4. Fetch latest release information from GitHub API
echo -e "${CYAN}[2/4] Finding latest .deb package from GitHub Releases (${REPO})...${NC}"

DEB_URL=""
TAG_NAME="latest"

RELEASE_JSON=$(curl -fsSL -H "User-Agent: VideoDownloaderApp-Installer" \
    "https://api.github.com/repos/${REPO}/releases/latest" 2>/dev/null || true)

if [ -n "$RELEASE_JSON" ]; then
    # Extract tag name
    if command -v python3 >/dev/null 2>&1; then
        TAG_NAME=$(python3 -c '
import sys, json
try:
    data = json.loads(sys.stdin.read())
    print(data.get("tag_name", "latest"))
except:
    print("latest")
' <<< "$RELEASE_JSON" 2>/dev/null || echo "latest")
    else
        TAG_NAME=$(echo "$RELEASE_JSON" | grep -o '"tag_name": *"[^"]*"' | head -n 1 | cut -d'"' -f4 || echo "latest")
    fi

    # Extract browser_download_url for .deb package
    if command -v python3 >/dev/null 2>&1; then
        DEB_URL=$(python3 -c '
import sys, json
try:
    data = json.loads(sys.stdin.read())
    for a in data.get("assets", []):
        url = a.get("browser_download_url", "")
        if url.endswith(".deb"):
            print(url)
            break
except:
    pass
' <<< "$RELEASE_JSON" 2>/dev/null || true)
    fi

    if [ -z "$DEB_URL" ]; then
        DEB_URL=$(echo "$RELEASE_JSON" | grep -o 'https://[^"[:space:]]*\.deb' | head -n 1 || true)
    fi
fi

# Fallback: If GitHub API was rate limited or failed, resolve latest release redirect
if [ -z "$DEB_URL" ]; then
    echo -e "${YELLOW}  -> GitHub API unavailable or rate-limited, using direct latest release download URL...${NC}"
    LATEST_REDIRECT=$(curl -sIL -o /dev/null -w "%{url_effective}" "https://github.com/${REPO}/releases/latest" 2>/dev/null || true)
    if [[ "$LATEST_REDIRECT" =~ /releases/tag/([^/]+) ]]; then
        TAG_NAME="${BASH_REMATCH[1]}"
        CLEAN_VER="${TAG_NAME#v}"
        DEB_URL="https://github.com/${REPO}/releases/download/${TAG_NAME}/video-downloader-pro_${CLEAN_VER}_amd64.deb"
    else
        DEB_URL="https://github.com/${REPO}/releases/latest/download/video-downloader-pro_1.1.0_amd64.deb"
    fi
fi

echo -e "${GREEN}  -> Target version: ${BOLD}${TAG_NAME}${NC}"
echo -e "${CYAN}  -> Download URL: ${DEB_URL}${NC}"

# 5. Download .deb package
TMP_DEB=$(mktemp --suffix=.deb 2>/dev/null || mktemp /tmp/vdown.XXXXXX.deb)
trap 'rm -f "$TMP_DEB"' EXIT

echo -e "${CYAN}[3/4] Downloading package...${NC}"
if ! curl -fSL "$DEB_URL" -o "$TMP_DEB" --progress-bar; then
    echo -e "${RED}[!] Failed to download .deb package from ${DEB_URL}.${NC}"
    echo -e "${YELLOW}Please check your internet connection or download manually from:${NC}"
    echo -e "    https://github.com/${REPO}/releases"
    exit 1
fi
echo -e "${GREEN}  -> Package downloaded successfully.${NC}"

# 6. Install the .deb package
echo -e "${CYAN}[4/4] Installing package and configuring application...${NC}"

if command -v apt-get >/dev/null 2>&1; then
    # apt-get automatically resolves and installs dependencies (ffmpeg, python3)
    $SUDO apt-get update -qq || true
    $SUDO apt-get install -y "$TMP_DEB"
elif command -v dpkg >/dev/null 2>&1; then
    $SUDO dpkg -i "$TMP_DEB" || {
        echo -e "${YELLOW}[*] Resolving missing dependencies...${NC}"
        if command -v apt-get >/dev/null 2>&1; then
            $SUDO apt-get install -f -y
        fi
    }
fi

# Ensure /usr/local/bin/video-downloader symlink exists for terminal access
if [ -f "/usr/bin/${APP_ID}" ]; then
    $SUDO ln -sf "/usr/bin/${APP_ID}" "${BIN_LINK}"
fi

# Refresh desktop database & icon cache
if command -v update-desktop-database >/dev/null 2>&1; then
    $SUDO update-desktop-database -q /usr/share/applications 2>/dev/null || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    $SUDO gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor 2>/dev/null || true
fi

echo -e ""
echo -e "${GREEN}=================================================${NC}"
echo -e "${BOLD}${GREEN}   Video Downloader Pro installed successfully!  ${NC}"
echo -e "${GREEN}=================================================${NC}"
echo -e "${CYAN}You can launch the application by:${NC}"
echo -e "  1. Running in terminal: ${BOLD}video-downloader${NC}"
echo -e "  2. Or launching from your desktop applications menu: ${BOLD}${APP_NAME}${NC}"
echo -e ""
