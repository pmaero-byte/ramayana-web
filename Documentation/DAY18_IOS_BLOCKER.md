# Day 18 Blocker — Unity iOS Build Support Module Missing

**Date:** 2026-07-22  
**Status:** Active — Day 18 build pipeline complete but blocked by host setup  
**Severity:** Host-level install gap, not a code defect in RamayanaPS5

## Symptom

`bash Tools/qa/build-ios-sim.sh` fails during xcodebuild with:

```
ld: framework 'UnityRuntime' not found
clang++: error: linker command failed with exit code 1
** BUILD FAILED **
```

The Xcode project is generated correctly, but `Frameworks/UnityRuntime.framework/`
is empty (only `Info.plist` + `PrivacyInfo.xcprivacy`, no binary). Xcode cannot
find the Unity runtime binary, so `UnityFramework` target fails to link.

## Root Cause

This Unity 6000.5.4f1 instance is missing the **iOS Build Support** playback engine:

```
/Users/prabaharan/Unity/Hub/Editor/6000.5.4f1/PlaybackEngines/
  MacStandaloneSupport/    ← present
  WebGLSupport/            ← present
  iOSSupport/              ← MISSING
  iPhoneSupport/           ← MISSING
```

Without that module, Unity's iOS exporter cannot copy the real
`UnityRuntime.framework` binary into the Xcode project's `Frameworks/` folder.

## Resolution

1. Open **Unity Hub**
2. Find **6000.5.4f1** → click the three-dot menu → **Add Modules**
3. Select **iOS Build Support** (includes `UnityRuntime.framework`)
4. Re-run:

   ```bash
   cd /Users/prabaharan/Aerospace_projects/RamayanaPS5
   bash Tools/qa/build-ios-sim.sh
   ```

5. After install, rebuild + verify:
   ```bash
   xcrun simctl io "835B499A-7963-4A6C-A356-C18644B872A2" screenshot /tmp/day18-screenshot.png
   open /tmp/day18-screenshot.png
   ```

## What Day 18 Already Shipped (no code change needed)

- `Tools/qa/build-ios-sim.sh` — 4-gate pipeline (Unity export → xcodebuild → simctl install → screenshot)
- `Tools/qa/hermes-verify-ramayana-ios-sim-day18.sh` — 31-check verifier
- Preflight gate in `build-ios-sim.sh` that detects the missing module and
  exits with a clear remediation message instead of cascading into linker failure

## Unblocks

Once **iOS Build Support** is installed via Unity Hub:

- Re-run `bash Tools/qa/build-ios-sim.sh`
- Verify screenshot at `/tmp/day18-screenshot.png`
- Confirm `Assembly-CSharp.dll` from Day 17 still compiles (DLL md5
  `9e621f3941a4c5a201af1ce43652b608`)
- Day 18 can advance to suite green
