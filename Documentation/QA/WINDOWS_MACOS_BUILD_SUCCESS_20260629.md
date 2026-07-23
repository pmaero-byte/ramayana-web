# Ramayana PS5 — First Build Success (Windows + macOS)

**Date:** 2026-06-29 11:13
**Editor:** Unity **6000.3.18f1** (just pinned this round — previously no ProjectVersion.txt)

## What was done this round

1. **Pinned Unity version** — `ProjectSettings/ProjectVersion.txt` was missing.
   Opened the project in Unity 6 — it auto-wrote the file with `6000.3.18f1 (5ebeb53e4c07)`.
2. **Created placeholder scene** — Project had `EditorBuildSettings.scenes: []`.
   Wrote `Assets/Editor/CreatePlaceholderScene.cs` — creates `Assets/Scenes/MainMenu.unity`
   (Camera + Directional Light) and registers it as the first enabled build scene.
3. **Wrote build scripts** — `BuildWindows64.cs` + `BuildMacOS.cs` (same pattern as
   Yuddhakanta/Civaka, namespace `Ramayana.Editor`).
4. **Built Windows** — `result=Succeeded`, 99 MB total, 19s.
5. **Built macOS** — `result=Succeeded`, 117 MB .app, 34s.

## Build results

```
[BuildWindows64] result=Succeeded totalSize=99,058,997 totalTime=19.5s outputPath=...RamayanaPS5.exe
[BuildMacOS]    result=Succeeded totalSize=116,740,109 totalTime=34.0s outputPath=...RamayanaPS5.app
```

## Artifacts

### Windows (`Build/Windows64/`)

```
RamayanaPS5.exe                 667,136 bytes   PE32+ GUI x86-64
UnityPlayer.dll              36,587,952 bytes   36 MB main runtime
UnityCrashHandler64.exe       1,621,936 bytes
RamayanaPS5_Data/                                179 files, 1 scene
├── sharedassets0.assets
├── level0
├── resources.assets
├── Managed/                                      ~120 Mono assemblies
└── MonoBleedingEdge/                             Mono runtime
D3D12/
RamayanaPS5_BurstDebugInformation_DoNotShip/
```
**Total: 95 MB on disk, 245 files**

### macOS (`Build/macOS/RamayanaPS5.app/`)

```
Contents/
├── Info.plist
├── MacOS/RamayanaPS5   Mach-O **universal** (x86_64 + arm64)
├── Resources/Data/     175 files, 1 scene (sharedassets0 + level0)
├── MonoBleedingEdge/   Mono runtime
└── _CodeSignature/
```
**Total: 112 MB on disk**

## Verifiers

| Script | Result |
|---|---|
| `Tools/qa/hermes-verify-ramayana-windows-build.sh` | **8/8 pass** |
| `Tools/qa/hermes-verify-ramayana-macos-build.sh`   | **9/9 pass**  |

## Pitfalls hit this round

1. **`totalScenes` is not a field on `BuildSummary`** in Unity 6 (only `BuildReport.scenesUsingAssets`).
   Removed — `LogReport` no longer references it.
2. **Unity refuses to build with `result=Unknown` and "Cannot build untitled scene"** if `BuildPipeline.BuildPlayer`
   is called with an empty scene array — it creates an untitled scene which has no path → no .exe.
   Fix: ensure at least one scene exists in `EditorBuildSettings.scenes` before building.
3. **No ProjectVersion.txt** — fixed automatically by opening in Unity 6.

## Project reality check (this session)

| Asset class | Count | Source |
|---|---|---|
| C# files in `Assets/` | 11 | `find Assets -name '*.cs'` |
| Total LoC | 1,534 | `wc -l` |
| PNG art in `Assets/` | 148 | `find Assets -name '*.png'` |
| Scenes at start | 0 | `find Assets -name '*.unity'` |
| Scenes now | 1 (`MainMenu.unity`) | `find Assets -name '*.unity'` |
| EditorBuildSettings.scenes | 1 (MainMenu) | `ProjectSettings/EditorBuildSettings.asset` |
| Unity 2022.3.15f1 license activation | Still blocked | `Unity_v2022.3.15f1.alf` legacy format |

## Status

**3 of 4 Unity games now build on 3+ non-console platforms** (Yuddhakanta, Civaka, Ramayana PS5).
**Hertree Traders_3D** still blocked by Unity 2022 license (`.alf` legacy format).
PS5 + Xbox GDKs still need user-side partner-program applications.
