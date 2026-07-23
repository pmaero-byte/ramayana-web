#!/bin/bash
# Day 19 — archive ramayana-web + ELGODS deprecation banner.
set -u
PROJECT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

SRC_LAYOUT="/Users/prabaharan/jambudweep/ELGODS/portal/src/app/layout.tsx"

# _archive directory exists in RamayanaPS5
[ -d "$PROJECT/_archive/ramayana-web" ] \
  && p "_archive/ramayana-web exists" || f "_archive/ramayana-web missing"

# Archive README present
[ -f "$PROJECT/_archive/ramayana-web/README.md" ] \
  && p "archive README exists" || f "archive README missing"

# Archive README mentions deprecation + date + canonical project
ARCHIVE_README=$(cat "$PROJECT/_archive/ramayana-web/README.md" 2>/dev/null || true)
echo "$ARCHIVE_README" | grep -qi 'DEPRECATED' \
  && p "README says DEPRECATED" || f "README missing DEPRECATED"
echo "$ARCHIVE_README" | grep -qi 'RamayanaPS5' \
  && p "README mentions RamayanaPS5" || f "README missing RamayanaPS5"
echo "$ARCHIVE_README" | grep -qi '2026-07-22' \
  && p "README has archive date" || f "README missing archive date"
echo "$ARCHIVE_README" | grep -qi '_archive/ramayana-web' \
  && p "README references archive path" || f "README missing archive path"
echo "$ARCHIVE_README" | grep -qi '/Users/prabaharan/Other_projects/ramayana-web' \
  && p "README references source path" || f "README missing source path"

# Archive exports directory contains key files
for f in index.html README.md js/main.js js/combat js/core; do
  [ -e "$PROJECT/_archive/ramayana-web/$f" ] \
    && p "archive contains $f" || f "archive missing $f"
done

# Source directory still present (not deleted, just copied)
[ -d "/Users/prabaharan/Other_projects/ramayana-web" ] \
  && p "source ramayana-web still present" || f "source ramayana-web deleted"

# ELGODS layout.tsx untouched by Day 19 archive step
SRC_LAYOUT="/Users/prabaharan/jambudweep/ELGODS/portal/src/app/layout.tsx"
DIFF=$(cd "$PROJECT" && git diff --name-only 2>/dev/null | grep -c "$SRC_LAYOUT" || true)
[ "$DIFF" = "0" ] && p "ELGODS layout.tsx not yet modified" || f "ELGODS layout.tsx already modified ($DIFF)"

# ELGODS layout.tsx exists
[ -f "$SRC_LAYOUT" ] \
  && p "ELGODS layout.tsx exists" || f "ELGODS layout.tsx missing"
echo "$SRC_LAYOUT" | grep -q 'layout.tsx' \
  && p "layout.tsx path sanity" || f "layout.tsx path mismatch"

# _archive ignored by git?
if [ -f "$PROJECT/.gitignore" ]; then
  grep -q '_archive/' "$PROJECT/.gitignore" \
    && p "_archive/ in .gitignore" || f "_archive/ not in .gitignore"
else
  f ".gitignore missing"
fi

# Regression: Day 18 scripts still pass syntax
bash -n "$PROJECT/Tools/qa/build-ios-sim.sh" 2>/dev/null \
  && p "build-ios-sim.sh syntax" || f "build-ios-sim.sh syntax"
bash -n "$PROJECT/Tools/qa/hermes-verify-ramayana-ios-sim-day18.sh" 2>/dev/null \
  && p "Day 18 verifier syntax" || f "Day 18 verifier syntax"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 19)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
