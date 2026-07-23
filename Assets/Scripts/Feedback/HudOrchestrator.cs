// Day 4 (RamayanaPS5) — HudOrchestrator.
// Single MonoBehaviour that wires StoryMomentPlayer + WaveController events
// into the existing feedback HUDs (KandaPortraitHUD, VerseStreakHUD,
// DayDotStrip, SanskritTitle) so the runtime HUD updates automatically
// as the player walks through objectives and clears waves.
//
// Style matches existing Feedback/* singletons: instance pattern with
// auto-bootstrap (no inspector wiring required) and runtime UI build.

using UnityEngine;
using UnityEngine.Events;
using Jambudweep.Ramayana.Story;
using Jambudweep.Ramayana.Combat;
using Jambudweep.Ramayana.Data;

namespace Jambudweep.Ramayana.Feedback
{
    public sealed class HudOrchestrator : MonoBehaviour
    {
        public static HudOrchestrator Instance { get; private set; }

        [Header("Optional wiring (auto-resolved if null)")]
        [SerializeField] private StoryMomentPlayer momentPlayer;
        [SerializeField] private WaveController waveController;

        [Header("Day mapping")]
        [Tooltip("Maps an actId to a 1-based 'day' number for the DayDotStrip + SanskritTitle HUDs.")]
        [SerializeField] private string[] actIdsInDayOrder = new string[]
        {
            "bala-birth",
            "ayodhya-dharma",
            "panchavati-golden-deer",
            "kishkindha-alliance",
            "sundarakanda-leap",
            "yuddhakanda-war",
            "return-ayodhya",
            "uttara-earth-return"
        };

        private int _currentDay;
        private int _streak;

        [Header("Events")]
        public UnityEvent<int> onDayChanged = new UnityEvent<int>();
        public UnityEvent<int> onStreakChanged = new UnityEvent<int>();

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("HudOrchestrator");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<HudOrchestrator>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (momentPlayer == null) momentPlayer = FindFirstObjectByType<StoryMomentPlayer>();
            if (waveController == null) waveController = FindFirstObjectByType<WaveController>();
        }

        void OnEnable()
        {
            // Ensure every HUD exists before we start wiring events.
            KandaPortraitHUD.EnsureCreated();
            VerseStreakHUD.EnsureCreated();
            DayDotStrip.EnsureCreated();
            SanskritTitle.EnsureCreated();
        }

        void Start()
        {
            BindEvents();
            // Initial paint so the HUDs don't sit empty on first frame.
            SanskritTitle.Instance.Show(_currentDay);
            DayDotStrip.Instance.SetDay(Mathf.Max(1, _currentDay), 0);
        }

        // ── Event wiring ──────────────────────────────────────────

        private void BindEvents()
        {
            if (momentPlayer != null)
            {
                momentPlayer.onObjectiveEntered.AddListener(HandleObjectiveEntered);
                momentPlayer.onActCompleted.AddListener(HandleActCompleted);
            }
            if (waveController != null)
            {
                waveController.onWaveCompleted.AddListener(HandleWaveCompleted);
                waveController.onAllWavesCompleted.AddListener(HandleAllWavesCompleted);
            }
        }

        void OnDestroy()
        {
            if (momentPlayer != null)
            {
                momentPlayer.onObjectiveEntered.RemoveListener(HandleObjectiveEntered);
                momentPlayer.onActCompleted.RemoveListener(HandleActCompleted);
            }
            if (waveController != null)
            {
                waveController.onWaveCompleted.RemoveListener(HandleWaveCompleted);
                waveController.onAllWavesCompleted.RemoveListener(HandleAllWavesCompleted);
            }
        }

        // ── Handlers ──────────────────────────────────────────────

        private void HandleObjectiveEntered(string objectiveId)
        {
            if (momentPlayer == null) return;
            int actNumber = ActNumberFor(momentPlayer.CurrentActId);
            KandaPortraitHUD.Instance.Show(actNumber, momentPlayer.ObjectiveIndex);
            // DayDotStrip uses 1-based day = act number for simplicity.
            if (_currentDay != actNumber) SetDay(actNumber);
        }

        private void HandleActCompleted(string actId)
        {
            VerseStreakHUD.Instance.OnSuccess();
            _streak = VerseStreakHUD.Instance.CurrentStreak;
            onStreakChanged?.Invoke(_streak);
        }

        private void HandleWaveCompleted(int wave)
        {
            VerseStreakHUD.Instance.OnSuccess();
            _streak = VerseStreakHUD.Instance.CurrentStreak;
            onStreakChanged?.Invoke(_streak);
        }

        private void HandleAllWavesCompleted()
        {
            // Wave cleared — bump the day dot strip forward by one.
            SetDay(Mathf.Min(_currentDay + 1, actIdsInDayOrder.Length));
            SanskritTitle.Instance.Show(_currentDay);
        }

        // ── Helpers ───────────────────────────────────────────────

        private void SetDay(int day)
        {
            _currentDay = Mathf.Clamp(day, 1, actIdsInDayOrder.Length);
            DayDotStrip.Instance.SetDay(_currentDay, 0);
            onDayChanged?.Invoke(_currentDay);
        }

        private int ActNumberFor(string actId)
        {
            if (string.IsNullOrEmpty(actId)) return 0;
            for (int i = 0; i < actIdsInDayOrder.Length; i++)
            {
                if (actIdsInDayOrder[i] == actId) return i + 1;
            }
            return 0;
        }
    }
}
