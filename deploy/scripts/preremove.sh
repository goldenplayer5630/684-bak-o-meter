#!/usr/bin/env bash
set -euo pipefail

echo ">>> Stopping 684bakometer service..."
systemctl stop 684bakometer.service 2>/dev/null || true
systemctl disable 684bakometer.service 2>/dev/null || true
