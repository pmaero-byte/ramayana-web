// Round 28 — TributeToFallenHUD: shows the most recent fallen commander with a soft
// black-and-white portrait card. Floats on the right edge of the screen briefly.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class TributeToFallenHUD : MonoBehaviour
    {
        public static TributeToFallenHUD Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Text _name;
        private Text _quote;
        private Coroutine _active;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("TributeToFallenHUD");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<TributeToFallenHUD>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4730;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var card = new GameObject("Card");
            card.transform.SetParent(transform, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 180f);
            rt.anchoredPosition = new Vector2(-30f, 0f);

            _bg = card.AddComponent<Image>();
            _bg.color = new Color(0.04f, 0.02f, 0.01f, 0.55f);

            // Vertical band on the left (red, like blood)
            var band = new GameObject("Band");
            band.transform.SetParent(card.transform, false);
            var brt = band.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(0f, 1f);
            brt.pivot = new Vector2(0f, 0.5f);
            brt.sizeDelta = new Vector2(6f, 0f);
            brt.anchoredPosition = Vector2.zero;
            var bImg = band.AddComponent<Image>();
            bImg.color = new Color(0.95f, 0.30f, 0.20f, 0.95f);

            _name = new GameObject("Name").AddComponent<Text>();
            _name.transform.SetParent(card.transform, false);
            _name.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _name.fontSize = 32;
            _name.alignment = TextAnchor.MiddleLeft;
            _name.color = new Color(1f, 0.85f, 0.55f, 1f);
            _name.fontStyle = FontStyle.Bold;
            _name.text = "";
            var nrt = _name.rectTransform;
            nrt.anchorMin = new Vector2(0f, 1f);
            nrt.anchorMax = new Vector2(1f, 1f);
            nrt.pivot = new Vector2(0f, 1f);
            nrt.anchoredPosition = new Vector2(28f, -20f);
            nrt.sizeDelta = new Vector2(-32f, 50f);

            _quote = new GameObject("Quote").AddComponent<Text>();
            _quote.transform.SetParent(card.transform, false);
            _quote.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quote.fontSize = 22;
            _quote.alignment = TextAnchor.UpperLeft;
            _quote.color = new Color(0.85f, 0.75f, 0.55f, 1f);
            _quote.text = "";
            var qrt = _quote.rectTransform;
            qrt.anchorMin = new Vector2(0f, 0f);
            qrt.anchorMax = new Vector2(1f, 0f);
            qrt.pivot = new Vector2(0f, 0f);
            qrt.anchoredPosition = new Vector2(28f, 20f);
            qrt.sizeDelta = new Vector2(-32f, 110f);

            _name.canvasRenderer.SetAlpha(0f);
            _quote.canvasRenderer.SetAlpha(0f);
            _bg.canvasRenderer.SetAlpha(0f);
        }

        public void Show(string name, string quote, float seconds = 6f)
        {
            if (_name == null) Build();
            _name.text = name ?? "";
            _quote.text = quote ?? "";
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(ShowAnim(seconds));
        }

        private IEnumerator ShowAnim(float seconds)
        {
            float t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float k = t / 0.7f;
                _name.canvasRenderer.SetAlpha(k);
                _quote.canvasRenderer.SetAlpha(k * 0.8f);
                _bg.canvasRenderer.SetAlpha(k * 0.55f);
                yield return null;
            }
            yield return new WaitForSeconds(seconds);
            float u = 0f;
            while (u < 0.7f)
            {
                u += Time.deltaTime;
                float k = 1f - u / 0.7f;
                _name.canvasRenderer.SetAlpha(k);
                _quote.canvasRenderer.SetAlpha(k * 0.8f);
                _bg.canvasRenderer.SetAlpha(k * 0.55f);
                yield return null;
            }
            _name.canvasRenderer.SetAlpha(0f);
            _quote.canvasRenderer.SetAlpha(0f);
            _bg.canvasRenderer.SetAlpha(0f);
        }
    }
}
