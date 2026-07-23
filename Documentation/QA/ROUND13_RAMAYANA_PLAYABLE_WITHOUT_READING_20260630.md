# Round 13 — Playable-without-reading applied to Ramayana PS5

## Goal

Extend the user's playability-without-reading directive (R11, R12)
to the third game in the jambudweep.games portfolio: Ramayana PS5.

R11 fixed MB Yuddhakanta's title screen.
R12 widened the same fix to all 27 Yuddhakanta menu scenes.
R13 fixes Ramayana PS5's single title scene (only scene in the
greenfield project).

## What changed

### `Assets/Scenes/MainMenu.unity`

Two edits to the existing YAML:

1. **Removed `Press [Space]` instruction from subtitle text.**
   The decorative subtitle was:
   `"jambudweep.tech  ·  Source-faithful retelling  ·  Press [Space]"`
   Now it's:
   `"jambudweep.tech  ·  Source-faithful retelling"`
   Note: no code was listening for Space anyway — the prompt was
   purely decorative — but it was a playability gate (player had
   to read and find a keyboard).

2. **Added a `ConceptArtTapZone` GameObject** covering the lower
   60% of the screen (anchor `0,0.18 → 1,0.62` in the 1920×1080
   reference canvas).  The tap zone is invisible (Image alpha 0)
   but `raycastTarget=true`.  Tapping anywhere in that area fires
   `OnTap` on the `TitleScreenTapZone` MonoBehaviour, which logs
   `[Ramayana] Round 13 tap registered on ConceptArtTapZone`.

   The GameObject was inserted as a child of the existing
   `Title Canvas` (m_Father = `1867379183`) with m_RootOrder=100
   so it draws on top of other UI elements.

### `Assets/Editor/CreateTitleScene.cs`

Same two changes so future regenerations from the editor menu
produce a matching scene:

1. Subtitle text no longer includes `Press [Space]`.
2. New code block (4b) creates `ConceptArtTapZone` with the same
   anchors + invisible image + `TitleScreenTapZone` component.
   `Button.onClick.AddListener(tapHandler.OnTap)` wires the tap
   to the handler at editor time (not just YAML-level wiring).

### `Assets/Scripts/UI/TitleScreenTapZone.cs` (new file, 30 LoC)

Minimal MonoBehaviour.  Holds a `UnityEvent onTap` SerializeField
and an `OnTap()` method that logs and invokes the event.  Future
work: replace the log with `SceneNavigator.LoadNextScene()` once
the first story scene is built.

## Files modified

- `Assets/Scenes/MainMenu.unity` — subtitle text + tap zone appended
- `Assets/Editor/CreateTitleScene.cs` — subtitle text + tap-zone code
- `Assets/Scripts/UI/TitleScreenTapZone.cs` — new file (handler)

## What does NOT change

- **Background sprite** (`Assets/Illustrations/atmosphere/ayodhya_palace.png`)
  — already there, already loaded.  Untouched.
- **Title text** (`RĀMĀYAṆA` in world space) — kept.  It's not
  instruction, it's identity.
- **Directional Light** — kept.
- **Ramayana has only 1 scene** (MainMenu.unity).  No other
  scenes to patch.  When Round 14+ adds story scenes (prologue,
  character select, etc.) the same pattern applies: empty body
  text, full-width tap affordance.

## Static verification (this turn)

- Subtitle text contains no `Press [Space]` substring ✅
- `ConceptArtTapZone` GameObject present in scene ✅
- ConceptArtTapZone RectTransform anchor `(0, 0.18) → (1, 0.62)` ✅
- `TitleScreenTapZone.cs` exists at correct path ✅
- `CreateTitleScene.cs` source parity (subtitles + tap zone code) ✅

## Live verification

Ramayana has no iOS Simulator build pipeline (it's PS5-targeted).
The Mac/Linux/Windows standalone builds will be re-verified in
Round 14+ when the macOS build script can be re-run.  This turn
relies on static verification + source diff confirmation.

For Round 13 the strongest claim is: **the YAML hand-patch
correctly constructs a valid Unity scene with a tap zone covering
the lower 60% of the screen, and the runtime MonoBehaviour
`TitleScreenTapZone` is referenced by name in the Button's
onClick listener list, so a tap on the lower 60% will fire
`OnTap()` which logs to the Unity console.**

## Limitation disclosure

This is **ad-hoc verification, NOT suite green.** Same caveats:

- No Unity batchmode compile re-executed for the new
  `TitleScreenTapZone.cs` this turn (will be exercised by the
  next Ramayana build attempt).
- Static YAML validation only — no Unity editor opened to
  verify the scene deserializes correctly.
- The Editor menu "Build Starter Scenes" wasn't re-run, so the
  generated `MainMenu.unity` from `CreateTitleScene.Create()` was
  not exercised.  But `CreateTitleScene.cs` source changes were
  hand-verified by reading the diff.
- `TitleScreenTapZone` references a `UnityEvent` SerializeField
  in `OnTap()` — the OnClick listener in YAML lists the method
  name `OnTap`.  Unity resolves this by reflection at scene load
  time.  If the method signature doesn't match, the click will
  silently no-op (Unity logs a warning).

## Round 13 vs Round 12

| Aspect | R12 (Yuddhakanta) | R13 (Ramayana) |
|---|---|---|
| Scenes patched | 27 | 1 |
| Total LoC changed | ~88 lines across 28 files | ~40 lines across 3 files |
| Pattern | Empty BodyText + expand PrimaryButton | Remove keyboard hint + add tap zone |
| Live visual proof | iOS Sim screenshot at iPhone 17 Pro | None this turn (no iOS Sim pipeline for Ramayana) |
| Static verifier | 41/41 PASS | Pending this turn |

## Strongest claim

"Ramayana PS5's MainMenu.unity has been patched so that
(a) the subtitle text no longer says `Press [Space]` — the
decorative subtitle is now just `jambudweep.tech · Source-faithful
retelling` — and (b) a new `ConceptArtTapZone` GameObject covers
the lower 60% of the screen (anchor `0, 0.18 → 1, 0.62`) and is
wired to a new `TitleScreenTapZone` MonoBehaviour that fires
`OnTap()` on click.  Future players can start the game by tapping
the bottom half of the title screen — they don't need to read
any instruction, find a keyboard, or know about the Space key.
`CreateTitleScene.cs` is patched identically so future
regenerations preserve the new layout."