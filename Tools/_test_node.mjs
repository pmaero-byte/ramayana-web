// Extract ramayanaEpisode from gameData.ts — handles mixed quote styles.
import fs from 'fs';

const srcPath = process.argv[2];
const src = fs.readFileSync(srcPath, 'utf-8');

// 1. Find the literal via brace-balancing
const m = src.match(/export const ramayanaEpisode\s*:\s*RamayanaEpisode\s*=\s*\{/);
if (!m) { console.error('no match'); process.exit(1); }
let start = m.index + m[0].length;
let depth = 1, i = start;
while (i < src.length && depth > 0) {
  const c = src[i];
  if (c === '{') depth++;
  else if (c === '}') depth--;
  i++;
}
let lit = src.slice(start, i - 1);

// 2. Strip TS type annotations AFTER keys
lit = lit.replace(/:\s*[A-Z]\w*(<[^>]*>)?(\|\s*[A-Z]\w*(<[^>]*>)?)*\s*(?=[,}\)\n])/g, '');
lit = lit.replace(/\s+as\s+\w+(<[^>]*>)?/g, '');

// 3. Normalize ALL strings to double-quoted by walking char-by-char.
//    Track in-string state by quote type (' vs ").
let out = '';
let j = 0;
let inStr = null; // null | "'" | '"'
let buf = '';
while (j < lit.length) {
  const c = lit[j];
  if (inStr === null) {
    if (c === "'" || c === '"') {
      inStr = c;
      buf = '';
      j++;
      continue;
    }
    out += c;
    j++;
    continue;
  }
  // Inside string: handle escapes
  if (c === '\\' && j + 1 < lit.length) {
    buf += c + lit[j+1];
    j += 2;
    continue;
  }
  if (c === inStr) {
    // Close string → emit as double-quoted via JSON.stringify
    out += JSON.stringify(buf);
    inStr = null;
    buf = '';
    j++;
    continue;
  }
  buf += c;
  j++;
}
if (inStr !== null) {
  // Unterminated string — emit as-is (will fail eval but at least preserves data)
  out += JSON.stringify(buf);
}
lit = out;

// 4. Trailing commas
lit = lit.replace(/,(\s*[\]}])/g, '$1');

const obj = eval('({' + lit + '})');
console.log(JSON.stringify({
  schemaVersion: '1',
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
      position: o.position || {x:0,y:0,z:0}
    })),
    dialogue: (a.dialogue || []).map(d => ({
      speaker: d.speaker, text: d.text, voice: d.voice || 'kathaka'
    })),
    reward: a.reward || {}
  }))
}, null, 2));
