#!/usr/bin/env python3
"""
RAMAYANA PS5 — corpus_to_unity.py (production version)
════════════════════════════════════════════════════════════════════════════════
Python entry point that wraps a Node-based TypeScript literal parser.

Why: parsing TypeScript object literals (with type annotations, single/double
quoted strings, escapes, comments) is 10x cleaner in JS via eval() than in pure
Python. We shell out to node for the parsing and return canonical JSON.

Usage:
  python3 Tools/corpus_to_unity.py
    --source ../Elgods/godsofjambudweep/portal/src/game/ramayana/gameData.ts
    --out    Assets/Resources/corpus_data.json

Schema: see CorpusImporter.cs (Unity side, reads corpus_data.json →
        ScriptableObject assets).

Reference: parity with civaka-cintamani-ps5/Tools/corpus_to_unity.py
"""
import argparse
import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
PARSER_SCRIPT = REPO_ROOT / "Tools" / "_ts_parser.mjs"


def ensure_parser_script() -> Path:
    """Write the node TS-literal parser to Tools/_ts_parser.mjs."""
    if PARSER_SCRIPT.exists():
        return PARSER_SCRIPT

    PARSER_SCRIPT.write_text('''// RAMAYANA PS5 — TypeScript literal → JSON parser
// Extracts the exported `ramayanaEpisode` constant from gameData.ts,
// strips type annotations, normalizes strings, and outputs canonical JSON.
import fs from "fs";

const srcPath = process.argv[2];
const src = fs.readFileSync(srcPath, "utf-8");

const m = src.match(/export const ramayanaEpisode\\s*:\\s*RamayanaEpisode\\s*=\\s*\\{/);
if (!m) { console.error("no match"); process.exit(1); }
let start = m.index + m[0].length;
let depth = 1, i = start;
while (i < src.length && depth > 0) {
  const c = src[i];
  if (c === "{") depth++;
  else if (c === "}") depth--;
  i++;
}
let lit = src.slice(start, i - 1);

// Strip TS type annotations AFTER keys
lit = lit.replace(/:\\s*[A-Z]\\w*(<[^>]*>)?(\\|\\s*[A-Z]\\w*(<[^>]*>)?)*\\s*(?=[,})\\n])/g, "");
lit = lit.replace(/\\s+as\\s+\\w+(<[^>]*>)?/g, "");

// Normalize ALL strings to double-quoted (handles both ' and " delimiters)
let out = "", j = 0, inStr = null, buf = "";
while (j < lit.length) {
  const c = lit[j];
  if (inStr === null) {
    if (c === "'" || c === "\\"") { inStr = c; buf = ""; j++; continue; }
    out += c; j++; continue;
  }
  if (c === "\\\\" && j + 1 < lit.length) { buf += c + lit[j+1]; j += 2; continue; }
  if (c === inStr) { out += JSON.stringify(buf); inStr = null; buf = ""; j++; continue; }
  buf += c; j++;
}
if (inStr !== null) out += JSON.stringify(buf);
lit = out;

// Trailing commas
lit = lit.replace(/,(\\s*[\\]\\}])/g, "$1");

const obj = eval("({" + lit + "})");
console.log(JSON.stringify({
  schemaVersion: "1",
  characters: obj.characters.map(c => ({
    characterId: c.id, displayName: c.name, role: c.role, color: c.color
  })),
  acts: obj.acts.map(a => ({
    actId: a.id, actNumber: a.actNumber, title: a.title,
    location: a.location, scene: a.scene, setup: a.setup,
    lesson: a.lesson, playerRole: a.playerRole,
    objectives: (a.objectives || []).map(o => ({
      id: o.id, type: o.type, title: o.title, marker: o.marker,
      cue: o.cue, actionLabel: o.actionLabel,
      completedLine: o.completedLine || {},
      target: o.target,
      position: o.position || {x:0, y:0, z:0}
    })),
    dialogue: (a.dialogue || []).map(d => ({
      speaker: d.speaker, text: d.text, voice: d.voice || "kathaka"
    })),
    reward: a.reward || {}
  }))
}, null, 2));
''')
    return PARSER_SCRIPT


def main():
    ap = argparse.ArgumentParser(description="Convert Ramayana gameData.ts → Unity corpus JSON")
    ap.add_argument("--source", required=True, help="Path to gameData.ts")
    ap.add_argument("--out", required=True, help="Output corpus_data.json path")
    ap.add_argument("--dry-run", action="store_true", help="Parse but don't write")
    args = ap.parse_args()

    src = Path(args.source)
    dst = Path(args.out)

    if not src.exists():
        print(f"[corpus_to_unity] Source not found: {src}", file=sys.stderr)
        sys.exit(1)

    if not shutil.which("node"):
        print("[corpus_to_unity] ERROR: 'node' not found in PATH. Install Node.js 18+.", file=sys.stderr)
        sys.exit(2)

    parser = ensure_parser_script()
    print(f"[corpus_to_unity] Parsing {src}")

    result = subprocess.run(
        ["node", str(parser), str(src)],
        capture_output=True, text=True, timeout=60,
    )
    if result.returncode != 0:
        print(f"[corpus_to_unity] Node parser failed:\n{result.stderr}", file=sys.stderr)
        sys.exit(3)

    corpus = json.loads(result.stdout)

    # Sanity-check counts
    char_count = len(corpus.get("characters", []))
    act_count = len(corpus.get("acts", []))
    obj_count = sum(len(a.get("objectives", [])) for a in corpus.get("acts", []))
    dia_count = sum(len(a.get("dialogue", [])) for a in corpus.get("acts", []))

    print(f"[corpus_to_unity] Parsed: {char_count} chars, {act_count} acts, {obj_count} objectives, {dia_count} dialogue lines")

    if args.dry_run:
        print("[corpus_to_unity] Dry run — not writing.")
        return

    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_text(json.dumps(corpus, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"[corpus_to_unity] Wrote {dst} ({dst.stat().st_size:,} bytes)")


if __name__ == "__main__":
    main()
