// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — EpisodeData ScriptableObject (root container)
// C# port of `RamayanaEpisode` from types.ts (lines 450-457)
// Holds the full 8-act episode + 50 characters + scenes
// ════════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Data
{
    [CreateAssetMenu(fileName = "Episode_Ramayana", menuName = "Ramayana/Episode (Root)", order = 0)]
    public class EpisodeData : ScriptableObject
    {
        [Header("Episode Identity")]
        public string episodeId = "ramayana-valmiki";
        public string title = "Ramayana — The Journey of Rama";
        [TextArea(2, 4)] public string subtitle = "";
        public string sourcePath = "divine-lens-content/stories/ramayana-valmiki.md";

        [Header("Acts (8 kandas)")]
        [Tooltip("Act_1_Bala through Act_8_Return")]
        public List<ActData> acts = new List<ActData>();

        [Header("Characters (50 named)")]
        public List<CharacterData> characters = new List<CharacterData>();

        [Header("Dharma Score Defaults")]
        public int startingDharma = 0;
        public DharmaCategory primaryCategory = DharmaCategory.Duty;

        // ── Helpers ────────────────────────────────────────────────────

        public ActData FindAct(string actId)
        {
            if (acts == null) return null;
            foreach (var a in acts)
            {
                if (a != null && a.actId == actId) return a;
            }
            return null;
        }

        public CharacterData FindCharacter(string characterId)
        {
            if (characters == null) return null;
            foreach (var c in characters)
            {
                if (c != null && c.characterId == characterId) return c;
            }
            return null;
        }

        public int TotalObjectives
        {
            get
            {
                int sum = 0;
                foreach (var a in acts)
                {
                    if (a != null) sum += a.ObjectiveCount;
                }
                return sum;
            }
        }
    }
}
