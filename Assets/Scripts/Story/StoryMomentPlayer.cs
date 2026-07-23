// Day 2 + Day 9 (RamayanaPS5) — StoryMomentPlayer.
// Loads a single act (kanda) from corpus_data.json and walks its
// objectives[] array. Day 9 plumbs character portraits into DialogueOverlay
// via PortraitResolver so Mac GTA dialogue panels show faces.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.UI;

namespace Jambudweep.Ramayana.Story
{
    public sealed class StoryMomentPlayer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private string corpusResourcePath = "corpus_data";

        [Header("Optional wiring")]
        [SerializeField] private StoryEngine storyEngine;
        [SerializeField] private DialogueOverlay overlay;

        [Header("Timing")]
        [SerializeField, Min(0.5f)] private float cueHoldSeconds = 2.4f;
        [SerializeField, Min(0.5f)] private float completedHoldSeconds = 3.0f;

        [Header("Events")]
        public UnityEvent<string> onObjectiveEntered = new UnityEvent<string>();
        public UnityEvent<string> onObjectiveCompleted = new UnityEvent<string>();
        public UnityEvent<string> onActCompleted = new UnityEvent<string>();

        private ActRecord _currentAct;
        private int _objectiveIndex = -1;
        private string _currentObjectiveId;

        [Serializable]
        private class CorpusData { public List<ActRecord> acts; }

        [Serializable]
        public class ActRecord
        {
            public string actId;
            public int actNumber;
            public string title;
            public string location;
            public string setup;
            public string lesson;
            public string playerRole;
            public List<ObjectiveRecord> objectives;
        }

        [Serializable]
        public class ObjectiveRecord
        {
            public string id;
            public string type;
            public string title;
            public string marker;
            public string cue;
            public string actionLabel;
            public string citation;
            public CompletedLineRecord completedLine;
            public Vec3 position;
        }

        [Serializable]
        public class CompletedLineRecord
        {
            public string speaker;
            public string text;
        }

        void Awake()
        {
            if (storyEngine == null) storyEngine = FindFirstObjectByType<StoryEngine>();
            if (overlay == null) overlay = DialogueOverlay.EnsureCreated();
            PortraitResolver.EnsureCreated();
        }

        // ── Public API ─────────────────────────────────────────────

        public bool LoadAct(string actId)
        {
            var data = LoadCorpus();
            if (data == null || data.acts == null) return false;
            foreach (var act in data.acts)
            {
                if (act != null && act.actId == actId)
                {
                    _currentAct = act;
                    _objectiveIndex = -1;
                    storyEngine?.BeginAct(actId);
                    Debug.Log($"[StoryMomentPlayer] Loaded act: {actId} ({act.objectives?.Count ?? 0} objectives)");
                    // Kick first cue automatically so Mac playtest sees dialogue + portrait.
                    AdvanceObjective();
                    return true;
                }
            }
            Debug.LogWarning($"[StoryMomentPlayer] Act not found: {actId}");
            return false;
        }

        public string CurrentActId => _currentAct?.actId;
        public int ObjectiveIndex => _objectiveIndex;
        public string CurrentObjectiveId => _currentObjectiveId;
        public int TotalObjectives => _currentAct?.objectives?.Count ?? 0;
        public ObjectiveRecord CurrentObjective =>
            (_currentAct?.objectives != null
             && _objectiveIndex >= 0
             && _objectiveIndex < _currentAct.objectives.Count)
                ? _currentAct.objectives[_objectiveIndex]
                : null;

        public bool AdvanceObjective()
        {
            if (_currentAct == null || _currentAct.objectives == null) return false;
            int next = _objectiveIndex + 1;
            if (next >= _currentAct.objectives.Count)
            {
                Debug.Log($"[StoryMomentPlayer] Act complete: {_currentAct.actId}");
                onActCompleted?.Invoke(_currentAct.actId);
                return false;
            }
            _objectiveIndex = next;
            _currentObjectiveId = _currentAct.objectives[next].id;
            onObjectiveEntered?.Invoke(_currentObjectiveId);
            storyEngine?.CompleteObjective(_currentObjectiveId);
            StartCoroutine(PlayCueRoutine(_currentAct.objectives[next]));
            return true;
        }

        public void CompleteCurrentObjective()
        {
            var obj = CurrentObjective;
            if (obj == null) return;
            string id = obj.id;
            if (overlay != null && obj.completedLine != null)
            {
                string speaker = obj.completedLine.speaker ?? "Narrator";
                overlay.Show(
                    speaker,
                    obj.completedLine.text ?? "",
                    null,
                    ResolvePortrait(speaker),
                    completedHoldSeconds);
            }
            onObjectiveCompleted?.Invoke(id);
            StartCoroutine(DelayedAdvance(completedHoldSeconds));
        }

        /// <summary>Day 9 — resolve speaker id to a portrait sprite.</summary>
        public Sprite ResolvePortrait(string speaker)
        {
            return PortraitResolver.Resolve(speaker);
        }

        // ── Internals ──────────────────────────────────────────────

        private IEnumerator PlayCueRoutine(ObjectiveRecord obj)
        {
            if (overlay != null)
            {
                // Prefer playerRole for the act hero face; fall back to marker.
                string speaker = !string.IsNullOrEmpty(_currentAct?.playerRole)
                    ? _currentAct.playerRole
                    : (obj.marker ?? "Objective");
                overlay.Show(
                    obj.marker ?? "Objective",
                    obj.cue ?? obj.title ?? "",
                    null,
                    ResolvePortrait(speaker),
                    cueHoldSeconds);
            }
            yield return null;
        }

        private IEnumerator DelayedAdvance(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            AdvanceObjective();
        }

        private CorpusData LoadCorpus()
        {
            var ta = Resources.Load<TextAsset>(corpusResourcePath);
            if (ta == null)
            {
                Debug.LogError($"[StoryMomentPlayer] corpus not found at Resources/{corpusResourcePath}");
                return null;
            }
            try { return JsonUtility.FromJson<CorpusData>(ta.text); }
            catch (Exception e)
            {
                Debug.LogError($"[StoryMomentPlayer] corpus parse failed: {e.Message}");
                return null;
            }
        }
    }
}
