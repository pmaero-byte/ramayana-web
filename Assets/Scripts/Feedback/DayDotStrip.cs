// Round 29 — DayDotStrip: 18 small horizontal dots at the top that fill as days progress.
// Active day pulses; completed days show as filled gold; future days as empty grey.

using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class DayDotStrip : MonoBehaviour
    {
        public static DayDotStrip Instance { get; private set; }

        private Canvas _canvas;
        private Image[] _dots = new Image[18];
        private Text _label;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("DayDotStrip");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<DayDotStrip>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4680;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var container = new GameObject("Row");
            container.transform.SetParent(transform, false);
            var rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 40f);
            rt.anchoredPosition = new Vector2(0f, -100f);

            float startX = 30f;
            float gap = 36f;
            for (int i = 0; i < 18; i++)
            {
                var dotGO = new GameObject($"Dot_{i + 1}");
                dotGO.transform.SetParent(container.transform, false);
                var drt = dotGO.AddComponent<RectTransform>();
                drt.anchorMin = new Vector2(0f, 0f);
                drt.anchorMax = new Vector2(0f, 1f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.sizeDelta = new Vector2(20f, 20f);
                drt.anchoredPosition = new Vector2(startX + i * gap, 0f);
                var img = dotGO.AddComponent<Image>();
                img.color = new Color(0.30f, 0.20f, 0.10f, 0.85f);
                img.sprite = MakeDot();
                _dots[i] = img;
            }
        }

        private Sprite MakeDot()
        {
            var tex = new Texture2D(20, 20, TextureFormat.RGBA32, false);
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    float dx = (x - 10) / 10f;
                    float dy = (y - 10) / 10f;
                    float d = dx * dx + dy * dy;
                    if (d < 0.7f) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 20, 20), Vector2.one * 0.5f);
        }

        public void SetDay(int day, int versesHeard)
        {
            if (_dots[0] == null) Build();
            for (int i = 0; i < 18; i++)
            {
                if (i + 1 < day) _dots[i].color = new Color(0.95f, 0.70f, 0.20f, 0.95f);   // completed (gold)
                else if (i + 1 == day) _dots[i].color = new Color(0.95f, 0.30f, 0.20f, 1f); // current (red, pulses)
                else _dots[i].color = new Color(0.30f, 0.20f, 0.10f, 0.65f);             // future
            }
        }
    }
}
