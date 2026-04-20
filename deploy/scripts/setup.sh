#!/usr/bin/env bash
# ============================================================
# 684 Bak-O-Meter — first-boot / post-install setup
# This script runs OUTSIDE of dpkg, so it can safely call apt.
# Idempotent — safe to re-run at any time.
# ============================================================
set -euo pipefail

APP_USER="arcade"
APP_GROUP="arcade"
APP_HOME="/opt/684bakometer"
ENV_FILE="/etc/684bakometer/app.env"
DB_NAME="684bakometer"
DB_USER="684bakometer"
DB_CRED_FILE="/etc/684bakometer/.db_password"
SETUP_MARKER="/etc/684bakometer/.setup_done"

echo "============================================================"
echo " 684 Bak-O-Meter — system setup"
echo "============================================================"

# ============================================================
# 1. Microsoft package feed & .NET 10 + ASP.NET Core Runtime
# ============================================================
install_dotnet() {
    DEBIAN_VERSION=$(. /etc/os-release && echo "${VERSION_ID}")

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

    echo ">>> Ensuring dotnet-runtime-10.0 and aspnetcore-runtime-10.0 are installed..."
    apt-get install -y -qq dotnet-runtime-10.0 aspnetcore-runtime-10.0
}
install_dotnet

# ============================================================
# 2. PostgreSQL setup (idempotent)
# ============================================================
echo ">>> Ensuring PostgreSQL is installed and running..."
apt-get install -y -qq postgresql
systemctl enable --now postgresql

generate_password() {
    tr -dc 'A-Za-z0-9' </dev/urandom | head -c 32 || true
}

if [ -f "${DB_CRED_FILE}" ]; then
    DB_PASS=$(cat "${DB_CRED_FILE}")
else
    DB_PASS=$(generate_password)
    install -m 0600 /dev/null "${DB_CRED_FILE}"
    echo "${DB_PASS}" > "${DB_CRED_FILE}"
    chown root:root "${DB_CRED_FILE}"
fi

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

if ! su - postgres -c "psql -lqt" | cut -d'|' -f1 | grep -qw "${DB_NAME}"; then
    su - postgres -c "createdb -O '${DB_USER}' '${DB_NAME}'"
    echo ">>> Database '${DB_NAME}' created."
else
    echo ">>> Database '${DB_NAME}' already exists."
fi

# ============================================================
# 3. Environment file
# ============================================================
CONNECTION_STRING="Host=localhost;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}"

cat > "${ENV_FILE}" <<EOF
# 684 Bak-O-Meter environment — managed by installer
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
EOF
chmod 0640 "${ENV_FILE}"
chown root:"${APP_GROUP}" "${ENV_FILE}"

# ============================================================
# 4. Run EF Core migration bundle if present
# ============================================================
MIGRATION_BUNDLE="${APP_HOME}/current/efbundle"
if [ -x "${MIGRATION_BUNDLE}" ]; then
    echo ">>> Running EF Core migration bundle..."
    "${MIGRATION_BUNDLE}" --connection "${CONNECTION_STRING}" || {
        echo "WARNING: Migration bundle failed. The app may apply migrations on startup."
    }
fi

# ============================================================
# 5. Permissions
# ============================================================
chown -R "${APP_USER}:${APP_GROUP}" "${APP_HOME}"
chown root:root "${DB_CRED_FILE}"

# ============================================================
# 6. Start the service
# ============================================================
systemctl daemon-reload
systemctl enable 684bakometer.service

if systemctl is-active --quiet 684bakometer.service; then
    systemctl restart 684bakometer.service
else
    systemctl start 684bakometer.service
fi

# Mark setup as complete
touch "${SETUP_MARKER}"

echo "============================================================"
echo " 684 Bak-O-Meter — setup complete!"
echo "   Service: systemctl status 684bakometer"
echo "   Env:     ${ENV_FILE}"
echo "============================================================"
