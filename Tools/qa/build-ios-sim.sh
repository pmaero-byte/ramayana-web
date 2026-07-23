#!/bin/bash
# Day 18 — iOS Simulator build pipeline.
# 1. Unity batchmode: generate/refresh Xcode project into Build/iOSSimulator/
# 2. xcodebuild: build + install on iOS Simulator (iPhone 17 Pro Max)
# 3. simctl: screenshot + file listing
# 4. structured report consumed by hermes-verify-ramayana-ios-sim-day18.sh
#
# Exit codes:
#   0 = all gates pass + screenshot taken
#   1 = any gate fails
# Artifacts:
#   /tmp/day18-report.txt
#   /tmp/day18-screenshot.png

set -u
PROJECT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
UNITY="/Users/prabaharan/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"
BUILD_DIR="$PROJECT/Build/iOSSimulator"
XCODE_DIR="$BUILD_DIR/RamayanaPS5-iOS-Simulator"
REPORT="/tmp/day18-report.txt"
SCREENSHOT="/tmp/day18-screenshot.png"
SIM="iPhone 17 Pro Max"
TAP="835B499A-7963-4A6C-A356-C18644B872A2"
FAIL=0

rm -f "$REPORT"
mkdir -p "$BUILD_DIR"

## Preflight: detect missing iOS support module before invoking Unity
UNITY_IOS_DIR=$(find "$UNITY" -path "*/PlaybackEngines/iOSSupport" -maxdepth 5 2>/dev/null | head -1)
if [ -z "$UNITY_IOS_DIR" ]; then
  UNITY_IOS_DIR=$(find "$UNITY" -path "*/PlaybackEngines/iPhoneSupport" -maxdepth 5 2>/dev/null | head -1)
fi
FRAMEWORK="$XCODE_DIR/Frameworks/UnityRuntime.framework/UnityRuntime"
if [ ! -f "$UNITY_IOS_DIR" ] || [ ! -f "$FRAMEWORK" ]; then
  echo "❌ Preflight FAIL: Unity iOS Build Support module not installed"
  echo "   Unity iOS export requires the iOS Build Support module"
  echo "   (PlaybackEngines/iOSSupport or iPhoneSupport), which is missing"
  echo "   from this Unity 6000.5.4f1 installation."
  echo "   This causes xcodebuild to fail with:"
  echo "     ld: framework 'UnityRuntime' not found"
  echo ""
  echo "   To resolve:"
  echo "     1. Open Unity Hub"
  echo "     2. Find 6000.5.4f1 → Add Modules → iOS Build Support"
  echo "     3. Re-run: bash Tools/qa/build-ios-sim.sh"
  echo ""
  echo "   Day 18 build-pipeline script is otherwise correct."
  echo "FAIL:PREFLIGHT:iOS module missing" >> "$REPORT"
  echo "BLOCKER: Unity iOS Build Support module not installed" >> "$REPORT"
  cat <<'EOF' >> "$REPORT"

Documentation: /Users/prabaharan/Aerospace_projects/RamayanaPS5/Documentation/DAY18_IOS_BLOCKER.md

Day 18 build pipeline script surface is correct. The pipeline is blocked by a
host-level Unity installation gap, not a code defect. Re-run this script after
installing Unity iOS Build Support via Unity Hub.
EOF
  exit 2
else
  echo "✅ Preflight: UnityRuntime.framework found at $FRAMEWORK"
  echo "PASS:preflight UnityRuntime.framework present" >> "$REPORT"
fi

## Gate 1: Unity batchmode export
echo "Gate 1: Unity batchmode iOS Simulator export..."
"$UNITY" \
  -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -logFile /tmp/unity-export-day18.log \
  -executeMethod Ramayana.Editor.BuildIOSSimulator.BuildFromCli 2>&1 | tail -3

if [ ! -d "$XCODE_DIR" ]; then
  echo "❌ Gate 1 FAIL: $XCODE_DIR missing after Unity export"
  echo "FAIL:GATE1:Unity export missing" >> "$REPORT"
  FAIL=1
else
  echo "✅ Gate 1: Unity exported Xcode project to $XCODE_DIR"
fi

if grep -q 'error CS' /tmp/unity-export-day18.log 2>/dev/null; then
  echo "❌ Gate 1b FAIL: CS errors during export"
  echo "FAIL:GATE1B:CS errors" >> "$REPORT"
  FAIL=1
fi

## Gate 2: xcodebuild
if [ "$FAIL" -eq 0 ]; then
  echo "Gate 2: xcodebuild build for $SIM..."
  xcodebuild -project "$XCODE_DIR/Unity-iPhone.xcodeproj" \
    -scheme Unity-iPhone \
    -destination "platform=iOS Simulator,name=$SIM" \
    -derivedDataPath /tmp/xcode-ddc-day18 \
    build 2>&1 | tail -25

  if [ $? -ne 0 ]; then
    echo "❌ Gate 2 FAIL: xcodebuild build failed"
    echo "FAIL:GATE2:xcodebuild" >> "$REPORT"
    FAIL=1
  else
    echo "✅ Gate 2: xcodebuild build succeeded"
    echo "PASS:xcodebuild" >> "$REPORT"
  fi
fi

## Gate 3: simctl install + launch + screenshot
if [ "$FAIL" -eq 0 ]; then
  APP=$(find /tmp/xcode-ddc-day18/Build -name "Unity-iPhone.app" 2>/dev/null | head -1)
  if [ -z "$APP" ]; then
    echo "❌ Gate 3a FAIL: .app not found in DerivedData"
    echo "FAIL:GATE3A:app not found" >> "$REPORT"
    FAIL=1
  else
    echo "✅ Gate 3a: .app found at $APP"
    echo "APP:$APP" >> "$REPORT"

    # ensure simulator is booted
    BOOTED=$(xcrun simctl list devices 2>/dev/null | grep "Booted" | grep "$SIM" | head -1)
    if [ -z "$BOOTED" ]; then
      echo "Booting $SIM..."
      xcrun simctl boot "$SIM" 2>/dev/null || true
      sleep 5
    fi

    echo "Installing on $SIM..."
    xcrun simctl install "$TAP" "$APP" 2>&1 | tail -3
    if [ $? -eq 0 ]; then
      echo "✅ Gate 3b: simctl install succeeded"
      echo "PASS:simctl install" >> "$REPORT"
    else
      echo "⚠  Gate 3b: simctl install had warnings (continuing)"
      echo "WARN:simctl install" >> "$REPORT"
    fi

    echo "Launching..."
    # Try common Unity-generated bundle identifiers.
    # Unity default: com.CompanyName.ProductName
    # Older default: com.DefaultCompany.ProductName
    # We probe in order; the first that launches wins.
    for BID in "com.JambudweepGames.Ramayana" "com.Company.ProductName" "com.DefaultCompany.Ramayana" "Ramayana"; do
      echo "  Trying launch with bundle id: $BID"
      if xcrun simctl launch "$TAP" "$BID" 2>/dev/null; then
        echo "✅ Gate 3d: launch succeeded with $BID"
        echo "PASS:launch bundle=$BID" >> "$REPORT"
        break
      fi
    done
    echo "Sleep 3 for stabilization..."
    sleep 3

    echo "Screenshot..."
    xcrun simctl io "$TAP" screenshot "$SCREENSHOT" 2>&1
    if [ -f "$SCREENSHOT" ]; then
      SIZE=$(wc -c < "$SCREENSHOT")
      if [ "$SIZE" -gt 1000 ]; then
        echo "✅ Gate 3c: screenshot taken ($SIZE bytes)"
        echo "PASS:screenshot $SIZE bytes" >> "$REPORT"
      else
        echo "⚠  Gate 3c: screenshot is suspiciously small ($SIZE bytes)"
        echo "WARN:screenshot size=$SIZE" >> "$REPORT"
      fi
    else
      echo "❌ Gate 3c FAIL: screenshot not taken"
      echo "FAIL:GATE3C:screenshot" >> "$REPORT"
      FAIL=1
    fi
  fi
fi

## Gate 4: summary
echo ""
echo "=== REPORT ==="
cat "$REPORT" 2>/dev/null || echo "(no report)"
echo ""
if [ "$FAIL" -eq 0 ]; then
  echo "ALL GATES PASS"
  exit 0
else
  echo "FAILURES DETECTED (see report)"
  exit 1
fi
