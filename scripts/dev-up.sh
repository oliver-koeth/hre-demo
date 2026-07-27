#!/bin/sh
set -eu

./scripts/generate-dev-secrets.sh
docker-compose up --build auth-module-api
