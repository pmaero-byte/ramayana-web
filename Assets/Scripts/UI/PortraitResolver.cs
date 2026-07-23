// Day 9 (RamayanaPS5) — PortraitResolver.
// Resolves speakerId → Sprite for DialogueOverlay. Show() already accepts a
// portrait; StoryMomentPlayer used to pass null. Looks up:
//   Resources/portraits/<id>
//   Resources/portraits/<id_lower>
//   Resources/portraits/default
// Falls back to a generated monogram plate so GTA-style dialogue never
// shows an empty face slot.
//
// Style: sealed MonoBehaviour + EnsureCreated + static Resolve helper.

using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.UI
{
    public sealed class PortraitResolver : MonoBehaviour
    {
        public static PortraitResolver Instance { get; private set; }

        [Header("Lookup")]
        [SerializeField] private string portraitsResourceFolder = "portraits";
        [SerializeField] private string defaultPortraitName = "default";

        private readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>(32);
        private Sprite _fallbackPlate;

        public static PortraitResolver EnsureCreated()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("PortraitResolver");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<PortraitResolver>();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Resolve a portrait for a speaker id (e.g. "Rama", "hanuman").</summary>
        public static Sprite Resolve(string speakerId)
        {
            EnsureCreated();
            return Instance.ResolvePortrait(speakerId);
        }

        public Sprite ResolvePortrait(string speakerId)
        {
            string key = Normalize(speakerId);
            if (string.IsNullOrEmpty(key)) return FallbackPlate();

            if (_cache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            Sprite sprite =
                LoadSprite(key) ??
                LoadSprite(key.Replace(" ", "")) ??
                LoadSprite(key.Replace("_", "")) ??
                LoadSprite(defaultPortraitName);

            if (sprite == null) sprite = FallbackPlate();
            _cache[key] = sprite;
            return sprite;
        }

        // ── Internals ──────────────────────────────────────────────

        private Sprite LoadSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            // Resources path is relative to Assets/Resources, no extension.
            var s = Resources.Load<Sprite>($"{portraitsResourceFolder}/{id}");
            if (s != null) return s;
            // Some pipelines import as Texture2D only — wrap as sprite.
            var tex = Resources.Load<Texture2D>($"{portraitsResourceFolder}/{id}");
            if (tex != null)
            {
                return Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
            return null;
        }

        private static string Normalize(string speakerId)
        {
            if (string.IsNullOrWhiteSpace(speakerId)) return string.Empty;
            string s = speakerId.Trim().ToLowerInvariant();
            // Strip diacritics-ish and honorifics common in corpus.
            s = s.Replace("lord ", "").Replace("prince ", "").Replace("king ", "");
            s = s.Replace("ś", "s").Replace("ṣ", "s").Replace("ṛ", "r").Replace("ā", "a")
                 .Replace("ī", "i").Replace("ū", "u").Replace("ṇ", "n").Replace("ṭ", "t")
                 .Replace("ḍ", "d").Replace("ṃ", "m").Replace("ḥ", "h");
            // Map a few epic aliases.
            if (s == "raghava" || s == "ramachandra") s = "rama";
            if (s == "janaki" || s == "vaidehi") s = "sita";
            if (s == "anjaneya" || s == "maruti") s = "hanuman";
            if (s == "saumitri") s = "lakshmana";
            return s;
        }

        private Sprite FallbackPlate()
        {
            if (_fallbackPlate != null) return _fallbackPlate;
            // Gold monogram plate — always visible even with zero art assets.
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var gold = new Color(0.85f, 0.68f, 0.30f, 1f);
            var ink = new Color(0.12f, 0.07f, 0.04f, 1f);
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                bool border = x < 3 || y < 3 || x > 60 || y > 60;
                tex.SetPixel(x, y, border ? gold : ink);
            }
            tex.Apply();
            _fallbackPlate = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);
            _fallbackPlate.name = "PortraitFallback";
            return _fallbackPlate;
        }
    }
}
