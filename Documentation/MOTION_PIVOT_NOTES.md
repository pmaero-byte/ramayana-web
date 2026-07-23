# Motion Pivot Plan — Ramayana PS5

*Created 2026-07-01 as part of the cross-game Round 8 motion pivot. Companion
to `Aerospace_projects/MB_Yuddhakanta/Documentation/MOTION_LAYER_API.md`.*

## Why Ramayana needs the pivot (most)

Ramayana's `StoryEngineState` (see `Story/StoryEngine.cs`) exposes
`StoryMode.Playing, Paused, Cutscene, Dialogue, Choice, Exploration, Combat,
Transition, Summary` — but in practice the runtime spends most of its time in
**Dialogue** and **Choice**, with **Cutscene** for camera-pan text-reveal.
Look at `StoryMomentData.cs`: every field is text (`text`, `tamilText`,
`sanskritShloka`, `transliteration`, `translation`). There is no `Transform`,
no `Collider`, no verb field. The story engine is text-first.

The model name itself says it: `StoryMomentData` is a *moment* — a
typewriter-text beat. The pivot is to introduce **motion-first** beats where
the player walks / drives / fights and the dialogue is layered on top via
`MotionTriggeredDialogue`. Not the inverse.

## Concrete port steps (Sprint B)

### Step 1 — Copy 5 contract files from Yuddhakanta (no edits needed)

```
FROM /Users/prabaharan/Aerospace_projects/MB_Yuddhakanta/Assets/Scripts/
  Motion3D/ThirdPersonMotionController.cs       → Assets/Scripts/Motion3D/
  Motion3D/AnalogJoystick3D.cs                  → Assets/Scripts/Motion3D/
  Motion3D/CinematicThirdPersonCamera.cs        → Assets/Scripts/Motion3D/
  Motion3D/MotionTriggeredDialogue.cs           → Assets/Scripts/Motion3D/
  UI/DialogueOverlay.cs                         → Assets/Scripts/UI/
```

The files use generic UnityEngine + UnityEngine.UI APIs — no Yuddhakanta-specific
references — so they should compile in Ramayana as-is.

### Step 2 — Update namespaces

Yuddhakanta namespaces use `Jambudweep.Mahabharata.Yuddhakanta.Motion3D` and
`Yuddhakanta.UI`. Ramayana should use `Jambudweep.Ramayana.Motion3D` and
`Ramayana.UI`. The files don't reference each other across namespaces — only
`MotionTriggeredDialogue` → `DialogueOverlay`. Adjust both at port time.

Also update the `Motion3D/MotionTriggeredDialogue.cs` line:
```
using Jambudweep.Mahabharata.Yuddhakanta.UI;
```
to
```
using Jambudweep.Ramayana.UI;
```

### Step 3 — Expand `StoryMomentType` enum

Current `Assets/Scripts/Data/RamayanaTypes.cs` line 27:

```csharp
public enum StoryMomentType
{
    Cutscene,    Dialogue,    Choice,
    Exploration, Witness,     Combat,
    Reflection,  Transition,  Shloka
}
```

Add (keeping existing values for back-compat):

```csharp
public enum StoryMomentType
{
    // ... existing ...
    MotionTraversal,   // 3D joystick walk through a scene; dialogue overlays
    MotionChariot,     // RAMA's chariot combat — bow, drive, parry
    MotionBowDuel,     // Rama vs. Ravana climax
    MotionWitnessWalk, // Slow walk-through; VO overlays (Stri-equivalent)
    MotionStealth,     // Hanuman in Lanka (cloaked / limited vision)
}
```

### Step 4 — Expand `StoryEngineMode` enum

In `StoryEngine.cs` line 13:

```csharp
public enum StoryMode
{
    // ... existing ...
    MotionTraversal, MotionChariot, MotionBowDuel,
    MotionWitnessWalk, MotionStealth, MotionLocked
}
```

`MotionLocked` is the cinematic that pauses input (kicks in during dramatic
crescendos, e.g. Ravana reveal).

### Step 5 — Add motion-verb field to `StoryMomentData`

In `Data/StoryMomentData.cs` after the existing `[Header]`:

```csharp
[Header("Motion")]
public SceneRoute sceneRoute;        // see Navigation/
[Tooltip("World-space Transform the player must walk to during this moment.")]
public Transform targetWaypoint;
[Tooltip("If true, dialogue audio plays as overlay during motion; player is NOT stopped.")]
public bool motionOverlayDialogue = true;
[Tooltip("Override for movement speed during this moment. 1=normal, 0.4=witness walk, 1.4=run.")]
public float motionSpeedScale = 1f;
```

This makes every story moment **optionally** motion-driven. Story content teams
can choose: keep this moment as text-first, OR mark it as motion-first and the
runtime will pop the joystick UI + active rig.

### Step 6 — Optional: copy Yuddhakanta's `Battle/` scripts

Only if Ramayana has chariot/bow gameplay (the Rama-Ravana bow duel is the
lead sequence). Scripts to copy if applicable:

```
Battle/TouchChariotController.cs
Battle/BowDragController.cs
Battle/ArrowProjectile.cs
```

These need namespace renames only.

### Step 7 — Build a single motion prototype scene

Use Yuddhakanta's `Embassy3DPrototype.unity` as the structural template. For
Ramayana, model it after Ayodhya's court or Kishkindha's throne room. Wire:

- Player GO (CharacterController + ThirdPersonMotionController)
- CinematicThirdPersonCamera on main Camera
- AnalogJoystick3D on canvas (bottom-left)
- DialogueOverlay on canvas (top-center)
- A few `MotionTriggeredDialogue` triggers at speakers (Bharata at the throne,
  Kaikeyi at her chambers, Rama at the ashram, etc.)

This becomes the first "playable act 1" prototype.

### Step 8 — Update AGENTS.md

Add a "Direction change" block matching Yuddhakanta's, pointing at this doc
and the motion-layer API doc.

## What stays reading-and-acting (and why)

Not everything needs motion. The Ramayana corpus has these text-first moments
that *should* remain dialogue:

- Most `Shloka` moments — sacred verses must appear text-first with transliteration
  + translation; motion would distract
- `Reflection` moments — internal monologue of Rama, Sita — these are *meant to be read slowly*
- `Choice` moments — moral crossroads that require the player to read, reflect,
  and choose; motion overlay would trivialize the choice

The pivot is *additive* — these moments gain a new flag `motionOverlayDialogue=false`
so the player walks to a quiet spot and the text fills the screen rather than
the corner. No moment becomes *less* readable. The default flips for traversal,
combat, and witness beats.

## Ramayana-specific ext calls (decided later)

- **Mace/bow combat** — only relevant in the Lanka bow-duel climax (Rama vs. Ravana).
  Don't port Yuddhakanta's chariot controller, port only BowDragController + a new
  BowDuel3D script.
- **Forest traversal** — Dandaka forest needs procedural tree generation + path
  choice. Not a Yuddhakanta pattern.
- **Ocean crossing** — Rama's bridge to Lanka could be 2.5D instead of 3D. Decide
  per level.

## Timeline estimate

After Yuddhakanta lands sprint A (motion pivot prototype compiles), Ramayana's
Sprint B is:
- Step 1–5 port: 2 days (copy + namespace renames + enum extensions)
- Step 7 build motion prototype scene: 4 days
- Step 8 docs: 0.5 day

**6.5 working days** to a working Rama-walks-Ayodhya prototype, assuming
Yuddhakanta motion layer is rock-solid.
