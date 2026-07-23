// Round 31 — GuidedModePill: small lower-left pill showing the current play
// mode (Guided / Exploration / Free Play). Driven by StoryMode from
// StoryEngine.state.mode. Kids mode default = "Guided" (scripted story).

using UnityEngine;
using UnityEngine.UI;
using Jambudweep.Ramayana.Story;

namespace Jambudweep.Ramayana.Feedback
{
    public sealed class GuidedModePill : MonoBehaviour
    {
        public static GuidedModePill Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Text _label;
        private Text _glyph;
        private StoryEngine _engine;
        private string _lastMode = "";

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("GuidedModePill");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<GuidedModePill>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4710;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var pill = new GameObject("Pill");
            pill.transform.SetParent(transform, false);
            var rt = pill.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(220f, 60f);
            rt.anchoredPosition = new Vector2(20f, 100f);

            _bg = pill.AddComponent<Image>();
            _bg.color = new Color(0.07f, 0.04f, 0.02f, 0.78f);

            _glyph = MakeText(pill.transform, "Glyph", 24, TextAnchor.MiddleLeft,
                new Vector2(8f, 0f), new Vector2(40f, 56f));
            _glyph.text = "▸";

            _label = MakeText(pill.transform, "Label", 18, TextAnchor.MiddleLeft,
                new Vector2(46f, 0f), new Vector2(166f, 56f));
            _label.text = "Guided";
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = pos;
            rt.offsetMax = new Vector2(pos.x + rectSize.x, pos.y + rectSize.y);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = new Color(0.65f, 0.95f, 0.85f, 1f);
            return t;
        }

        void Awake()
        {
            if (_engine == null) _engine = FindFirstObjectByType<StoryEngine>();
        }

        public void SetEngine(StoryEngine engine) { _engine = engine; }

        void Update()
        {
            if (_engine == null) _engine = FindFirstObjectByType<StoryEngine>();
            string label = ModeLabel(_engine != null ? _engine.state.mode : StoryMode.Idle);
            if (label != _lastMode)
            {
                _lastMode = label;
                _label.text = label;
                _glyph.text = GlyphForMode(_engine != null ? _engine.state.mode : StoryMode.Idle);
            }
        }

        public static string ModeLabel(StoryMode mode)
        {
            switch (mode)
            {
                case StoryMode.Idle:           return "Idle";
                case StoryMode.CharacterSelect: return "Choose Hero";
                case StoryMode.Prologue:        return "Prologue";
                case StoryMode.Playing:         return "Guided";
                case StoryMode.Paused:          return "Paused";
                case StoryMode.Cutscene:        return "Cutscene";
                case StoryMode.Dialogue:        return "Dialogue";
                case StoryMode.Choice:          return "Choice";
                case StoryMode.Exploration:     return "Explore";
                case StoryMode.Combat:          return "Combat";
                case StoryMode.Transition:      return "Travel";
                case StoryMode.Summary:         return "Summary";
                default:                        return "—";
            }
        }

        private static string GlyphForMode(StoryMode mode)
        {
            switch (mode)
            {
                case StoryMode.Playing:        return "▸";
                case StoryMode.Exploration:    return "◌";
                case StoryMode.Combat:         return "⚔";
                case StoryMode.Dialogue:       return "“";
                case StoryMode.Choice:         return "?";
                case StoryMode.Cutscene:       return "✦";
                case StoryMode.Paused:         return "‖";
                case StoryMode.Summary:        return "✓";
                default:                       return "·";
            }
        }
    }
}