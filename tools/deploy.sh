#!/bin/bash
# Deploy ramayana-web to a static server path (macOS local / GitHub Pages / Hertree)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"

case "${1:-help}" in
  gh-pages)
    HOST="${2:-pmaero-byte/ramayana-web}"
    echo "=== Deploying to GitHub Pages (orphan gh-pages) ==="
    cd "$ROOT"
    git branch -D gh-pages 2>/dev/null || true
    git checkout --orphan gh-pages
    rm -f .gitignore .gitattributes
    git add -A
    git commit -m "deploy: ramayana-web action slice"
    git push -f origin gh-pages
    git checkout main
    echo "Deployed to https://$HOST/"
    ;;
  hertree)
    DST="${2:-/Users/prabaharan/jambudweep/hertree/public/game/ramayana-web}"
    echo "=== Syncing to hertree embed: $DST ==="
    mkdir -p "$DST"
    rsync -a --delete --exclude='.git/' --exclude='.hermes/' \
      "$ROOT/" "$DST/"
    echo "Synced. Serve via hertree dev server."
    ;;
  static)
    echo "=== Serving via python3 ==="
    cd "$ROOT"
    python3 -m http.server 8765
    ;;
  *)
    echo "Usage: $0 {gh-pages|hertree|static}"
    echo "  $0 gh-pages [user/repo]   — push to GitHub Pages"
    echo "  $0 hertree                — rsync into hertree public dir"
    echo "  $0 static                 — python3 http.server :8765"
    exit 1
    ;;
esac
