/**
 * HERMES: The Ramayana — Interactive Story Player
 *
 * Loads Assets/Resources/corpus_data.json + AI-generated illustrations
 * from Assets/Illustrations/ and presents the 8-act Ramayana as a Netflix-style
 * story picker + scene-by-scene narrative player.
 *
 * Designed to feel like a PS5 cinematic experience — full-screen plates,
 * atmospheric backgrounds, sanskrit shlokas with translations.
 */

// ── Configuration ────────────────────────────────────────────
const ASSETS_BASE = '../Assets';
const CORPUS_URL = `${ASSETS_BASE}/Resources/corpus_data.json`;
const CHARACTERS_BASE = `${ASSETS_BASE}/Illustrations/characters`;
const CITIES_BASE = `${ASSETS_BASE}/Illustrations/cities`;
const ATMOSPHERE_BASE = `${ASSETS_BASE}/Illustrations/atmosphere`;
const STORY_MOMENTS_BASE = `${ASSETS_BASE}/Illustrations/story_moments`;

// HERMES: Map act IDs → atmosphere backgrounds (location-keyed)
const ACT_ATMOSPHERE = {
  'bala-birth':             'sky_dawn_ayodhya.png',
  'ayodhya-dharma':         'sky_sarayu_banks.png',
  'panchavati-golden-deer': 'sky_night_chitrakuta.png',
  'kishkindha-alliance':    'sky_himalayas_dawn.png',
  'sundarakanda-leap':      'sky_dusk_lanka.png',
  'yuddhakanda-war':        'sky_ocean_storm.png',
  'return-ayodhya':         'sky_celestial_heaven.png',
  'uttara-earth-return':    'sky_sunset_setu.png',
};

// HERMES: Map act → scene plate (story_moments/ collection)
const ACT_SCENE = {
  'bala-birth':           '01_sita_swayamvar.png',
  'ayodhya-dharma':       '02_rama_breaks_bow.png',
  'panchavati-golden-deer': '03_forest_exile.png',
  'kishkindha-alliance':  '05_hanuman_meets_rama.png',
  'sundarakanda-leap':    '04_sita_in_ashoka_vatika.png',
  'yuddhakanda-war':      '06_setu_bandhan.png',
  'return-ayodhya':       '08_rama_coronation.png',
  'uttara-earth-return':  '10_rama_darshana.png',
};

// HERMES: Speaker-to-character-portrait map (id → filename)
const SPEAKER_PORTRAITS = {
  // HERMES Round 8+: 27 character portraits mapped (was 21)
  'narrator':     'vasishtha.png',
  'rama':         'rama.png',
  'sita':         'sita.png',
  'hanuman':      'hanuman.png',
  'ravana':       'ravana.png',
  'lakshmana':    'lakshmana.png',
  'bharata':      'bharata.png',
  'manthara':     'manthara.png',
  'kaikeyi':      'kaikeyi.png',
  // Das/asharatha spelling variants both resolve to portrait
  'dasaratha':    'dasaratha.png',
  'dasharatha':   'dasaratha.png',
  'sumitra':      'sumitra.png',
  'kausalya':     'kausalya.png',
  'shurpanakha':  'shurpanakha.png',
  'indrajit':     'indrajit.png',
  'mandodari':    'mandodari.png',
  'kumbhakarna':  'kumbhakarna.png',
  'vasishtha':    'vasishtha.png',
  'vishwamitra':  'vishwamitra.png',
  'vali':         'vali.png',
  'angada':       'angada.png',
  'jatayu':       'jatayu.png',
  // New Round 8 batch
  'jambavan':     'jambavan.png',
  'trijata':      'trijata.png',
  'valmiki':      'valmiki.png',
  'janaka':       'janaka.png',
  'shatrughna':   'shatrughna.png',
  'shabari':      'shabari.png',
  'sugriva':      'sugriva.png',
  // Round 8 wave 2 — major remaining speakers
  'vibhishana':   'vibhishana.png',
  'tara':         'tara.png',
  'sampati':      'sampati.png',
  'tataka':       'tataka.png',
  'ahalya':       'ahalya.png',
  'kabandha':     'kabandha.png',
  'urmila':       'urmila.png',
  'guha':         'guha.png',
  'anasuya':      'anasuya.png',
};

const TIPS = [
  'Preparing the ancient tale…',
  'Gathering the divine characters…',
  'Painting the scenes from memory…',
  'Loading the eight acts…',
  'Spreading the sacred verses…',
];

// ── State ────────────────────────────────────────────────────
let CORPUS = null;
let CURRENT_ACT = null;
let CURRENT_MOMENT = 0;
let momentSequence = []; // flattened moments across acts
let globalIndex = 0;

// ── Loader Tips ──────────────────────────────────────────────
let tipIndex = 0;
const tipEl = () => document.getElementById('loader-tip');
const fillEl = () => document.querySelector('.loader-fill');

function setTip() {
  tipEl().textContent = TIPS[tipIndex % TIPS.length];
  tipIndex++;
}
setTip();
const tipInterval = setInterval(() => {
  setTip();
  fillEl().style.width = `${Math.min(95, tipIndex * 20)}%`;
}, 1200);

// ── Boot ─────────────────────────────────────────────────────
async function boot() {
  try {
    // HERMES: load corpus (uses fetch from local file server or static)
    const res = await fetch(CORPUS_URL);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    CORPUS = await res.json();
    console.log(`Loaded ${CORPUS.acts.length} acts, ${CORPUS.characters.length} characters`);

    // Compute flat sequence of moments
    momentSequence = [];
    for (const act of CORPUS.acts) {
      for (const moment of (act.dialogue || act.moments || [])) {
        momentSequence.push({ act, moment });
      }
    }
    console.log(`Total moments: ${momentSequence.length}`);

    // Build act grid
    renderActGrid();

    // Fade out loader
    clearInterval(tipInterval);
    fillEl().style.width = '100%';
    setTimeout(() => {
      document.getElementById('loader').classList.add('fade-out');
      document.getElementById('title-screen').classList.add('active');
    }, 400);
  } catch (e) {
    console.error('Boot failed:', e);
    tipEl().textContent = `⚠ Failed to load corpus: ${e.message}. Serve via HTTP (python3 -m http.server).`;
  }
}

// ── Title screen ─────────────────────────────────────────────
function renderActGrid() {
  const grid = document.getElementById('act-grid');
  grid.innerHTML = '';
  for (const act of CORPUS.acts) {
    const card = document.createElement('button');
    card.className = 'act-card';
    card.innerHTML = `
      <div class="act-num-badge">Act ${act.actNumber} · ${act.scene?.toUpperCase() || ''}</div>
      <h3>${act.title}</h3>
      <div class="act-loc">📍 ${act.location || 'Unknown'}</div>
      <div class="act-lesson">${act.lesson || act.setup?.substring(0, 120) + '…' || ''}</div>
    `;
    card.addEventListener('click', () => enterAct(act));
    grid.appendChild(card);
  }
}

// ── Story player ─────────────────────────────────────────────
function enterAct(act) {
  CURRENT_ACT = act;
  CURRENT_MOMENT = 0;
  document.getElementById('title-screen').classList.remove('active');
  const ss = document.getElementById('story-screen');
  ss.classList.remove('hidden');
  ss.classList.add('active');
  document.getElementById('act-num').textContent = `Act ${act.actNumber} · ${act.scene?.toUpperCase() || ''}`;
  document.getElementById('act-title').textContent = act.title;
  renderCurrentMoment();
  document.getElementById('back-btn').focus();
}

function backToActs() {
  const ss = document.getElementById('story-screen');
  ss.classList.remove('active');
  ss.classList.add('hidden');
  document.getElementById('title-screen').classList.add('active');
}

function getCurrentMoments() {
  if (!CURRENT_ACT) return [];
  return CURRENT_ACT.dialogue || CURRENT_ACT.moments || [];
}

function renderCurrentMoment() {
  const moments = getCurrentMoments();
  if (!moments.length) {
    document.getElementById('moment-text').textContent =
      'No moments available for this act yet. Coming soon.';
    return;
  }
  const idx = Math.min(CURRENT_MOMENT, moments.length - 1);
  const m = moments[idx];

  // Update progress
  document.getElementById('story-progress').style.width =
    `${((idx + 1) / moments.length) * 100}%`;

  // Atmosphere background
  const atmos = ACT_ATMOSPHERE[CURRENT_ACT.actId];
  if (atmos) {
    document.getElementById('story-bg').style.backgroundImage =
      `url('${ATMOSPHERE_BASE}/${atmos}')`;
  }

  // Scene plate
  const scene = ACT_SCENE[CURRENT_ACT.actId];
  const sceneEl = document.getElementById('moment-scene');
  if (scene) {
    sceneEl.style.backgroundImage = `url('${STORY_MOMENTS_BASE}/${scene}')`;
  } else {
    sceneEl.style.backgroundImage = '';
  }

  // Speaker portrait
  const speakerKey = (m.speaker || '').toLowerCase().replace(/[^a-z]/g, '');
  const portraitFile = SPEAKER_PORTRAITS[speakerKey] || SPEAKER_PORTRAITS[m.speaker?.toLowerCase()];
  const portraitEl = document.getElementById('moment-portrait');
  if (portraitFile) {
    portraitEl.innerHTML = `<img src="${CHARACTERS_BASE}/${portraitFile}" alt="${m.speaker}" onerror="this.style.display='none'"/>`;
  } else {
    portraitEl.innerHTML = `<div style="width:140px;height:140px;border-radius:50%;background:var(--ink-mid);border:4px solid var(--gold-light);display:flex;align-items:center;justify-content:center;font-family:Cinzel,serif;font-size:48px;color:var(--saffron);">${(m.speaker || '?')[0]}</div>`;
  }

  // Speaker name + tone
  document.getElementById('speaker-name').textContent = m.speaker || 'Narrator';
  document.getElementById('speaker-tone').textContent = m.emotionalTone || '';

  // Text
  const textEl = document.getElementById('moment-text');
  textEl.textContent = m.text || m.description || '';

  // Shloka (if any)
  const shlokaBlock = document.getElementById('shloka-block');
  if (m.sanskritShloka) {
    shlokaBlock.style.display = 'block';
    document.getElementById('shloka-sanskrit').textContent = m.sanskritShloka;
    document.getElementById('shloka-translit').textContent = m.transliteration || '';
    document.getElementById('shloka-translation').textContent = m.shlokaTranslation || '';
  } else {
    shlokaBlock.style.display = 'none';
  }

  // Knowledge
  const knowledgeBlock = document.getElementById('knowledge-block');
  if (m.knowledge) {
    knowledgeBlock.style.display = 'block';
    document.getElementById('knowledge-text').textContent = m.knowledge;
  } else {
    knowledgeBlock.style.display = 'none';
  }

  // Objectives
  const objectives = (m.objectives || CURRENT_ACT.objectives || [])
    .map(o => `<span class="objective">✓ ${typeof o === 'string' ? o : (o.text || o.id || '')}</span>`)
    .join('');
  document.getElementById('objectives').innerHTML = objectives;

  // Nav button states
  document.getElementById('prev-btn').disabled = idx === 0;
  document.getElementById('next-btn').textContent =
    idx === moments.length - 1 ? 'Complete ✓' : 'Next ›';
}

function nextMoment() {
  const moments = getCurrentMoments();
  if (CURRENT_MOMENT >= moments.length - 1) {
    // Show completion
    document.getElementById('moment-text').textContent =
      'Act complete. Return to act selector or continue your journey.';
    document.getElementById('next-btn').disabled = true;
    return;
  }
  CURRENT_MOMENT++;
  renderCurrentMoment();
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function prevMoment() {
  if (CURRENT_MOMENT > 0) {
    CURRENT_MOMENT--;
    renderCurrentMoment();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}

// ── Wire up ──────────────────────────────────────────────────
document.getElementById('back-btn').addEventListener('click', backToActs);
document.getElementById('prev-btn').addEventListener('click', prevMoment);
document.getElementById('next-btn').addEventListener('click', nextMoment);
document.addEventListener('keydown', (e) => {
  if (!document.getElementById('story-screen').classList.contains('active')) return;
  if (e.key === 'ArrowRight' || e.key === ' ') { nextMoment(); e.preventDefault(); }
  if (e.key === 'ArrowLeft') { prevMoment(); }
  if (e.key === 'Escape') { backToActs(); }
});

// ── Character gallery ─────────────────────────────────────────
function openCharacterModal() {
  const modal = document.getElementById('character-modal');
  const grid = document.getElementById('character-grid');
  grid.innerHTML = '';

  for (const ch of CORPUS.characters) {
    const id = (ch.characterId || '').toLowerCase();
    const name = ch.displayName || ch.characterId;
    const portrait = SPEAKER_PORTRAITS[id];
    const card = document.createElement('div');
    card.className = 'character-card';
    card.innerHTML = `
      ${portrait
        ? `<img src="${CHARACTERS_BASE}/${portrait}" alt="${name}" loading="lazy" onerror="this.style.background='var(--ink-warm)';this.alt='${name}'"/>`
        : `<div style="aspect-ratio:1;display:flex;align-items:center;justify-content:center;font-family:Cinzel,serif;font-size:48px;color:var(--saffron);background:var(--ink-warm);">${(name || '?')[0]}</div>`
      }
      <div class="char-info">
        <div class="char-name">${name}</div>
        ${ch.role ? `<div class="char-title">${ch.role}</div>` : ''}
      </div>
    `;
    card.addEventListener('click', () => {
      // Close modal and try to find an act with this character
      closeCharacterModal();
      const matchingAct = CORPUS.acts.find(a =>
        (a.dialogue || []).some(d => (d.speaker || '').toLowerCase() === id || (d.speaker || '').toLowerCase() === name.toLowerCase())
      );
      if (matchingAct) {
        enterAct(matchingAct);
      } else {
        document.getElementById('title-screen').classList.add('active');
      }
    });
    grid.appendChild(card);
  }

  modal.classList.remove('hidden');
}

function closeCharacterModal() {
  document.getElementById('character-modal').classList.add('hidden');
}

document.getElementById('show-cast-btn').addEventListener('click', openCharacterModal);
document.getElementById('modal-close').addEventListener('click', closeCharacterModal);
document.getElementById('character-modal').addEventListener('click', (e) => {
  if (e.target.id === 'character-modal') closeCharacterModal();
});
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && !document.getElementById('character-modal').classList.contains('hidden')) {
    closeCharacterModal();
  }
});

boot();