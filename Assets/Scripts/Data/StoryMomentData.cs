// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — StoryMomentData ScriptableObject
// C# port of `StoryMoment` from types.ts (lines 213-235)
// Story-first architecture — single narrative beat
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Data
{
    [Serializable]
    public struct StoryBranch
    {
        public string condition; // e.g. "dharma>50" or "chose:mercy"
        public string momentId;  // destination
    }

    [CreateAssetMenu(fileName = "Moment_", menuName = "Ramayana/Story Moment", order = 3)]
    public class StoryMomentData : ScriptableObject
    {
        [Header("Identity")]
        public string momentId = "";
        public StoryMomentType type;
        public RamayanaScene scene;

        [Header("Cast")]
        [Tooltip("Character IDs present in this moment")]
        public List<string> characters = new List<string>();
        public string speaker = "Narrator";
        public VoiceRegister voice;

        [Header("Text")]
        [TextArea(3, 12)] public string text = "";
        public string tamilText = "";

        [Header("Shloka (for type=Shloka)")]
        [TextArea(2, 6)] public string sanskritShloka = "";
        [TextArea(2, 6)] public string transliteration = "";
        [TextArea(2, 6)] public string shlokaTranslation = "";

        [Header("Branching")]
        public List<RamayanaChoice> choices = new List<RamayanaChoice>();
        public CameraMove camera;
        public MusicCue music;

        [Header("World")]
        public Vec3 position;
        public string nextMomentId;
        public List<StoryBranch> branchTo;

        [Header("Metadata")]
        public float durationMs;
        [Tooltip("Historical/cultural fact learned — used by KnowledgeTracker")]
        [TextArea(2, 6)] public string knowledge = "";
        public EmotionalTone emotionalTone;
        public string collectible;
    }
}
