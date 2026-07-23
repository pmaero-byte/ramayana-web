// Round 26 — VersesProgressHUD: bottom-of-screen progress strip that tracks
// total verses heard vs total available. Updates live; auto-creates a Canvas.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class VersesProgressHUD : MonoBehaviour
    {
        public static VersesProgressHUD Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Image _bar;
        private Text _count;
        private Text _day;
        private int _total;
        private int _heard;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("VersesProgressHUD");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VersesProgressHUD>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4700;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            // Bottom container
            var bottom = new GameObject("BottomBar");
            bottom.transform.SetParent(transform, false);
            var rt = bottom.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 90f);
            rt.anchoredPosition = Vector2.zero;

            _bg = bottom.AddComponent<Image>();
            _bg.color = new Color(0.04f, 0.02f, 0.01f, 0.45f);

            // Bar background
            var barBG = new GameObject("BarBG");
            barBG.transform.SetParent(bottom.transform, false);
            var bgRT = barBG.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0f);
            bgRT.anchorMax = new Vector2(1f, 0f);
            bgRT.pivot = new Vector2(0.5f, 0f);
            bgRT.sizeDelta = new Vector2(-40f, 14f);
            bgRT.anchoredPosition = new Vector2(0f, 50f);
            var bgImg = barBG.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.07f, 0.04f, 0.95f);

            // Bar fill
            var barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBG.transform, false);
            var fillRT = barFill.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(0f, 1f);
            fillRT.pivot = new Vector2(0f, 0.5f);
            fillRT.sizeDelta = new Vector2(0f, 0f);
            fillRT.anchoredPosition = Vector2.zero;
            _bar = barFill.AddComponent<Image>();
            _bar.color = new Color(0.95f, 0.65f, 0.20f, 0.95f);

            // Count text (left)
            _count = MakeText(bottom.transform, "Count", 24, TextAnchor.MiddleLeft,
                new Vector2(20f, 50f), new Vector2(280f, 30f));
            _count.text = "0 / 0";

            // Day text (right)
            _day = MakeText(bottom.transform, "Day", 26, TextAnchor.MiddleRight,
                new Vector2(-20f, 50f), new Vector2(280f, 30f));
            _day.text = "";
            _day.fontStyle = FontStyle.Bold;
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = new Color(1f, 0.86f, 0.55f, 1f);
            return t;
        }

        public void SetTotal(int total) { _total = total; Refresh(); }
        public void SetHeard(int heard) { _heard = heard; Refresh(); }
        public void SetDay(int day)
        {
            _day.text = day > 0 ? $"Day {day}" : "";
        }

        private void Refresh()
        {
            if (_count != null) _count.text = $"{_heard} / {(_total > 0 ? _total : _heard)}";
            if (_bar != null)
            {
                float pct = _total > 0 ? Mathf.Clamp01((float)_heard / _total) : 0f;
                var rt = _bar.rectTransform;
                rt.sizeDelta = new Vector2(pct * (rt.parent.GetComponent<RectTransform>().rect.width - 8f), 0f);
            }
        }
    }
}
