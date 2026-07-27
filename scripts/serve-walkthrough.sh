#!/bin/sh
set -eu

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
HOST="${HOST:-127.0.0.1}"
PORT="${PORT:-8000}"
WALKTHROUGH_URL="http://${HOST}:${PORT}/aidlc-docs/v1-ai-dlc-walkthrough.html"

if ! command -v watchexec >/dev/null 2>&1; then
  echo "Error: watchexec is required but not found in PATH." >&2
  echo "Install it first, e.g. 'brew install watchexec'." >&2
  exit 1
fi

if ! command -v python3 >/dev/null 2>&1; then
  echo "Error: python3 is required but not found in PATH." >&2
  exit 1
fi

echo "Serving walkthrough with auto-restart on changes."
echo "Open: ${WALKTHROUGH_URL}"
echo "Watching: aidlc-docs/, _knowledge/compliance-frameworks/"

exec watchexec \
  --restart \
  --watch "${ROOT_DIR}/aidlc-docs" \
  --watch "${ROOT_DIR}/_knowledge/compliance-frameworks" \
  --exts html,md,css,js,json \
  -- python3 -m http.server "${PORT}" --bind "${HOST}" --directory "${ROOT_DIR}"
