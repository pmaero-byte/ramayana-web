// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — CharacterData ScriptableObject
// C# port of `CharacterProfile` from types.ts (lines 277-294)
// Maps to 50 named characters from gameData.ts (50 ids, color-coded)
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Data
{
    [CreateAssetMenu(fileName = "Character_", menuName = "Ramayana/Character Data", order = 2)]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterId = "";
        public string characterName = "";
        public string tamilName = "";
        public string sanskritName = "";

        [Header("Role")]
        public string role = "";
        public VoiceRegister voice;
        public CharacterAlignment alignment;

        [Header("Visual")]
        [Tooltip("Hex color used for UI accent and dialogue nameplate")]
        public string colorHex = "#FFFFFF";
        public string icon = "";

        [Header("Dharma")]
        [Range(-100, 100)]
        public int dharmaAlignment = 0;

        [Header("Profile")]
        [TextArea(2, 6)] public string description = "";
        [TextArea(1, 4)] public string keyQuote = "";

        [Header("Relationships")]
        public List<CharacterRelationship> relationships = new List<CharacterRelationship>();

        [Header("Unlock")]
        [Tooltip("e.g. 'complete:rama-arc' or 'default:unlocked'")]
        public string unlockCondition = "default:unlocked";
        public bool unlockedByDefault = false;

        // ── Helpers ────────────────────────────────────────────────────

        public Color GetColor()
        {
            if (ColorUtility.TryParseHtmlString(colorHex, out var c)) return c;
            return Color.white;
        }
    }
}
