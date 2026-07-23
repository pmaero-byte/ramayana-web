// Round 31 — TimeOfDayPill: small upper-right pill showing the current
// Vedic time-of-day band (sunrise/midday/sunset/night). Auto-ticks via
// Time.time scaled to a 4-band cycle. Independent of story state — pure
// ambience HUD (kids mode: "the world is in the afternoon now").

using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class TimeOfDayBand
    {
        public const int Sunrise = 0;
        public const int Midday  = 1;
        public const int Sunset  = 2;
        public const int Night   = 3;

        public static string Label(int band)
        {
            switch (band)
            {
                case Sunrise: return "Sunrise";
                case Midday:  return "Midday";
                case Sunset:  return "Sunset";
                case Night:   return "Night";
                default:      return "—";
            }
        }
    }

    public sealed class TimeOfDayPill : MonoBehaviour
    {
        public static TimeOfDayPill Instance { get; private set; }

        [Header("Timing")]
        [Tooltip("Seconds of real time per band. 30s = full cycle in 2 minutes.")]
        [SerializeField, Min(1f)] private float secondsPerBand = 30f;

        [Header("Optional")]
        [Tooltip("Force a specific band (useful for cinematic moments). -1 = auto.")]
        [SerializeField] private int forceBand = -1;

        private Canvas _canvas;
        private Image _bg;
        private Text _label;
        private Text _glyph;
        private int _lastBand = -1;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("TimeOfDayPill");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<TimeOfDayPill>();
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
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(180f, 60f);
            rt.anchoredPosition = new Vector2(-20f, -100f);

            _bg = pill.AddComponent<Image>();
            _bg.color = new Color(0.07f, 0.04f, 0.02f, 0.78f);

            _glyph = MakeText(pill.transform, "Glyph", 26, TextAnchor.MiddleLeft,
                new Vector2(8f, 0f), new Vector2(40f, 56f));
            _glyph.text = "☀";

            _label = MakeText(pill.transform, "Label", 20, TextAnchor.MiddleLeft,
                new Vector2(46f, 0f), new Vector2(126f, 56f));
            _label.text = "Sunrise";
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
            t.color = new Color(0.95f, 0.85f, 0.55f, 1f);
            return t;
        }

        public int CurrentBand
        {
            get
            {
                if (forceBand >= 0 && forceBand <= 3) return forceBand;
                int elapsed = Mathf.FloorToInt(Time.time / Mathf.Max(1f, secondsPerBand));
                return elapsed % 4;
            }
        }

        void Update()
        {
            int band = CurrentBand;
            if (band != _lastBand)
            {
                _lastBand = band;
                _label.text = TimeOfDayBand.Label(band);
                _glyph.text = GlyphForBand(band);
                _bg.color = ColorForBand(band);
            }
        }

        public void SetForceBand(int band)
        {
            forceBand = Mathf.Clamp(band, -1, 3);
            _lastBand = -1; // force refresh
        }

        private static string GlyphForBand(int band)
        {
            switch (band)
            {
                case TimeOfDayBand.Sunrise: return "☀";
                case TimeOfDayBand.Midday:  return "◉";
                case TimeOfDayBand.Sunset:  return "☽";
                case TimeOfDayBand.Night:   return "✦";
                default: return "·";
            }
        }

        private static Color ColorForBand(int band)
        {
            switch (band)
            {
                case TimeOfDayBand.Sunrise: return new Color(0.30f, 0.18f, 0.08f, 0.78f);
                case TimeOfDayBand.Midday:  return new Color(0.20f, 0.22f, 0.08f, 0.78f);
                case TimeOfDayBand.Sunset:  return new Color(0.20f, 0.08f, 0.18f, 0.78f);
                case TimeOfDayBand.Night:   return new Color(0.04f, 0.04f, 0.10f, 0.78f);
                default: return new Color(0.07f, 0.04f, 0.02f, 0.78f);
            }
        }
    }
}