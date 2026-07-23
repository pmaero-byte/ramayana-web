// Round 29 — EmbassyRecapHUD: brief recap of what was achieved in the embassy /
// formation / pilot. Shown after pilot phase and at start of war phase.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class EmbassyRecapHUD : MonoBehaviour
    {
        public static EmbassyRecapHUD Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Text _title;
        private Text _line1;
        private Text _line2;
        private Text _line3;
        private Coroutine _active;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("EmbassyRecapHUD");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<EmbassyRecapHUD>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4740;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var card = new GameObject("Card");
            card.transform.SetParent(transform, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800f, 320f);
            rt.anchoredPosition = Vector2.zero;
            _bg = card.AddComponent<Image>();
            _bg.color = new Color(0.05f, 0.02f, 0.01f, 0.7f);

            _title = MakeText(card.transform, "Title", 36, TextAnchor.MiddleCenter,
                new Vector2(0f, -20f), new Vector2(720f, 60f));
            _title.fontStyle = FontStyle.Bold;
            _title.text = "";

            _line1 = MakeText(card.transform, "L1", 24, TextAnchor.MiddleCenter,
                new Vector2(0f, -80f), new Vector2(720f, 36f));
            _line1.text = "";

            _line2 = MakeText(card.transform, "L2", 24, TextAnchor.MiddleCenter,
                new Vector2(0f, -120f), new Vector2(720f, 36f));
            _line2.text = "";

            _line3 = MakeText(card.transform, "L3", 24, TextAnchor.MiddleCenter,
                new Vector2(0f, -160f), new Vector2(720f, 36f));
            _line3.text = "";

            _title.canvasRenderer.SetAlpha(0f);
            _line1.canvasRenderer.SetAlpha(0f);
            _line2.canvasRenderer.SetAlpha(0f);
            _line3.canvasRenderer.SetAlpha(0f);
            _bg.canvasRenderer.SetAlpha(0f);
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = new Color(1f, 0.86f, 0.55f, 1f);
            return t;
        }

        public void Show(string title, string l1, string l2, string l3, float seconds = 5f)
        {
            if (_title == null) Build();
            _title.text = title ?? "";
            _line1.text = l1 ?? "";
            _line2.text = l2 ?? "";
            _line3.text = l3 ?? "";
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(Anim(seconds));
        }

        private IEnumerator Anim(float seconds)
        {
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = t / 0.6f;
                _bg.canvasRenderer.SetAlpha(k * 0.7f);
                _title.canvasRenderer.SetAlpha(k);
                _line1.canvasRenderer.SetAlpha(k);
                _line2.canvasRenderer.SetAlpha(k);
                _line3.canvasRenderer.SetAlpha(k);
                yield return null;
            }
            yield return new WaitForSeconds(seconds);
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = 1f - t / 0.6f;
                _bg.canvasRenderer.SetAlpha(k * 0.7f);
                _title.canvasRenderer.SetAlpha(k);
                _line1.canvasRenderer.SetAlpha(k);
                _line2.canvasRenderer.SetAlpha(k);
                _line3.canvasRenderer.SetAlpha(k);
                yield return null;
            }
            _bg.canvasRenderer.SetAlpha(0f);
            _title.canvasRenderer.SetAlpha(0f);
            _line1.canvasRenderer.SetAlpha(0f);
            _line2.canvasRenderer.SetAlpha(0f);
            _line3.canvasRenderer.SetAlpha(0f);
        }
    }
}
