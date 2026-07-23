// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — ActData ScriptableObject
// C# port of `RamayanaAct` from types.ts (lines 412-435) + gameData.ts acts
// Unity 2022.3 LTS pattern: [CreateAssetMenu] for editor generation
// Reference: unity-ps5-patterns.md "ScriptableObject Pattern for Data"
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Data
{
    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 6)] public string text;
        public VoiceRegister voice;
    }

    [Serializable]
    public struct ObjectiveData
    {
        public string id;
        public ObjectiveType type;
        public string title;
        public string marker;
        [TextArea(2, 4)] public string cue;
        public string actionLabel;
        public ObjectiveCompletedLine completedLine;
        public ObjectiveChoice[] choices;
        public int target;
        public Vec3 position;
    }

    [Serializable]
    public struct ActReward
    {
        public string badge;
        [TextArea(2, 6)] public string lore;
        public int dharma;
    }

    [CreateAssetMenu(fileName = "Act_", menuName = "Ramayana/Act Data", order = 1)]
    public class ActData : ScriptableObject
    {
        [Header("Identity")]
        public string actId = "";
        public int actNumber = 0;
        public string title = "";
        public string location = "";

        [Header("Scene")]
        public RamayanaScene scene;

        [Header("Narrative")]
        [TextArea(3, 10)] public string setup = "";
        [TextArea(2, 6)] public string lesson = "";
        public string playerRole = "";

        [Header("Objectives")]
        public List<ObjectiveData> objectives = new List<ObjectiveData>();

        [Header("Dialogue")]
        public List<DialogueLine> dialogue = new List<DialogueLine>();

        [Header("Source-Fidelity Markers")]
        [Tooltip("Consequence echoes — dharma choices ripple into later acts")]
        public List<ConsequenceEcho> consequenceEchoes = new List<ConsequenceEcho>();

        [Tooltip("Shloka stones — sacred verses placed in 3D world")]
        public List<ShlokaStone> shlokaStones = new List<ShlokaStone>();

        [Tooltip("Hidden collectibles — murti fragments, temple stamps, relics")]
        public List<HiddenCollectible> hiddenCollectibles = new List<HiddenCollectible>();

        [Header("Reward")]
        public ActReward reward;

        // ── Helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Total objectives for progress tracking.
        /// </summary>
        public int ObjectiveCount => objectives?.Count ?? 0;

        /// <summary>
        /// Find an objective by ID (used by trigger system).
        /// </summary>
        public ObjectiveData? FindObjective(string id)
        {
            if (objectives == null) return null;
            foreach (var o in objectives)
            {
                if (o.id == id) return o;
            }
            return null;
        }

        /// <summary>
        /// All choice objectives (used by dharma scoring system).
        /// </summary>
        public IEnumerable<ObjectiveData> ChoiceObjectives
        {
            get
            {
                if (objectives == null) yield break;
                foreach (var o in objectives)
                {
                    if (o.type == ObjectiveType.Choice) yield return o;
                }
            }
        }
    }
}
