# Ramayana PS5 — Linux64 Build Success (3rd platform)

**Date:** 2026-06-29 11:26
**Editor:** Unity 6000.3.18f1
**Backend:** Mono (Linux Mono module was installed in Round 3 for Yuddhakanta)

## Result

```
[BuildLinux64] result=Succeeded totalSize=94,672,729 totalTime=00:00:36.02 outputPath=...RamayanaPS5.x86_64
```

## Artifact

```
Build/Linux64/
├── RamayanaPS5.x86_64          4,472 bytes    ELF 64-bit LSB executable, x86-64, dynamically linked
├── UnityPlayer.so          41,414,832 bytes    ELF 64-bit LSB shared object
├── RamayanaPS5_Data/                          196 files, 1 scene
├── libdecor-0.so.0         45,536 bytes
├── libdecor-cairo.so       69,664 bytes
└── RamayanaPS5_BurstDebugInformation_DoNotShip/
```

## Verifier

`Tools/qa/hermes-verify-ramayana-linux-build.sh` — **11/11 pass:**
- ✅ RamayanaPS5.x86_64 exists (4,472 bytes)
- ✅ UnityPlayer.so exists (41 MB)
- ✅ RamayanaPS5_Data/ exists
- ✅ Executable is ELF 64-bit x86-64
- ✅ Dynamically linked
- ✅ UnityPlayer.so is ELF shared object
- ✅ 1 sharedassets + 1 level file + 196 Data files
- ✅ `resources.assets` present (Linux main data bundle)
- ✅ `globalgamemanagers` present

## Comparison: Ramayana across all 3 platforms

| Platform    | Build time | Total size | Format               |
|-------------|-----------:|-----------:|----------------------|
| Windows64   | 19s        | 95 MB      | PE32+ x86-64         |
| macOS       | 34s        | 112 MB     | Mach-O universal     |
| **Linux64** | **36s**    | **95 MB**  | **ELF x86-64**       |

## Civaka Windows rebuild (Round 6 bonus)

Discovered `/Other_projects/civaka-cintamani-ps5/unity/Build/Windows64/` was empty
(prev session's `Build/` got gitignored + cleaned). Rebuilt via existing
`Civaka.Editor.BuildWindows64.Build` (note: method is `Build`, NOT `BuildFromCli`).

**Civaka → Windows:** 186 MB at `/Other_projects/civaka-cintamani-ps5/Build/Windows64/` (one level up from `unity/`),
PE32+ x86-64. This directory is OUTSIDE `unity/` so it's NOT gitignored — survives cleanups.

## Status

**3 of 4 Unity games now build on 3+ non-console platforms.** Only Hertree Traders_3D
remains, blocked by Unity 2022 license.
