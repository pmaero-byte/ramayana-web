// Round 31 — DharmaLedgerMini: lower-center compact ledger showing the
// current dharma score (positive/negative) + an icon hint. Kid-friendly
// framing: "Your choices have moved the dharma meter". Distinct from
// EmbassyRecapHUD (which recaps events, not a live score).

using UnityEngine;
using UnityEngine.UI;
using Jambudweep.Ramayana.Story;

namespace Jambudweep.Ramayana.Feedback
{
    public sealed class DharmaLedgerMini : MonoBehaviour
    {
        public static DharmaLedgerMini Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Text _label;
        private Text _scoreText;
        private Image _bar;
        private StoryEngine _engine;
        private int _lastScore = int.MinValue;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("DharmaLedgerMini");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<DharmaLedgerMini>();
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

            var panel = new GameObject("Panel");
            panel.transform.SetParent(transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(360f, 80f);
            rt.anchoredPosition = new Vector2(0f, 20f);

            _bg = panel.AddComponent<Image>();
            _bg.color = new Color(0.07f, 0.04f, 0.02f, 0.78f);

            // Title row
            _label = MakeText(panel.transform, "Title", 16, TextAnchor.UpperLeft,
                new Vector2(10f, 4f), new Vector2(180f, 22f));
            _label.text = "Dharma";

            // Score text (right-aligned)
            _scoreText = MakeText(panel.transform, "Score", 28, TextAnchor.UpperRight,
                new Vector2(-10f, 4f), new Vector2(180f, 36f));
            _scoreText.text = "+0";
            _scoreText.fontStyle = FontStyle.Bold;

            // Bar background
            var barBgGO = new GameObject("BarBG");
            barBgGO.transform.SetParent(panel.transform, false);
            var barBgRT = barBgGO.AddComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(0f, 0f);
            barBgRT.anchorMax = new Vector2(1f, 0f);
            barBgRT.pivot = new Vector2(0.5f, 0f);
            barBgRT.sizeDelta = new Vector2(-20f, 14f);
            barBgRT.anchoredPosition = new Vector2(0f, 8f);
            var barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = new Color(0.05f, 0.02f, 0.08f, 0.85f);

            // Bar fill
            var barGO = new GameObject("BarFill");
            barGO.transform.SetParent(barBgGO.transform, false);
            var barRT = barGO.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0.5f, 1f); // initial neutral
            barRT.pivot = new Vector2(0.5f, 0.5f);
            barRT.sizeDelta = Vector2.zero;
            barRT.anchoredPosition = Vector2.zero;
            _bar = barGO.AddComponent<Image>();
            _bar.color = new Color(0.65f, 0.85f, 0.55f, 1f);
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
            t.color = new Color(0.95f, 0.92f, 0.85f, 1f);
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
            int score = _engine != null ? _engine.state.dharmaScore : 0;
            if (score != _lastScore)
            {
                _lastScore = score;
                _scoreText.text = (score >= 0 ? "+" : "") + score.ToString();
                _bar.color = score >= 0
                    ? new Color(0.55f, 0.85f, 0.55f, 1f)
                    : new Color(0.85f, 0.45f, 0.45f, 1f);
                // Map [-100, +100] to bar fill [0.05, 0.95]
                float t = Mathf.Clamp(0.5f + score / 200f, 0.05f, 0.95f);
                var barRT = _bar.GetComponent<RectTransform>();
                barRT.anchorMax = new Vector2(t, 1f);
            }
        }
    }
}