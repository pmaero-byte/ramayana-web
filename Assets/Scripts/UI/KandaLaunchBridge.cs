// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Kanda Launch Bridge (Day 36)
// Carries kanda selection state across SceneManager.LoadScene and configures
// the runtime chain in the destination scene.
//
// Flow:
//   MainMenu card tap / KandaTreeSceneBootstrap keyboard shortcut
//     → KandaLaunchBridge.Select(kandaId)
//     → KandaTree.TryLoadScene(sceneName)
//     → SceneManager.LoadScene(sceneName)
//     → PlayerSceneBootstrap.Awake() detects pending kanda
//     → VerseOrchestrator.LoadAndPlay(kandaId)
//     → RagaAudioEngine.CrossfadeToRaga(kandaRaga)
//     → VerseCombatTrigger.ResetExhausted()
// ════════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using Jambudweep.Ramayana.Gameplay;
using Jambudweep.Ramayana.Verse;
using Jambudweep.Ramayana.Audio;
using Jambudweep.Ramayana.Combat;

namespace Jambudweep.Ramayana.UI
{
    /// <summary>
    /// Singleton that holds the last-selected kanda across a scene transition.
    /// Cleared after PlayerSceneBootstrap consumes it.
    /// </summary>
    public sealed class KandaLaunchBridge : MonoBehaviour
    {
        public static KandaLaunchBridge Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private string pendingKanda;

        // Raga mapping per kanda (mirrors RagaAudioEngine comment block).
        private static readonly System.Collections.Generic.Dictionary<string, Raga> KandaRagaMap
            = new System.Collections.Generic.Dictionary<string, Raga>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "bala-kanda",     Raga.Bhairavi },
            { "ayodhya-kanda",  Raga.Kalyani },
            { "aranya-kanda",   Raga.Hamsadhwani },
            { "kishkindha-kanda", Raga.Desh },
            { "sundara-kanda",  Raga.Desh },
            { "yuddha-kanda",   Raga.Bhairav },
            { "uttara-kanda",   Raga.Poorvikalyan },
            { "return-kanda",   Raga.Poorvikalyan }
        };

        // corpus_data.json actId → KandaTree.kandaId mapping (names differ).
        private static readonly System.Collections.Generic.Dictionary<string, string> ActToKandaMap
            = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "bala-birth",          "bala-kanda" },
            { "ayodhya-dharma",      "ayodhya-kanda" },
            { "panchavati-golden-deer", "aranya-kanda" },
            { "kishkindha-alliance", "kishkindha-kanda" },
            { "sundarakanda-leap",   "sundara-kanda" },
            { "yuddhakanda-war",     "yuddha-kanda" },
            { "return-ayodhya",      "uttara-kanda" },
            { "uttara-earth-return", "return-kanda" }
        };

        public static string ResolveKandaId(string actId)
        {
            if (ActToKandaMap.TryGetValue(actId, out var kandaId))
                return kandaId;
            // If it already looks like a kandaId, pass through.
            if (KandaRagaMap.ContainsKey(actId))
                return actId;
            return actId;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Selects a kanda and loads its scene. Call from MainMenu card tap
        /// or KandaTreeSceneBootstrap keyboard shortcut.
        /// </summary>
        public static void Select(string actId)
        {
            if (string.IsNullOrEmpty(actId)) return;
            string kandaId = ResolveKandaId(actId);
            var inst = EnsureInstance();
            inst.pendingKanda = kandaId;
            inst.StartCoroutine(inst.LaunchRoutine(kandaId));
        }

        /// <summary>
        /// Returns the pending kanda and clears it. Safe to call from any
        /// scene-root bootstrap.
        /// </summary>
        public static string ConsumePending()
        {
            var inst = Instance;
            if (inst == null) return null;
            string k = inst.pendingKanda;
            inst.pendingKanda = null;
            return k;
        }

        private static KandaLaunchBridge EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("KandaLaunchBridge");
            Instance = go.AddComponent<KandaLaunchBridge>();
            return Instance;
        }

        private IEnumerator LaunchRoutine(string kandaId)
        {
            var entry = KandaTree.GetEntry(kandaId);
            if (entry == null)
            {
                Debug.LogError($"[KandaLaunchBridge] Unknown kanda: {kandaId}");
                yield break;
            }

            Debug.Log($"[KandaLaunchBridge] Launching kanda: {entry.DisplayTitle} (scene={entry.SceneName})");

            bool launched = KandaTree.TryLoadScene(kandaId);
            if (!launched)
            {
                Debug.LogError($"[KandaLaunchBridge] Failed to load scene for kanda: {kandaId}");
                pendingKanda = null;
                yield break;
            }

            // Wait for the new scene to fully load so PlayerSceneBootstrap.Start()
            // can consume the pending kanda and start the moment+audio+combat chain.
            // Do not call VerseOrchestrator here; PlayerSceneBootstrap owns that.
            yield break;
        }
    }
}
