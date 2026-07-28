#!/usr/bin/env python3
from __future__ import annotations

import argparse
import re
from pathlib import Path


def is_local_href(href: str) -> bool:
    return not (
        href.startswith("#")
        or href.startswith("http://")
        or href.startswith("https://")
        or href.startswith("mailto:")
        or href.startswith("tel:")
    )


def extract_hrefs(html: str) -> list[str]:
    return re.findall(r'href="([^"]+)"', html)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate local href targets in walkthrough HTML.")
    parser.add_argument("--site-root", required=True, help="Root directory containing published files")
    parser.add_argument(
        "--walkthrough",
        default="aidlc-docs/v1-ai-dlc-walkthrough_v2.html",
        help="Walkthrough path relative to site-root",
    )
    args = parser.parse_args()

    site_root = Path(args.site_root).resolve()
    walkthrough = (site_root / args.walkthrough).resolve()
    if not walkthrough.exists():
        print(f"ERROR: Walkthrough file not found: {walkthrough}")
        return 1

    html = walkthrough.read_text(encoding="utf-8")
    hrefs = sorted(set(h for h in extract_hrefs(html) if is_local_href(h)))
    missing: list[tuple[str, Path]] = []
    escaped: list[tuple[str, Path]] = []

    for href in hrefs:
        target = (walkthrough.parent / href).resolve()
        try:
            target.relative_to(site_root)
        except ValueError:
            escaped.append((href, target))
            continue
        if not target.exists():
            missing.append((href, target))

    if escaped:
        print("ERROR: Link target escapes publish root:")
        for href, target in escaped:
            print(f"  - {href} -> {target}")

    if missing:
        print("ERROR: Missing local link targets:")
        for href, target in missing:
            print(f"  - {href} -> {target}")

    if escaped or missing:
        return 1

    print(f"OK: validated {len(hrefs)} local links in {walkthrough}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
