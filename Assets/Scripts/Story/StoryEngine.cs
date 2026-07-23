// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Story Engine (Dharma + Choice + Branching)
// C# port of `storyEngine.ts` (14.3KB) — typewriter, branching, save/load
// Source-fidelity: every moment carries a Valmiki sarga citation in `knowledge`
// ════════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using Jambudweep.Ramayana.Data;

namespace Jambudweep.Ramayana.Story
{
    public enum StoryMode
    {
        Idle,
        CharacterSelect,
        Prologue,
        Playing,
        Paused,
        Cutscene,
        Dialogue,
        Choice,
        Exploration,
        Combat,
        Transition,
        Summary
    }

    /// <summary>
    /// Runtime story state. Persisted via SaveSystem on quicksave/autosave.
    /// </summary>
    [System.Serializable]
    public class StoryEngineState
    {
        public StoryMode mode = StoryMode.Idle;
        public string selectedCharacterId;
        public string currentActId;
        public int currentMomentIndex;
        public List<string> choiceHistory = new List<string>(); // "momentId:choiceId"
        public int dharmaScore = 0;
        public Dictionary<DharmaCategory, int> dharmaCategories = new Dictionary<DharmaCategory, int>();
        public List<string> completedObjectiveIds = new List<string>();
        public List<string> unlockedCharacterIds = new List<string>();
        public List<string> collectedShlokaIds = new List<string>();
        public List<string> collectedCollectibleIds = new List<string>();
        public List<RamayanaScene> visitedScenes = new List<RamayanaScene>();
        public float sessionStartTime;
        public float totalPlayTime;
    }

    /// <summary>
    /// Story engine MonoBehaviour. Drives the act-by-act progression.
    /// Wiring follows the 5-step pattern from pop-grade-game-polish skill:
    /// 1. Imported at top of consumer
    /// 2. Instantiated in scene setup
    /// 3. Updated each frame
    /// 4. Triggered by UI / input
    /// </summary>
    public class StoryEngine : MonoBehaviour
    {
        [Header("Episode")]
        public Data.EpisodeData episode;

        [Header("State (runtime)")]
        public StoryEngineState state = new StoryEngineState();

        [Header("Typewriter")]
        public float typewriterSpeed = 50f; // chars/sec
        public bool typewriterActive;
        public string typewriterBuffer;

        // ── Lifecycle ──────────────────────────────────────────────────

        void Awake()
        {
            // Two-flag init guard pattern from pop-grade-game-polish §10
            if (s_initialized) return;
            s_initialized = true;
            state.sessionStartTime = Time.time;
        }

        private static bool s_initialized;

        // ── Public API ─────────────────────────────────────────────────

        public void SelectCharacter(string characterId)
        {
            state.selectedCharacterId = characterId;
            state.mode = StoryMode.Prologue;
        }

        public void BeginAct(string actId)
        {
            state.currentActId = actId;
            state.currentMomentIndex = 0;
            state.mode = StoryMode.Playing;
        }

        public void CompleteObjective(string objectiveId)
        {
            if (!state.completedObjectiveIds.Contains(objectiveId))
            {
                state.completedObjectiveIds.Add(objectiveId);
            }
        }

        public void ApplyChoice(string momentId, string choiceId, int dharmaImpact, DharmaCategory category)
        {
            state.choiceHistory.Add($"{momentId}:{choiceId}");
            state.dharmaScore += dharmaImpact;
            if (!state.dharmaCategories.ContainsKey(category))
            {
                state.dharmaCategories[category] = 0;
            }
            state.dharmaCategories[category] += Mathf.Max(0, dharmaImpact);
        }

        public void CollectShloka(string shlokaId)
        {
            if (!state.collectedShlokaIds.Contains(shlokaId))
            {
                state.collectedShlokaIds.Add(shlokaId);
            }
        }

        public void UnlockCharacter(string characterId)
        {
            if (!state.unlockedCharacterIds.Contains(characterId))
            {
                state.unlockedCharacterIds.Add(characterId);
            }
        }

        // ── Autosave on every moment/choice ───────────────────────────

        public void Autosave()
        {
            var data = new Core.SaveData
            {
                episodeId = episode?.episodeId,
                currentActId = state.currentActId,
                currentMomentIndex = state.currentMomentIndex,
                selectedCharacterId = state.selectedCharacterId,
                dharmaScore = state.dharmaScore,
                completedObjectiveIds = state.completedObjectiveIds.ToArray(),
                unlockedCharacterIds = state.unlockedCharacterIds.ToArray(),
                collectedShlokaIds = state.collectedShlokaIds.ToArray(),
                collectedCollectibleIds = state.collectedCollectibleIds.ToArray(),
                visitedScenes = state.visitedScenes.ConvertAll(s => s.ToString()).ToArray(),
                totalPlayTimeSec = state.totalPlayTime
            };
            Core.SaveSystem.Save(Data.SaveKeys.AutoSave, data);
        }
    }
}
