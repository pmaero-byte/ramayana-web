#!/bin/bash
# Deploy ramayana-web to a static server path (macOS local / GitHub Pages / Hertree)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

case "${1:-help}" in
  gh-pages)
    # Prefer Pages from main/ (already the ship branch). Ensures/repairs the site.
    OWNER_REPO="${2:-pmaero-byte/ramayana-web}"
    echo "=== GitHub Pages from main/ @ $OWNER_REPO ==="
    cd "$ROOT"
    git push origin main
    # Create or update Pages config (main branch, site root)
    if gh api "repos/$OWNER_REPO/pages" >/dev/null 2>&1; then
      gh api -X PUT "repos/$OWNER_REPO/pages" --input - <<'EOF'
{"build_type":"legacy","source":{"branch":"main","path":"/"}}
EOF
    else
      gh api -X POST "repos/$OWNER_REPO/pages" --input - <<'EOF'
{"build_type":"legacy","source":{"branch":"main","path":"/"}}
EOF
    fi
    echo "Site: https://$(echo "$OWNER_REPO" | cut -d/ -f1).github.io/$(echo "$OWNER_REPO" | cut -d/ -f2)/"
    gh api "repos/$OWNER_REPO/pages" --jq '{url:.html_url,status:.status,source:.source}'
    ;;
  hertree)
    DST="${2:-/Users/prabaharan/Aerospace_projects/Hertree/hertree/public/game/ramayana-web}"
    echo "=== Syncing to hertree embed: $DST ==="
    mkdir -p "$DST"
    rsync -a --delete \
      --exclude='.git/' --exclude='.hermes/' --exclude='CONTINUE.md' \
      --exclude='.github/' --exclude='node_modules/' \
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
    echo "  $0 gh-pages [user/repo]   — enable/update GitHub Pages from main/"
    echo "  $0 hertree                — rsync into hertree public dir"
    echo "  $0 static                 — python3 http.server :8765"
    exit 1
    ;;
esac
