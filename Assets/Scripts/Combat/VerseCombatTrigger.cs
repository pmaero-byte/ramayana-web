// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Verse Combat Trigger (Day 16)
// Bridges VerseOrchestrator.onMomentCompleted → WaveController.StartWaves.
// Data-driven: a trigger map configures which moment cueIds spawn which
// formation/wave-count/grace-period combos. Supports per-cue customization
// without hardcoding each transition in C#.
//
// Wiring pattern matches Day 1-10 + Day 14/15:
//   - public sealed class MonoBehaviour
//   - public static Instance + EnsureCreated() factory
//   - FindFirstObjectByType<VerseOrchestrator> + WaveController auto-resolve
//   - UnityEvent hooks (onTriggerFired, onTriggerSkipped) for caller wiring
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Jambudweep.Ramayana.Combat;
using Jambudweep.Ramayana.Verse;

namespace Jambudweep.Ramayana.Combat
{
    [Serializable]
    public class VerseCombatTriggerRule
    {
        [Tooltip("The voice cueId to listen for (e.g., vc_bala_face-challenge_vishw_protects_tadaka).")]
        public string cueId;

        [Tooltip("How many waves to spawn when this trigger fires.")]
        [Min(1)] public int totalWaves = 1;

        [Tooltip("Formation strategy to use for these waves.")]
        public FormationKind formation = FormationKind.Arc;

        [Tooltip("Seconds to wait after the moment completes before spawning waves.")]
        [Min(0f)] public float graceSeconds = 1.5f;

        [Tooltip("If true, this trigger fires only once per session (after which it's marked exhausted).")]
        public bool oneShot = true;

        [Tooltip("Optional human-readable label for logs / debug UI.")]
        public string label;
    }

    [Serializable]
    public class VerseCombatTriggerEvent : UnityEvent<string> { } // cueId

    public sealed class VerseCombatTrigger : MonoBehaviour
    {
        public static VerseCombatTrigger Instance { get; private set; }

        [Header("Trigger rules")]
        [Tooltip("Each rule maps a moment cueId → wave spawn config.")]
        [SerializeField] private List<VerseCombatTriggerRule> rules = new List<VerseCombatTriggerRule>();

        [Header("Optional wiring (auto-resolved if null)")]
        [SerializeField] private VerseOrchestrator verseOrchestrator;
        [SerializeField] private WaveController waveController;

        [Header("Defaults")]
        [Tooltip("If true and no rule matches a cueId, fall back to a kanda-based default.")]
        [SerializeField] private bool useKandaDefaults = true;

        [Header("Events")]
        public VerseCombatTriggerEvent onTriggerFired = new VerseCombatTriggerEvent();
        public VerseCombatTriggerEvent onTriggerSkipped = new VerseCombatTriggerEvent();
        public VerseCombatTriggerEvent onError = new VerseCombatTriggerEvent();

        // ── Runtime state ──────────────────────────────────────────────────
        private readonly HashSet<string> _exhausted = new HashSet<string>();
        private readonly Dictionary<string, VerseCombatTriggerRule> _rulesByCueId =
            new Dictionary<string, VerseCombatTriggerRule>();
        private Coroutine _pendingRoutine;
        private bool _wired;

        public int RuleCount => rules?.Count ?? 0;
        public int ExhaustedCount => _exhausted.Count;
        public bool IsReady => verseOrchestrator != null && waveController != null;

        // ── Lifecycle ──────────────────────────────────────────────────────

        public static VerseCombatTrigger EnsureCreated()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<VerseCombatTrigger>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }
            var go = new GameObject("VerseCombatTrigger");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VerseCombatTrigger>();
            return Instance;
        }

        public static VerseCombatTrigger EnsureCreatedWithRules(List<VerseCombatTriggerRule> initialRules)
        {
            var inst = EnsureCreated();
            if (initialRules != null && inst.rules.Count == 0)
            {
                inst.rules = initialRules;
            }
            return inst;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildLookup();
        }

        void Start()
        {
            WireIfReady();
        }

        void Update()
        {
            if (!_wired) WireIfReady();
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void AddRule(VerseCombatTriggerRule rule)
        {
            if (rule == null || string.IsNullOrEmpty(rule.cueId)) return;
            rules.Add(rule);
            _rulesByCueId[rule.cueId] = rule;
        }

        public bool HasRule(string cueId) => _rulesByCueId.ContainsKey(cueId);

        public void ResetExhausted()
        {
            _exhausted.Clear();
        }

        public void WireIfReady()
        {
            if (verseOrchestrator == null) verseOrchestrator = VerseOrchestrator.Instance
                ?? FindFirstObjectByType<VerseOrchestrator>();
            if (waveController == null) waveController = FindFirstObjectByType<WaveController>();
            if (verseOrchestrator == null || waveController == null) return;
            if (_wired) return;
            verseOrchestrator.onMomentCompleted.AddListener(HandleMomentCompleted);
            _wired = true;
        }

        // ── Internals ──────────────────────────────────────────────────────

        private void BuildLookup()
        {
            _rulesByCueId.Clear();
            if (rules == null) return;
            foreach (var r in rules)
            {
                if (r == null || string.IsNullOrEmpty(r.cueId)) continue;
                _rulesByCueId[r.cueId] = r;
            }
        }

        private void HandleMomentCompleted(string cueId)
        {
            if (string.IsNullOrEmpty(cueId)) return;
            if (_rulesByCueId.TryGetValue(cueId, out var rule))
            {
                if (rule.oneShot && _exhausted.Contains(cueId))
                {
                    onTriggerSkipped?.Invoke(cueId);
                    return;
                }
                if (rule.oneShot) _exhausted.Add(cueId);
                onTriggerFired?.Invoke(cueId);
                if (_pendingRoutine != null) StopCoroutine(_pendingRoutine);
                _pendingRoutine = StartCoroutine(SpawnAfterGrace(rule));
                return;
            }

            onTriggerSkipped?.Invoke(cueId);

            if (!useKandaDefaults) return;
            if (!TryGetDefaultRuleForCueId(cueId, out rule)) return;

            onTriggerFired?.Invoke(cueId);
            if (_pendingRoutine != null) StopCoroutine(_pendingRoutine);
            _pendingRoutine = StartCoroutine(SpawnAfterGrace(rule));
        }

        private bool TryGetDefaultRuleForCueId(string cueId, out VerseCombatTriggerRule rule)
        {
            rule = null;
            if (verseOrchestrator == null)
            {
                verseOrchestrator = VerseOrchestrator.Instance
                    ?? FindFirstObjectByType<VerseOrchestrator>();
            }
            if (verseOrchestrator == null) return false;

            string kandaId = verseOrchestrator.CurrentKanda;
            if (string.IsNullOrEmpty(kandaId)) return false;

            string mappedCueId = null;
            if (kandaId.Equals("bala-kanda", StringComparison.OrdinalIgnoreCase)
                || kandaId.Equals("ayodhya-kanda", StringComparison.OrdinalIgnoreCase))
            {
                mappedCueId = "vc_default_early_kanda_combat";
            }
            else if (kandaId.Equals("aranya-kanda", StringComparison.OrdinalIgnoreCase))
            {
                mappedCueId = "vc_default_aranya_skirmish";
            }
            else if (kandaId.Equals("kishkindha-kanda", StringComparison.OrdinalIgnoreCase)
                     || kandaId.Equals("sundara-kanda", StringComparison.OrdinalIgnoreCase))
            {
                mappedCueId = "vc_default_alliance_search_combat";
            }
            else if (kandaId.Equals("yuddha-kanda", StringComparison.OrdinalIgnoreCase))
            {
                mappedCueId = "vc_default_war_assault";
            }
            else if (kandaId.Equals("uttara-kanda", StringComparison.OrdinalIgnoreCase)
                     || kandaId.Equals("return-kanda", StringComparison.OrdinalIgnoreCase))
            {
                mappedCueId = "vc_default_finale_defense";
            }
            else
            {
                mappedCueId = "vc_default_combat";
            }

            if (string.IsNullOrEmpty(mappedCueId)) return false;
            if (!_rulesByCueId.TryGetValue(mappedCueId, out rule)) return false;

            Debug.Log($"[VerseCombatTrigger] Default fallback for cue '{cueId}' " +
                      $"→ kanda='{kandaId}' rule='{mappedCueId}'");
            return true;
        }

        private IEnumerator SpawnAfterGrace(VerseCombatTriggerRule rule)
        {
            if (rule.graceSeconds > 0f)
            {
                yield return new WaitForSeconds(rule.graceSeconds);
            }
            if (waveController == null)
            {
                onError?.Invoke("WaveController missing at spawn time");
                yield break;
            }
            waveController.SetFormation(rule.formation);
            waveController.StartWaves(rule.totalWaves);
            Debug.Log($"[VerseCombatTrigger] Fired '{rule.label ?? rule.cueId}' " +
                      $"→ {rule.totalWaves} wave(s), formation={rule.formation}, " +
                      $"grace={rule.graceSeconds}s");
        }
    }
}