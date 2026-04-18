#!/usr/bin/env bash
set -euo pipefail

APP_USER="arcade"
APP_GROUP="arcade"
APP_HOME="/opt/684bakometer"

# ============================================================
# 1. System user & group (no dpkg lock needed)
# ============================================================
if ! getent group "${APP_GROUP}" >/dev/null 2>&1; then
    groupadd --system "${APP_GROUP}"
fi

if ! id -u "${APP_USER}" >/dev/null 2>&1; then
    useradd --system \
        --gid "${APP_GROUP}" \
        --home-dir "${APP_HOME}" \
        --shell /usr/sbin/nologin \
        --no-create-home \
        "${APP_USER}"
fi

# Serial device access
usermod -aG dialout "${APP_USER}" 2>/dev/null || true

# ============================================================
# 2. Seed initial release from package contents (no lock needed)
# ============================================================
PACKAGE_DIR="${APP_HOME}/package"
if [ -d "${PACKAGE_DIR}" ] && [ -n "$(ls -A "${PACKAGE_DIR}" 2>/dev/null)" ]; then
    PKG_VERSION=$(dpkg-query -W -f='${Version}' 684bakometer 2>/dev/null || echo "0.0.0")
    TAG="v${PKG_VERSION}"
    RELEASE_DIR="${APP_HOME}/releases/${TAG}"

    if [ ! -d "${RELEASE_DIR}" ]; then
        mkdir -p "${RELEASE_DIR}"
        cp -a "${PACKAGE_DIR}/." "${RELEASE_DIR}/"
        echo ">>> Seeded release ${TAG} from package."
    fi

    ln -sfn "${RELEASE_DIR}" "${APP_HOME}/current"
    echo "${TAG}" > "${APP_HOME}/.installed_version"
fi

# ============================================================
# 3. Basic permissions (no lock needed)
# ============================================================
chown -R "${APP_USER}:${APP_GROUP}" "${APP_HOME}"

# ============================================================
# 4. Schedule the full setup to run AFTER dpkg releases its lock.
#    This is needed because apt-get cannot run inside a dpkg
#    maintainer script (the lock is already held).
# ============================================================
SETUP_SCRIPT="${APP_HOME}/package/setup.sh"
if [ ! -x "${SETUP_SCRIPT}" ]; then
    SETUP_SCRIPT="/opt/684bakometer/684bakometer-setup.sh"
fi

echo ""
echo "============================================================"
echo " Package files installed."
echo ""
echo " To complete setup (install .NET, PostgreSQL, start service):"
echo ""
echo "   sudo /opt/684bakometer/684bakometer-setup.sh"
echo ""
echo " This step must run OUTSIDE of apt/dpkg."
echo "============================================================"
echo ""

# Attempt to run setup automatically in the background after dpkg exits.
# Uses a polling loop that waits for the dpkg lock to be released.
nohup bash -c '
    # Wait up to 120 seconds for dpkg lock to be released
    for i in $(seq 1 120); do
        if flock --nonblock 9 9</var/lib/dpkg/lock-frontend 2>/dev/null; then
            exec 9>&-
            break
        fi
        sleep 1
    done
    /opt/684bakometer/684bakometer-setup.sh
' >> /var/log/684bakometer-setup.log 2>&1 &

echo ">>> Background setup scheduled. Monitor with: tail -f /var/log/684bakometer-setup.log"
