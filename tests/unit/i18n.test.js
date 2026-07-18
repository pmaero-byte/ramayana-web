import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { setLocale, getLocale, t, translateCorpus, availableLocales } from '../../js/core/i18n.js';

describe('i18n', () => {
  afterEach(() => setLocale('en'));

  it('exposes both locales', () => {
    expect(availableLocales()).toEqual(expect.arrayContaining(['en', 'sa']));
  });

  it('t interpolates vars and falls back to key on miss', () => {
    expect(t('hp.label', { hp: 5, max: 5 })).toBe('HP 5/5');
    expect(t('does.not.exist')).toBe('does.not.exist');
  });

  it('switches to Sanskrit on setLocale', () => {
    setLocale('sa');
    expect(getLocale()).toBe('sa');
    expect(t('hp.label', { hp: 3, max: 5 })).toContain('प्राणाः');
  });

  it('translateCorpus replaces known char names', () => {
    setLocale('sa');
    expect(translateCorpus('Rama', 'char')).toBe('रामः');
    setLocale('en');
    expect(translateCorpus('Rama', 'char')).toBe('Rama');
  });

  it('translateCorpus returns input on unknown key', () => {
    setLocale('en');
    expect(translateCorpus('NotARealName_xyz', 'char')).toBe('NotARealName_xyz');
  });
});
