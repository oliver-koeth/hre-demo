#!/bin/sh
set -eu

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
SITE_DIR="${ROOT_DIR}/site"

rm -rf "${SITE_DIR}"
mkdir -p "${SITE_DIR}"

# Export tracked repository content (no .git metadata) so relative links keep working.
cd "${ROOT_DIR}"
git archive --format=tar HEAD | tar -xf - -C "${SITE_DIR}"

cat > "${SITE_DIR}/index.html" <<'HTML'
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>AI-DLC Walkthrough</title>
  <meta http-equiv="refresh" content="0; url=./aidlc-docs/v1-ai-dlc-walkthrough_v2.html" />
</head>
<body>
  <p>Redirecting to the walkthrough… <a href="./aidlc-docs/v1-ai-dlc-walkthrough_v2.html">open it here</a>.</p>
</body>
</html>
HTML

# Ensure GitHub Pages serves all files including dot-directories.
touch "${SITE_DIR}/.nojekyll"

echo "Pages site built at: ${SITE_DIR}"
