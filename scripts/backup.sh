#!/bin/sh
set -eu

STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
TARGET="/backups/foundation-${STAMP}.tar.gz"

if [ -d /data ]; then
  tar -czf "$TARGET" -C / data
fi

