// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — DualSense + Tempest 3D Audio + Trophy Shims
// Reference: unity-ps5-patterns.md lines 31-87
// ════════════════════════════════════════════════════════════════════════════

using UnityEngine;

namespace Jambudweep.Ramayana.Platform
{
    public enum TriggerSide { Left, Right }

    /// <summary>
    /// PS5 DualSense integration. Adaptive triggers for bow-draw resistance,
    /// lightbar for dharmic acts (gold) vs war acts (red), haptics on key moments.
    /// Reference: unity-ps5-patterns.md lines 31-48
    /// </summary>
    public class DualSense : MonoBehaviour
    {
        [Header("Lightbar (game-color indicator)")]
        public Color dharmicColor = new Color(212, 164, 67); // gold #D4A443
        public Color warColor = new Color(200, 50, 30);       // red

        [Header("Haptics")]
        [Range(0f, 1f)] public float defaultHapticStrength = 0.3f;

        public void SetTriggerResistance(TriggerSide side, float startPos, float endPos)
        {
#if UNITY_PS5
            // UnityEngine.PS5.PS5Input.SetTriggerEffect(
            //     0,
            //     UnityEngine.PS5.PS5Input.TriggerEffect.CreateResistance(
            //         side == TriggerSide.Left
            //             ? UnityEngine.PS5.PS5Input.TriggerEffect.Trigger.Left
            //             : UnityEngine.PS5.PS5Input.TriggerEffect.Trigger.Right,
            //         startPos, endPos
            //     )
            // );
#endif
        }

        public void SetVibration(float lowFreq, float highFreq)
        {
#if UNITY_PS5
            // UnityEngine.PS5.PS5Input.SetVibration(0, lowFreq, highFreq);
#endif
        }

        public void SetLightbar(Color c)
        {
#if UNITY_PS5
            // UnityEngine.PS5.PS5Input.SetLightBar(0, (int)(c.r*255), (int)(c.g*255), (int)(c.b*255));
#endif
        }
    }

    /// <summary>
    /// 31-trophy set: 1 platinum, 3 gold, 9 silver, 18 bronze
    /// Reference: ps5-certification-checklist.md line 13
    /// Bronze = per-act completion (8 acts)
    /// Silver = per-character arc completion (10 unlocked chars)
    /// Gold = completionist + dharma master
    /// Platinum = all gold
    /// </summary>
    public static class TrophySystem
    {
        public const string TrophyBronze = "ramayana.bronze.{0}";    // act-{n}
        public const string TrophySilver = "ramayana.silver.{0}";    // char-{id}
        public const string TrophyGoldDharmaMaster = "ramayana.gold.dharma-master";
        public const string TrophyGoldCompletionist = "ramayana.gold.completionist";
        public const string TrophyGoldAllShlokas = "ramayana.gold.all-shlokas";
        public const string TrophyPlatinum = "ramayana.platinum";

        public static void Unlock(string trophyId, int progressPct = 100)
        {
#if UNITY_PS5
            // UnityEngine.PS5.PS5Trophy.UnlockTrophy(trophyId);
            // if (progressPct < 100) UnityEngine.PS5.PS5Trophy.SetTrophyProgress(trophyId, progressPct);
#elif UNITY_GAMECORE_XBOXSERIES
            // Microsoft.Xbox.Services.Achievements
#endif
            Debug.Log($"[Trophy] Unlocked {trophyId} @ {progressPct}%");
        }
    }

    /// <summary>
    /// Tempest 3D Audio — 14 virtual speakers on PS5
    /// Reference: ps5-certification-checklist.md line 19
    /// Reference: ps5 ProjectSettings audio3dVirtualSpeakerCount: 14
    /// </summary>
    public static class TempestAudio
    {
        public const int VirtualSpeakerCount = 14;

        public static void ConfigureTempest()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.speakerMode = AudioSpeakerMode.Surround;
            config.numRealVoices = 32;
            config.numVirtualVoices = 64;
            AudioSettings.Reset(config);
        }
    }
}
