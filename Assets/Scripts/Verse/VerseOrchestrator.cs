// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Verse Orchestrator (Day 14)
// Consumes the Day 12 Bala Kanda moments corpus + Day 13 voice cue catalog
// + Day 11 character roster, and plays them as a sequential verse loop.
// Parallel to the existing StoryMomentPlayer (which consumes the Day 1-10
// corpus_data.json Act/Objective shape) — does NOT modify Story/Story*.cs.
// ════════════════════════════════════════════════════════════════════════════
//
// Lifecycle:
//   1. EnsureCreated() called from PlayerSceneBootstrap (Day 8)
//   2. LoadCorpora() async-loads Resources/Ramayana/{characters,moments_bala_kanda,voices}.json
//   3. Play(kanda="bala-kanda") walks moments in order, fires UnityEvents
//   4. Advance() / Skip() / Stop() public API for caller-driven control
//
// Wiring (UnityEvent hooks — no hardcoded references):
//   onVerseLoaded:     fired after LoadCorpora completes (caller can read IsLoaded)
//   onMomentEntered:   string cueId   (Day 12 voiceCueId)
//   onMomentCompleted: string cueId   (after durationSec elapsed)
//   onVerseCompleted:  int momentsPlayed
//   onError:           string message  (file missing / JSON parse fail)
//
// Style matches Day 1-10: namespace Jambudweep.Ramayana.Verse, Instance
// singleton + EnsureCreated() factory, FindFirstObjectByType for optional
// wiring (StoryEngine, DialogueOverlay), JsonUtility for corpus deserialization,
// UnityEvent for caller wiring.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.Audio;

namespace Jambudweep.Ramayana.Verse
{
    [Serializable]
    public class VerseMomentEvent : UnityEvent<string> { }

    public sealed class VerseOrchestrator : MonoBehaviour
    {
        public static VerseOrchestrator Instance { get; private set; }

        [Header("Corpus resource paths")]
        [SerializeField] private string charactersResourcePath = "Ramayana/characters";
        [SerializeField] private string momentsResourcePath   = "Ramayana/moments_bala_kanda";
        [SerializeField] private string voicesResourcePath    = "Ramayana/voices";

        [Header("Playback")]
        [SerializeField, Min(1f)] private float defaultHoldMultiplier = 1.0f;
        [SerializeField, Min(0.05f)] private float autoAdvanceWaitSeconds = 0.5f;
        [SerializeField] private bool autoPlayOnLoad = true;

        [Header("Optional wiring (auto-resolved if null)")]
        [SerializeField] private Story.StoryEngine storyEngine;
        [SerializeField] private UI.DialogueOverlay overlay;

        [Header("Events")]
        public UnityEvent onVerseLoaded = new UnityEvent();
        public VerseMomentEvent onMomentEntered = new VerseMomentEvent();
        public VerseMomentEvent onMomentCompleted = new VerseMomentEvent();
        public UnityEvent<int> onVerseCompleted = new UnityEvent<int>();
        public VerseMomentEvent onError = new VerseMomentEvent();

        // ── Runtime state ──────────────────────────────────────────────────
        private RamayanaCharacterRoster _roster;
        private RamayanaMomentsCorpus _moments;
        private RamayanaVoicesCorpus _voices;
        private Dictionary<string, string> _characterVoiceLookup;
        private Dictionary<string, string> _momentByVoiceCueId;
        private List<RamayanaVoiceCue> _currentCues;
        private int _currentCueIndex = -1;
        private bool _isPlaying;
        private bool _isLoaded;
        [Header("Audio")]
        [SerializeField] private RagaAudioEngine ragaAudioEngine;
        [SerializeField] private FallbackRagaSynth fallbackRagaSynth;

        private bool _audioWired;
        private string _currentKanda;
        private string _pendingMomentsResource; // set before LoadAndPlay resolves

        public bool IsLoaded => _isLoaded;
        public bool IsPlaying => _isPlaying;
        public int CurrentCueIndex => _currentCueIndex;
        public string CurrentCueId => (_currentCues != null && _currentCueIndex >= 0
                                       && _currentCueIndex < _currentCues.Count)
                                      ? _currentCues[_currentCueIndex].cueId
                                      : null;
        public int TotalCues => _currentCues?.Count ?? 0;
        public string CurrentKanda => _currentKanda;

        // ── Lifecycle ──────────────────────────────────────────────────────

        public static VerseOrchestrator EnsureCreated()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<VerseOrchestrator>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }
            var go = new GameObject("VerseOrchestrator");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VerseOrchestrator>();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (storyEngine == null) storyEngine = FindFirstObjectByType<Story.StoryEngine>();
            if (overlay == null) overlay = UI.DialogueOverlay.EnsureCreated();
        }

        void Start()
        {
            if (autoPlayOnLoad)
            {
                StartCoroutine(LoadAndPlay("bala-kanda"));
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void LoadCorpora()
        {
            StartCoroutine(LoadCorporaRoutine());
        }

        public bool Play(string kanda)
        {
            if (!_isLoaded)
            {
                Debug.LogWarning("[VerseOrchestrator] Play() called before LoadCorpora() finished. " +
                                 "Use LoadAndPlay() or wait for onVerseLoaded.");
                StartCoroutine(LoadAndPlay(kanda));
                return false;
            }
            if (_currentCues == null)
            {
                _currentCues = BuildCueList(kanda);
                _currentKanda = kanda;
                _currentCueIndex = -1;
            }
            if (_currentCues.Count == 0)
            {
                onError?.Invoke($"No moments found for kanda '{kanda}'");
                return false;
            }
            _isPlaying = true;
            Advance();
            return true;
        }

        public IEnumerator LoadAndPlay(string kanda)
        {
            if (!_isLoaded)
            {
                // Resolve corpus path BEFORE LoadCorporaRoutine reads moments.
                _pendingMomentsResource = ResolveMomentsResourceForKanda(kanda);
                yield return StartCoroutine(LoadCorporaRoutine());
            }
            // On first LoadCorpora run, _moments is set from _pendingMomentsResource.
            // On subsequent calls, Play() just builds a new cue list for the kanda.
            Play(kanda);
        }

        private string ResolveMomentsResourceForKanda(string kanda)
        {
            if (string.IsNullOrEmpty(kanda))
                return momentsResourcePath;

            var fileNoExt = Gameplay.KandaTree.GetCorpusFile(kanda);
            if (!string.IsNullOrEmpty(fileNoExt))
                return $"Ramayana/{fileNoExt}";

            return momentsResourcePath;
        }

        private static string ResolveMomentsFile(string pendingResource)
        {
            if (string.IsNullOrEmpty(pendingResource))
                return "moments_bala_kanda.json";
            if (pendingResource.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
                return pendingResource;
            return pendingResource + ".json";
        }

        public bool Advance()
        {
            if (_currentCues == null || _currentCues.Count == 0) return false;
            int next = _currentCueIndex + 1;
            if (next >= _currentCues.Count)
            {
                _isPlaying = false;
                int played = _currentCues.Count;
                _currentCueIndex = -1;
                onVerseCompleted?.Invoke(played);
                return false;
            }
            _currentCueIndex = next;
            var cue = _currentCues[next];
            onMomentEntered?.Invoke(cue.cueId);
            StartCoroutine(PlayCueRoutine(cue));
            return true;
        }

        public void Stop()
        {
            _isPlaying = false;
            _currentCueIndex = -1;
            StopAllCoroutines();
            overlay?.Hide();
        }

        public string GetVoiceForSpeaker(string speakerId)
        {
            if (_characterVoiceLookup == null || string.IsNullOrEmpty(speakerId)) return "kathaka";
            return _characterVoiceLookup.TryGetValue(speakerId.ToLower(), out var v) ? v : "kathaka";
        }

        public RamayanaCharacter GetCharacterById(string characterId)
        {
            if (_roster?.characters == null || string.IsNullOrEmpty(characterId)) return null;
            return _roster.characters.Find(c => c.id == characterId);
        }

        public RamayanaMoment GetMomentByCueId(string cueId)
        {
            if (_momentByVoiceCueId == null || string.IsNullOrEmpty(cueId)) return null;
            if (_momentByVoiceCueId.TryGetValue(cueId, out var momentId) && _moments?.moments != null)
            {
                return _moments.moments.Find(m => m.momentId == momentId);
            }
            return null;
        }

        // ── Internals ──────────────────────────────────────────────────────

        private IEnumerator LoadCorporaRoutine()
        {
            _roster = LoadJson<RamayanaCharacterRoster>(charactersResourcePath, "characters.json");
            yield return null;
            string momentsResource = string.IsNullOrEmpty(_pendingMomentsResource) ? momentsResourcePath : _pendingMomentsResource;
            _moments = LoadJson<RamayanaMomentsCorpus>(momentsResource, Path.GetFileName(momentsResource));
            yield return null;
            _voices = LoadJson<RamayanaVoicesCorpus>(voicesResourcePath, "voices.json");
            yield return null;

            if (_roster == null || _moments == null || _voices == null)
            {
                onError?.Invoke("One or more corpus files failed to load");
                yield break;
            }

            // Build character→voice lookup from voices corpus
            _characterVoiceLookup = new Dictionary<string, string>();
            if (_voices.characterVoiceBindings != null)
            {
                foreach (var b in _voices.characterVoiceBindings)
                {
                    _characterVoiceLookup[b.characterId.ToLower()] = b.registerId;
                }
            }

            // Build cueId → momentId lookup
            _momentByVoiceCueId = new Dictionary<string, string>();
            if (_moments.moments != null)
            {
                foreach (var m in _moments.moments)
                {
                    _momentByVoiceCueId[m.voiceCueId] = m.momentId;
                }
            }

            _isLoaded = true;
            onVerseLoaded?.Invoke();
            Debug.Log($"[VerseOrchestrator] Loaded {_roster.characters?.Count ?? 0} chars, " +
                      $"{_moments.moments?.Count ?? 0} moments, " +
                      $"{_voices.voiceCues?.Count ?? 0} voice cues.");
        }

        private T LoadJson<T>(string resourcePath, string fileLabel) where T : class
        {
            var ta = Resources.Load<TextAsset>(resourcePath);
            if (ta == null)
            {
                Debug.LogError($"[VerseOrchestrator] {fileLabel} not found at Resources/{resourcePath}");
                onError?.Invoke($"{fileLabel} not found");
                return null;
            }
            try
            {
                return JsonUtility.FromJson<T>(ta.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[VerseOrchestrator] {fileLabel} parse failed: {e.Message}");
                onError?.Invoke($"{fileLabel} parse failed");
                return null;
            }
        }

        private List<RamayanaVoiceCue> BuildCueList(string kanda)
        {
            if (_voices?.voiceCues == null) return new List<RamayanaVoiceCue>();
            // Sort by momentId for deterministic order (matches Day 12 moment ordering).
            // Filter by kanda if specified (currently only "bala-kanda" exists).
            var cues = new List<RamayanaVoiceCue>();
            foreach (var c in _voices.voiceCues)
            {
                if (string.IsNullOrEmpty(kanda) || c.kanda == kanda)
                {
                    cues.Add(c);
                }
            }
            cues.Sort((a, b) => string.Compare(a.momentId, b.momentId, StringComparison.Ordinal));
            return cues;
        }

        private void WireAudioOnce()
        {
            if (_audioWired) return;
            _audioWired = true;
            if (ragaAudioEngine != null && fallbackRagaSynth != null && ragaAudioEngine.voSource != null)
            {
                fallbackRagaSynth.SetSink(ragaAudioEngine.voSource);
            }
        }

        private IEnumerator PlayCueRoutine(RamayanaVoiceCue cue)
        {
            WireAudioOnce();
            float hold = cue.durationSec * defaultHoldMultiplier;
            string speakerLabel = !string.IsNullOrEmpty(cue.speaker) ? cue.speaker : "Narrator";
            if (overlay != null)
            {
                overlay.Show(
                    speakerLabel,
                    cue.narrationLine ?? "",
                    cue.texture ?? "",
                    null,
                    hold);
            }

            if (ragaAudioEngine != null && fallbackRagaSynth != null)
            {
                ragaAudioEngine.DuckForVO();
                FallbackRagaSynth.Instance.SetSink(ragaAudioEngine.voSource);
                FallbackRagaSynth.Instance.GetOrCreate(cue.cueId, autoPlay: true);
            }

            yield return new WaitForSeconds(hold + autoAdvanceWaitSeconds);
            onMomentCompleted?.Invoke(cue.cueId);
            if (ragaAudioEngine != null) ragaAudioEngine.UnduckAfterVO();
            if (_isPlaying)
            {
                Advance();
            }
        }
    }
}