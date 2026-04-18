#!/usr/bin/env bash
# ============================================================
# 684 Bak-O-Meter — GitHub Release Updater
# Safe to run repeatedly. Intended to run as ExecStartPre.
# ============================================================
set -euo pipefail

REPO="goldenplayer5630/684-bak-o-meter"
API_URL="https://api.github.com/repos/${REPO}/releases/latest"
APP_HOME="/opt/684bakometer"
DOWNLOADS="${APP_HOME}/downloads"
RELEASES="${APP_HOME}/releases"
CURRENT_LINK="${APP_HOME}/current"
VERSION_FILE="${APP_HOME}/.installed_version"
ENV_FILE="/etc/684bakometer/app.env"

ASSET_NAME="684BakOMeter-linux-x64.tar.gz"
CHECKSUM_NAME="${ASSET_NAME}.sha256"

mkdir -p "${DOWNLOADS}" "${RELEASES}"

# ============================================================
# Determine latest release tag
# ============================================================
echo ">>> Checking latest release from GitHub..."
LATEST_JSON=$(curl -fsSL "${API_URL}" 2>/dev/null) || {
    echo "WARNING: Could not reach GitHub API. Skipping update."
    exit 0
}

LATEST_TAG=$(echo "${LATEST_JSON}" | jq -r '.tag_name // empty')
if [ -z "${LATEST_TAG}" ]; then
    echo "WARNING: Could not parse latest tag. Skipping update."
    exit 0
fi

# ============================================================
# Compare with installed version
# ============================================================
INSTALLED_TAG=""
if [ -f "${VERSION_FILE}" ]; then
    INSTALLED_TAG=$(cat "${VERSION_FILE}")
fi

if [ "${LATEST_TAG}" = "${INSTALLED_TAG}" ]; then
    echo ">>> Already on latest version ${LATEST_TAG}. No update needed."
    exit 0
fi

echo ">>> Updating from ${INSTALLED_TAG:-<none>} to ${LATEST_TAG}..."

# ============================================================
# Download asset
# ============================================================
ASSET_URL=$(echo "${LATEST_JSON}" | jq -r ".assets[] | select(.name == \"${ASSET_NAME}\") | .browser_download_url // empty")
if [ -z "${ASSET_URL}" ]; then
    echo "ERROR: Asset ${ASSET_NAME} not found in release ${LATEST_TAG}."
    exit 1
fi

DOWNLOAD_PATH="${DOWNLOADS}/${LATEST_TAG}-${ASSET_NAME}"
echo ">>> Downloading ${ASSET_URL}..."
curl -fsSL -o "${DOWNLOAD_PATH}" "${ASSET_URL}"

# ============================================================
# Verify checksum (if available)
# ============================================================
CHECKSUM_URL=$(echo "${LATEST_JSON}" | jq -r ".assets[] | select(.name == \"${CHECKSUM_NAME}\") | .browser_download_url // empty")
if [ -n "${CHECKSUM_URL}" ]; then
    echo ">>> Verifying checksum..."
    EXPECTED=$(curl -fsSL "${CHECKSUM_URL}" | awk '{print $1}')
    ACTUAL=$(sha256sum "${DOWNLOAD_PATH}" | awk '{print $1}')
    if [ "${EXPECTED}" != "${ACTUAL}" ]; then
        echo "ERROR: Checksum mismatch!"
        echo "  Expected: ${EXPECTED}"
        echo "  Actual:   ${ACTUAL}"
        rm -f "${DOWNLOAD_PATH}"
        exit 1
    fi
    echo ">>> Checksum OK."
else
    echo ">>> No checksum asset found. Skipping verification."
fi

# ============================================================
# Extract into releases/<tag>
# ============================================================
RELEASE_DIR="${RELEASES}/${LATEST_TAG}"
rm -rf "${RELEASE_DIR}"
mkdir -p "${RELEASE_DIR}"
tar -xzf "${DOWNLOAD_PATH}" -C "${RELEASE_DIR}"

# Make main binary executable
chmod +x "${RELEASE_DIR}/684BakOMeter.Web" 2>/dev/null || true

# Make migration bundle executable if present
chmod +x "${RELEASE_DIR}/efbundle" 2>/dev/null || true

# ============================================================
# Run migration bundle if present
# ============================================================
if [ -x "${RELEASE_DIR}/efbundle" ] && [ -f "${ENV_FILE}" ]; then
    # Extract connection string from env file
    CONN_STR=$(grep '^ConnectionStrings__DefaultConnection=' "${ENV_FILE}" | cut -d'=' -f2-)
    if [ -n "${CONN_STR}" ]; then
        echo ">>> Running EF Core migration bundle..."
        "${RELEASE_DIR}/efbundle" --connection "${CONN_STR}" || {
            echo "WARNING: Migration bundle failed. App may self-migrate on startup."
        }
    fi
fi

# ============================================================
# Switch symlink
# ============================================================
ln -sfn "${RELEASE_DIR}" "${CURRENT_LINK}"
echo "${LATEST_TAG}" > "${VERSION_FILE}"

# Clean up old download
rm -f "${DOWNLOAD_PATH}"

echo ">>> Update to ${LATEST_TAG} complete."
