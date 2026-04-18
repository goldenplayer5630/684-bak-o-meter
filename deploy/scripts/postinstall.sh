#!/usr/bin/env bash
set -euo pipefail

APP_USER="arcade"
APP_GROUP="arcade"
APP_HOME="/opt/684bakometer"
ENV_FILE="/etc/684bakometer/app.env"
DB_NAME="684bakometer"
DB_USER="684bakometer"
DB_CRED_FILE="/etc/684bakometer/.db_password"

# ============================================================
# 1. System user & group
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
# 2. Microsoft package feed & .NET 10 + ASP.NET Core Runtime
# ============================================================
install_dotnet() {
    # Determine Debian version
    DEBIAN_VERSION=$(. /etc/os-release && echo "${VERSION_ID}")

    # Install Microsoft package signing key & feed
    local pkg_url="https://packages.microsoft.com/config/debian/${DEBIAN_VERSION}/packages-microsoft-prod.deb"
    local tmp_deb
    tmp_deb=$(mktemp /tmp/ms-prod-XXXXXX.deb)

    if ! dpkg -s packages-microsoft-prod >/dev/null 2>&1; then
        echo ">>> Installing Microsoft package feed for Debian ${DEBIAN_VERSION}..."
        curl -fsSL "${pkg_url}" -o "${tmp_deb}"
        dpkg -i "${tmp_deb}"
        rm -f "${tmp_deb}"
        apt-get update -qq
    fi

    # Install runtimes (idempotent via apt-get)
    echo ">>> Ensuring dotnet-runtime-10.0 and aspnetcore-runtime-10.0 are installed..."
    apt-get install -y -qq dotnet-runtime-10.0 aspnetcore-runtime-10.0
}
install_dotnet

# ============================================================
# 3. PostgreSQL setup (idempotent)
# ============================================================
echo ">>> Ensuring PostgreSQL is running..."
systemctl enable --now postgresql

generate_password() {
    tr -dc 'A-Za-z0-9' </dev/urandom | head -c 32 || true
}

# Generate or read existing password
if [ -f "${DB_CRED_FILE}" ]; then
    DB_PASS=$(cat "${DB_CRED_FILE}")
else
    DB_PASS=$(generate_password)
    install -m 0600 /dev/null "${DB_CRED_FILE}"
    echo "${DB_PASS}" > "${DB_CRED_FILE}"
    chown root:root "${DB_CRED_FILE}"
fi

# Create role if missing, update password always
su - postgres -c "psql -v ON_ERROR_STOP=1" <<EOSQL || true
DO \$\$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '${DB_USER}') THEN
        CREATE ROLE "${DB_USER}" LOGIN PASSWORD '${DB_PASS}';
    ELSE
        ALTER ROLE "${DB_USER}" PASSWORD '${DB_PASS}';
    END IF;
END
\$\$;
EOSQL

# Create database if missing
if ! su - postgres -c "psql -lqt" | cut -d'|' -f1 | grep -qw "${DB_NAME}"; then
    su - postgres -c "createdb -O '${DB_USER}' '${DB_NAME}'"
    echo ">>> Database '${DB_NAME}' created."
else
    echo ">>> Database '${DB_NAME}' already exists."
fi

# ============================================================
# 4. Environment file
# ============================================================
CONNECTION_STRING="Host=localhost;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}"

# Write env file (overwrite connection string each time for password sync)
cat > "${ENV_FILE}" <<EOF
# 684 Bak-O-Meter environment — managed by installer
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
EOF
chmod 0640 "${ENV_FILE}"
chown root:"${APP_GROUP}" "${ENV_FILE}"

# ============================================================
# 5. Seed initial release from package contents
# ============================================================
PACKAGE_DIR="${APP_HOME}/package"
if [ -d "${PACKAGE_DIR}" ] && [ -n "$(ls -A "${PACKAGE_DIR}" 2>/dev/null)" ]; then
    # Detect version from the deb metadata or fallback
    PKG_VERSION=$(dpkg-query -W -f='${Version}' 684bakometer 2>/dev/null || echo "0.0.0")
    TAG="v${PKG_VERSION}"
    RELEASE_DIR="${APP_HOME}/releases/${TAG}"

    if [ ! -d "${RELEASE_DIR}" ]; then
        mkdir -p "${RELEASE_DIR}"
        cp -a "${PACKAGE_DIR}/." "${RELEASE_DIR}/"
        echo ">>> Seeded release ${TAG} from package."
    fi

    # Point current symlink
    ln -sfn "${RELEASE_DIR}" "${APP_HOME}/current"

    # Persist version
    echo "${TAG}" > "${APP_HOME}/.installed_version"
fi

# ============================================================
# 6. Run EF Core migration bundle if present
# ============================================================
MIGRATION_BUNDLE="${APP_HOME}/current/efbundle"
if [ -x "${MIGRATION_BUNDLE}" ]; then
    echo ">>> Running EF Core migration bundle..."
    "${MIGRATION_BUNDLE}" --connection "${CONNECTION_STRING}" || {
        echo "WARNING: Migration bundle failed. The app may apply migrations on startup."
    }
fi

# ============================================================
# 7. Permissions
# ============================================================
chown -R "${APP_USER}:${APP_GROUP}" "${APP_HOME}"
# Keep cred file owned by root
chown root:root "${DB_CRED_FILE}"

# ============================================================
# 8. systemd
# ============================================================
systemctl daemon-reload
systemctl enable 684bakometer.service

# Start or restart the service on install/upgrade
if systemctl is-active --quiet 684bakometer.service; then
    systemctl restart 684bakometer.service
else
    systemctl start 684bakometer.service
fi

echo ">>> 684 Bak-O-Meter installed successfully."
echo "    Service: systemctl status 684bakometer"
echo "    Env:     ${ENV_FILE}"
