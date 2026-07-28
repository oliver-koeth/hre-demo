# Walkthrough GitHub Pages Deployment

## Purpose
Publish the AI-DLC walkthrough for this repository to:

- `https://oliver-koeth.github.io/hre-demo/`

The root URL redirects to:

- `aidlc-docs/v1-ai-dlc-walkthrough_v2.html`

## Build Strategy

1. `scripts/build-pages-site.sh` exports tracked repository content into `site/` using `git archive`.
2. It writes `site/index.html` as a redirect entry point for the walkthrough.
3. It creates `site/.nojekyll` so dot-directories (for example `.github` and `.aidlc-rule-details`) are served.

## Link Validation

`scripts/validate-walkthrough-links.py` validates all non-HTTP local `href` links in the walkthrough against the `site/` publish bundle.

Run locally:

```bash
sh scripts/build-pages-site.sh
python3 scripts/validate-walkthrough-links.py --site-root site
```

## CI Deployment

Workflow: `.github/workflows/pages-deploy.yml`

On push to `main` (or manual dispatch), CI:
1. Builds `site/`
2. Validates local walkthrough links
3. Uploads Pages artifact
4. Deploys via GitHub Pages

## Local + Pages Compatibility

- Local serving uses `scripts/serve-walkthrough.sh`.
- The walkthrough JS includes fetch fallbacks so markdown-popup links work in both localhost and GitHub Pages contexts.
