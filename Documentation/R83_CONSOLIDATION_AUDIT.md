# R83 Feature Parity Audit — ELGODS vs RamayanaPS5

**Date:** 2026-07-22  
**Status:** AD-HOC — Day 20 audit  
**Scope:** Read-only. No source changes.  
**Source of truth:** ELGODS `portal/` at `/Users/prabaharan/jambudweep/ELGODS/portal/`

---

## Executive summary

| Dimension | ELGODS | RamayanaPS5 | Delta |
|---|---|---|---|
| Characters | 26 | 26 (yes) | 0 |
| Story moments | 312 | 36 (Bala Kanda only) | -276 |
| Voice registers | 11 | 11 (yes) | 0 |
| Combat formation system | Arc/Chakra/Vyuha + Rakshasa targeting | Arc/Chakra/Vyuha + WaveController + VerseCombatTrigger | partial |
| HUD components | 30+ | 15 | -15+ |
| Save/load | 3-slot + auto-save | 1-slot SaveLoadHud | -2 slots + auto |
| Scene transitions | flash + verse drum | VerseFlashOverlay + VerseDrumKick | comparable |
| Test coverage | 715 Vitest (71 JS files) | 0 Unity EditMode tests | -715 |
| Build pipeline | Next.js + Capacitor | Unity 6000.5.4f1 admixed iOS scaffold (blocked by missing module) | divergent |
| Live deploy | elgods.jambudweep.tech | none yet | — |

**Verdict:** RamayanaPS5 has the data layer and the verse→combat loop, but it is
not feature-equivalent with ELGODS. Deprecation of the ELGODS live site is
premature until this audit’s gaps are closed.

---

## 1. ELGODS component inventory (107 game source files, 71 tests)

### 1.1 HUD / feedback (51 components)
ELGODS ships these in `src/game/ramayana/components/`:
AchievementGallery, ActCompletionCelebration, ActEditor, BlessingMeter,
CharacterSelect, CinematicLoadingScreen, DharmaLedger, DharmaLedgerMini,
DharmaScoreDisplay, DharmicQuiz, Dialogue, EmbassyRecapHUD, GlossaryTooltip,
GuidedModePill, KandaTree, KnowledgeDashboard, KnowledgeSummary,
LevelTransition, LoadingScreen, Minimap, MobileTouchHint, NPCDialogue,
NavaRasa, ObjectiveTrackerPill, PhotoMode, PremiumUpgradeModal,
RamayanaStatistics, RegionalRetellings, SacredHUD, SadhanaPath,
SattvikPause, SceneSelector, ShlokaDisplay, ShlokaRecitation, StoryJournal,
StoryPrologue, StoryRecap, StorySceneImage, StorySummary, TimeOfDayControls,
TimeOfDayPill, TirthaYatra, TitleScreen, VerseDrumKick, VerseFlashOverlay,
VerseHud, VerseIntroTTS, VerseSaveState, VerseStreakHUD, VersesProgressHUD,
WarDrumBeat.

### 1.2 Data bindings
- characters.json (26)
- voices.json (11 registers × 26 characters = 286 bindings)
- moments corpus: Bala Kanda 36, Ayodhya ~48, Aranya ~55, Kishkindha ~42,
  Sundara ~52, Yuddha ~79 → 312 total.

### 1.3 System features
- Multi-slot save/load — 3 named slots + auto-save
- Arc completion unlock chain
- Scene transition flash + verse drum kick
- Touch + portrait 1080×1920
- Kanda tree navigation
- Sadhana path meta-progression
- Quiz + achievements + statistics + photo mode + shloka recitation
- Regional retellings (translations)
- NPC dialogue system
- Glossary + knowledge dashboard

---

## 2. RamayanaPS5 current state (62 C# files, 6 JSON corpora)

### 2.1 Data layer
| File | Status |
|---|---|
| characters.json | ✅ 26 chars |
| voices.json | ✅ 11 registers + 55 bindings |
| moments_bala_kanda.json | ✅ 36 moments, sarga 1–73 |
| corpus_data.json | ✅ Day 1–10 corpus |

### 2.2 C# scripts by module
- `Assets/Scripts/Data/` — RamayanaCharactersData, RamayanaMomentsData,
  RamayanaVoicesData, ActData, EpisodeData, StoryMomentData, RamayanaTypes
- `Assets/Scripts/Verse/` — VerseOrchestrator (onVerseLoaded, onMomentEntered)
- `Assets/Scripts/Feedback/` — TimeOfDayPill, GuidedModePill, DharmaLedgerMini,
  QuestPill, VerseStreakHUD, DayDotStrip, VerseHud, VerseFlashOverlay,
  VerseDrumKick, VerseIntroTTS, VerseSaveState, VerseHapticFeedback,
  BattleBackdrop, EmbassyRecapHUD, TributeToFallenHUD, CinematicLetterbox,
  SanskritTitle, StaminaBar, HudOrchestrator, KandaPortraitHUD, SaveLoadHud,
  TitleScreenTapZone, DialogueOverlay
- `Assets/Scripts/Combat/` — WaveController, FormationStrategy, VerseCombatTrigger,
  ArcherAutoFire, ArrowProjectile, BowCooldown, RakshasaTarget
- `Assets/Scripts/UI/` — SafeAreaOverlay, PortraitResolver, Screens
- `Assets/Scripts/Audio/` — RagaAudioEngine
- `Assets/Scripts/Platform/` — PlatformShims, MacDesktopInput

### 2.3 Gaps vs ELGODS

#### CRITICAL gaps
1. **Moments corpus** — only Bala Kanda (36/312). Needs Ayodhya through Yuddha.
2. **No Unity EditMode tests** — 715 ELGODS tests have no RamayanaPS5 analog.
3. **iOS build blocked** — Unity iOS Build Support module missing from
   6000.5.4f1; xcodebuild fails with `ld: framework 'UnityRuntime' not found`.
4. **Save system** — 1-slot only; ELGODS has 3-slot + auto-save.

#### HIGH gaps
5. **No Kanda tree navigation** — ELGODS has KandaTree component;
   RamayanaPS5 has DayDotStrip but not kanda-scoped navigation.
6. **No Sadhana path** — meta-progression summary missing.
7. **No Quiz / Achievements / Statistics** — 3 ELGODS components not ported.
8. **No shloka recitation** — audio-prompted recall mode missing.
9. **No regional retellings** — i18n limited to Sanskrit/English in assets;
   no UI for Hindi/Tamil/South retellings.
10. **No NPC dialogue system** — `NPCDialogue.tsx` has branching conversational
    model; RamayanaPS5 has DialogueOverlay but not branching NPC layer.

#### MEDIUM gaps
11. **Photo mode** — ELGODS has PhotoMode component.
12. **Blessing meter** — dharma-point visualizer.
13. **Minimap** — not implemented.
14. **LoadingScreen** — CinematicLetterbox exists but no loading-state HUD.
15. **Character select polish** — ELGODS has CharacterSelect.tsx with group tabs;
    RamayanaPS5 needs equivalent scene.

---

## 3. Cross-reference: ELGODS → RamayanaPS5 feature map

| ELGODS component | RamayanaPS5 analog | Status |
|---|---|---|
| RamayanaGame.tsx | VerseOrchestrator + VerseCombatTrigger | ✅ partial |
| storyEngine.ts | StoryMomentPlayer (pre-Day 10) + VerseOrchestrator | ✅ |
| WaveController | WaveController + FormationStrategy + VerseCombatTrigger | ✅ |
| characterStoryArcs.ts | characters.json + moments_bala_kanda.json | ⚠️ 36/312 |
| characterVoices.ts | voices.json | ✅ |
| DharmicQuiz | none | ❌ |
| AchievementGallery | none | ❌ |
| SadhanaPath | none | ❌ |
| KandaTree | DayDotStrip | ⚠️ |
| SadhanaPath | none | ❌ |
| ShlokaRecitation | VerseIntroTTS | ⚠️ |
| RegionalRetellings | none | ❌ |
| PhotoMode | none | ❌ |
| Minimap | none | ❌ |
| CharacterSelect.tsx | none (assumed in scene) | ❓ |
| NPCDialogue | DialogueOverlay | ⚠️ |
| LoadingScreen | CinematicLetterbox | ⚠️ |

Legend: ✅ parity | ⚠️ partial | ❌ missing | ❓ unclear

---

## 4. Data fidelity

- Yamaka/anuprasa coverage: not yet audited against corpus_data.json; corpus
  lives in 3 JSON files across 3 directories.
- Tier system: ELGODS enforces Tier 1/2/3 provenance per entry.
  RamayanaPS5 has no provenance tracking yet.
- Source citations: Bala Kanda moments reference sarga IDs from character
  `knowledge` field; higher kāṇḍa not yet ported.

---

## 5. Audio parity

| ELGODS | RamayanaPS5 | Status |
|---|---|---|
| Prodedural Web Audio API | RagaAudioEngine | ✅ |
| Voice register mixer | VoiceSelect / RamayanaVoicesData | ⚠️ |
| Shloka recitation | VerseIntroTTS | ⚠️ |
| War drum + verse drum kick | VerseDrumKick | ✅ |
| Haptic feedback | VerseHapticFeedback | ✅ |

---

## 6. Test parity

| Dimension | ELGODS | RamayanaPS5 |
|---|---|---|
| Unit tests | 715 Vitest across 32 files | 0 |
| E2E tests | browser.spec.js | 0 |
| Per-day hermes verifier | N/A | 10 scripts |
| Unity batchmode compile | Next.js build | 1 historical successful compile |
| iOS Simulator build | Capacitor | Blocked (iOS module missing) |

---

## 7. Platform parity

| Dimension | ELGODS | RamayanaPS5 |
|---|---|---|
| Primary platform | Web (browser) | iOS Simulator (scaffold only) |
| Deployment | Railway (Nixpacks) | None |
| Offline / PWA | Service Worker registered | Not started |
| Input | Touch + keyboard | PlatformShims + MacDesktopInput |
| Orientation | Portrait 1080×1920 | Portrait (SafeAreaOverlay added) |

---

## 8. Build/pipeline parity

| Dimension | ELGODS | RamayanaPS5 |
|---|---|---|
| CI | GitHub Actions (verify.yml) | None |
| Type check | `npx tsc --noEmit` | `bash -n` on scripts |
| Lint | ESLint via CI | None in Unity |
| Build | `npx next build` | Unity batchmode (iOS blocked) |
| Artifact | Static deploy to Railway | Build/iOSSimulator/ (empty due to linker fail) |

---

## 9. Recommended launch order

1. Install Unity iOS Build Support → unblock Day 18 real xcodebuild circuit.
2. Port Ayodhya–Yuddha moments corpus (Days equivalent of 12 extended).
3. Add `VersesProgressHUD`/`KandaTree` scene-scoped navigation.
4. Wire 3-slot save/load + auto-save.
5. Add EditMode tests for VerseOrchestrator and WaveController.
6. Port NPCDialogue branching model into DialogueOverlay.
7. Add Quiz, SadhanaPath, Achievements, Statistics HUDs.
8. Add PhotoMode, BlessingMeter, Minimap if needed for gameplay.
9. Revisit ELGODS deprecation banner only after RamayanaPS5 is
   feature-equivalent and unblocked at iOS gate.

---

## 10. Plan phasing notes

- Phases 0–2 shipped: repo hygiene, data import, gameplay foundation.
- Phase 3 partially shipped: build scaffold done, but iOS blocker not resolved.
- ELGODS banner deferred: hard constraint in consolidation plan —
  ELGODS stays live during consolidation; do not break production deploy.
  RamayanaPS5 is not yet feature-equivalent, so banner step postponed.
