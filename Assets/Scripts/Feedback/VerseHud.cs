// Round 25 — VerseHud: shows brief HUD for the active verse (verb + paraphrase) for
// 5-7 seconds on verse start, then fades out. Voice carries the actual narration.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class VerseHud : MonoBehaviour
    {
        public static VerseHud Instance { get; private set; }

        private Canvas _canvas;
        private Text _header;
        private Text _verb;
        private Image _pulseRing;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("VerseHud");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VerseHud>();
            Instance.BuildUI();
        }

        private void BuildUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4800;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            // Container at top
            var top = new GameObject("Top");
            top.transform.SetParent(transform, false);
            var rt = top.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 130f);
            rt.anchoredPosition = new Vector2(0f, -50f);

            var bg = top.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.02f, 0.01f, 0.32f);

            // Day / Parva caption
            _header = MakeText(top.transform, "Header", 26, TextAnchor.MiddleLeft,
                new Vector2(20f, 65f), new Vector2(900f, 35f));
            _header.text = "";

            _verb = MakeText(top.transform, "Verb", 56, TextAnchor.MiddleLeft,
                new Vector2(20f, 25f), new Vector2(900f, 60f));
            _verb.text = "";
            _verb.fontStyle = FontStyle.Bold;

            // Pulse ring around the verb
            var ringGO = new GameObject("Pulse");
            ringGO.transform.SetParent(_verb.transform, false);
            var rrt = ringGO.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0f, 0f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.offsetMin = new Vector2(-12f, -8f);
            rrt.offsetMax = new Vector2(12f, 8f);
            _pulseRing = ringGO.AddComponent<Image>();
            _pulseRing.color = new Color(0.95f, 0.70f, 0.20f, 0f);

            // Hide initially
            HideAll();
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = new Color(1f, 0.86f, 0.55f, 1f);
            t.supportRichText = true;
            return t;
        }

        private void HideAll()
        {
            if (_header != null) _header.canvasRenderer.SetAlpha(0f);
            if (_verb != null) _verb.canvasRenderer.SetAlpha(0f);
            if (_pulseRing != null) _pulseRing.canvasRenderer.SetAlpha(0f);
        }

        public void ShowVerse(string dayLabel, string verbName, float seconds = 6f)
        {
            if (_header == null) BuildUI();
            _header.text = string.IsNullOrEmpty(dayLabel) ? "" : dayLabel;
            _verb.text = string.IsNullOrEmpty(verbName) ? "" : verbName.ToUpper();
            StopAllCoroutines();
            StartCoroutine(PulseAndFade(seconds));
        }

        private IEnumerator PulseAndFade(float seconds)
        {
            float t = 0f;
            // ease in
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = t / 0.6f;
                _header.canvasRenderer.SetAlpha(k);
                _verb.canvasRenderer.SetAlpha(k);
                _pulseRing.canvasRenderer.SetAlpha((1f - k) * 0.6f);
                yield return null;
            }
            // pulse while visible
            float held = seconds - 1.2f;
            while (held > 0)
            {
                held -= Time.deltaTime;
                float pulse = 0.20f + 0.18f * Mathf.Sin(Time.time * 5f);
                _pulseRing.color = new Color(0.95f, 0.70f, 0.20f, pulse);
                _pulseRing.transform.localScale = Vector3.one * (1.0f + 0.05f * Mathf.Sin(Time.time * 5f));
                yield return null;
            }
            // fade out
            float u = 0f;
            while (u < 0.6f)
            {
                u += Time.deltaTime;
                float k = 1f - u / 0.6f;
                _header.canvasRenderer.SetAlpha(k);
                _verb.canvasRenderer.SetAlpha(k);
                _pulseRing.canvasRenderer.SetAlpha(0f);
                yield return null;
            }
            HideAll();
        }
    }
}
