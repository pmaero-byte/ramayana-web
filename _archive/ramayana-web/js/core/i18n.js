/** Lightweight i18n — strings.js pattern + locale files, no deps. */
import en from './locales/en.js';
import sa from './locales/sa.js';

const LOCALES = { en, sa };
let current = 'en';

export function setLocale(code) {
  if (LOCALES[code]) current = code;
}

export function getLocale() { return current; }

export function t(key, vars) {
  const dict = LOCALES[current] || LOCALES.en;
  let s = dict[key] || LOCALES.en[key] || key;
  if (vars) for (const [k, v] of Object.entries(vars)) s = s.replace(`{${k}}`, v);
  return s;
}

/** Translate a corpus character name or objective title from Sanskrit -> current locale. */
export function translateCorpus(text, type = 'char') {
  if (!text) return text;
  const dict = LOCALES[current] || LOCALES.en;
  const m = dict.corpus?.[type];
  if (m && m[text]) return m[text];
  return text;
}

export function availableLocales() { return Object.keys(LOCALES); }
