#!/usr/bin/env bash
set -euo pipefail

systemctl daemon-reload

# On full purge ($1 == "purge" for deb), remove data
if [ "${1:-}" = "purge" ]; then
    echo ">>> Purging 684bakometer data..."

    # Remove app directory
    rm -rf /opt/684bakometer

    # Remove config
    rm -rf /etc/684bakometer

    # Remove system user & group
    if id -u arcade >/dev/null 2>&1; then
        userdel arcade 2>/dev/null || true
    fi
    if getent group arcade >/dev/null 2>&1; then
        groupdel arcade 2>/dev/null || true
    fi

    echo ">>> Purge complete. PostgreSQL database was NOT dropped (manual step)."
fi
