#!/bin/sh
set -eu

mkdir -p secrets

if [ ! -f secrets/encryption-key ]; then
  openssl rand -base64 32 > secrets/encryption-key
  echo "Created secrets/encryption-key"
fi

if [ ! -f secrets/hmac-key ]; then
  openssl rand -base64 32 > secrets/hmac-key
  echo "Created secrets/hmac-key"
fi

echo "Dev secrets are ready."
