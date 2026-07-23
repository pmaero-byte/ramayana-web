// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Kanda Tree Navigation (Day 23)
// Canonical kanda registry + scene-launch wiring for MainMenu.
// Single source of truth for VerseOrchestrator, SaveSystem, main menu cards.
// ════════════════════════════════════════════════════════════════════════════
//
// Registry rules:
//   - kandaId matches moments JSON field exactly.
//   - sceneName matches Assets/Scenes/<sceneName>.unity exactly.
//   - corpusFileName is the Resources JSON stem WITHOUT .json.
//   - order is canonical gameplay order.
//
// Adding a new kanda:
//   1. Add a KandaEntry record below.
//   2. Add Assets/Resources/Ramayana/moments_{kanda}.json.
//   3. Add Assets/Scenes/{SceneName}.unity.
//   4. Update Day 23 verifier expectations.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Gameplay
{
    [Serializable]
    public sealed class KandaEntry
    {
        [SerializeField] public string kandaId;
        [SerializeField] public string corpusFileName;
        [SerializeField] public string sceneName;
        [SerializeField] public string displayTitle;
        [SerializeField] public int order;
        [SerializeField] public string summary;

        public string KandaId => kandaId;
        public string CorpusFileName => corpusFileName;
        public string SceneName => sceneName;
        public string DisplayTitle => displayTitle;
        public int Order => order;
        public string Summary => summary;
    }

    public static class KandaTree
    {
        private static readonly KandaEntry[] _registry = new KandaEntry[]
        {
            new KandaEntry
            {
                kandaId = "bala-kanda",
                corpusFileName = "moments_bala_kanda",
                sceneName = "BalaKanda",
                displayTitle = "Bala Kanda",
                order = 1,
                summary = "Rama's birth, childhood, and first trials in the forest."
            },
            new KandaEntry
            {
                kandaId = "ayodhya-kanda",
                corpusFileName = "moments_ayodhya_kanda",
                sceneName = "AyodhyaKanda",
                displayTitle = "Ayodhya Kanda",
                order = 2,
                summary = "The court of Ayodhya, exile, and the kingdom left behind."
            },
            new KandaEntry
            {
                kandaId = "aranya-kanda",
                corpusFileName = "moments_aranya_kanda",
                sceneName = "AranyaKanda",
                displayTitle = "Aranya Kanda",
                order = 3,
                summary = "The fourteen years in the forest — loss and resilience."
            },
            new KandaEntry
            {
                kandaId = "kishkindha-kanda",
                corpusFileName = "moments_kishkindha_kanda",
                sceneName = "KishkindhaKanda",
                displayTitle = "Kishkindha Kanda",
                order = 4,
                summary = "Allies forged in the hills — Hanuman and Sugriva."
            },
            new KandaEntry
            {
                kandaId = "sundara-kanda",
                corpusFileName = "moments_sundara_kanda",
                sceneName = "SundaraKanda",
                displayTitle = "Sundara Kanda",
                order = 5,
                summary = "The great leap across the ocean and the search for Sita."
            },
            new KandaEntry
            {
                kandaId = "yuddha-kanda",
                corpusFileName = "moments_yuddha_kanda",
                sceneName = "YuddhaKanda",
                displayTitle = "Yuddha Kanda",
                order = 6,
                summary = "The war that ends Ravana and restores dharma."
            },
            new KandaEntry
            {
                kandaId = "uttara-kanda",
                corpusFileName = "moments_uttara_kanda",
                sceneName = "UttaraKanda",
                displayTitle = "Uttara Kanda",
                order = 7,
                summary = "Return, coronation, and the final test of dharma."
            },
            new KandaEntry
            {
                kandaId = "return-kanda",
                corpusFileName = "moments_return_kanda",
                sceneName = "ReturnKanda",
                displayTitle = "Return Kanda",
                order = 8,
                summary = "Rama returns to Ayodhya — the Deepavali that begins Ram Rajya."
            }
        };

        private static readonly Dictionary<string, KandaEntry> _byId;
        private static readonly Dictionary<string, string> _sceneByKanda;
        private static readonly Dictionary<string, string> _corpusByKanda;

        public static int KandaCount => _registry.Length;
        public static IReadOnlyList<KandaEntry> Entries => _registry;

        static KandaTree()
        {
            _byId = new Dictionary<string, KandaEntry>(StringComparer.OrdinalIgnoreCase);
            _sceneByKanda = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _corpusByKanda = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in _registry)
            {
                _byId[e.KandaId] = e;
                _sceneByKanda[e.KandaId] = e.SceneName;
                _corpusByKanda[e.KandaId] = e.CorpusFileName;
            }
        }

        public static KandaEntry GetEntry(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return null;
            _byId.TryGetValue(kandaId, out var e);
            return e;
        }

        public static string GetSceneName(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return null;
            _sceneByKanda.TryGetValue(kandaId, out var s);
            return s;
        }

        public static string GetCorpusFile(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return null;
            _corpusByKanda.TryGetValue(kandaId, out var f);
            return f;
        }

        public static string GetNextKanda(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return null;
            _byId.TryGetValue(kandaId, out var current);
            if (current == null) return null;
            foreach (var e in _registry)
                if (e.Order == current.Order + 1) return e.KandaId;
            return null;
        }

        public static string GetPreviousKanda(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return null;
            _byId.TryGetValue(kandaId, out var current);
            if (current == null) return null;
            foreach (var e in _registry)
                if (e.Order == current.Order - 1) return e.KandaId;
            return null;
        }

        public static bool TryLoadScene(string kandaId)
        {
            var sceneName = GetSceneName(kandaId);
            if (string.IsNullOrEmpty(sceneName)) return false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}
