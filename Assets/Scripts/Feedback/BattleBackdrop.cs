// Round 29 — BattleBackdrop: subtle battlefield silhouette that fades in on each verse.
// Player rig stays free; backdrop is pure background.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class BattleBackdrop : MonoBehaviour
    {
        public static BattleBackdrop Instance { get; private set; }

        private Canvas _canvas;
        private Image _image;
        private SpriteRenderer _r;
        private Coroutine _active;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("BattleBackdrop");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<BattleBackdrop>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1000; // far behind everything

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var imgGO = new GameObject("Backdrop");
            imgGO.transform.SetParent(transform, false);
            var rt = imgGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _image = imgGO.AddComponent<Image>();
            _image.color = new Color(0.0f, 0.0f, 0.0f, 0f);
        }

        public void FlashTone(Color tint, float seconds = 2.5f)
        {
            if (_image == null) Build();
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(FlashAnim(tint, seconds));
        }

        private IEnumerator FlashAnim(Color tint, float seconds)
        {
            float t = 0f;
            Color baseColor = new Color(tint.r, tint.g, tint.b, 0.18f);
            Color fadeOut = new Color(tint.r, tint.g, tint.b, 0f);
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = t / 0.6f;
                _image.color = Color.Lerp(fadeOut, baseColor, k);
                yield return null;
            }
            yield return new WaitForSeconds(seconds);
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float k = t / 0.6f;
                _image.color = Color.Lerp(baseColor, fadeOut, k);
                yield return null;
            }
            _image.color = fadeOut;
        }
    }
}
