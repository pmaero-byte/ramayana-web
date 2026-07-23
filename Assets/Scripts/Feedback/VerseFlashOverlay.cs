// Round 25 — VerseFlashOverlay: brief full-screen tint for success/error moments.
// Auto-creates a hidden GameObject + Canvas on first use; safe across many verses.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class VerseFlashOverlay : MonoBehaviour
    {
        public static VerseFlashOverlay Instance { get; private set; }
        private Image _image;
        private Coroutine _active;
        private Canvas _canvas;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("VerseFlashOverlay");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VerseFlashOverlay>();
            Instance.BuildOverlay();
        }

        private void BuildOverlay()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var imgGO = new GameObject("Flash");
            imgGO.transform.SetParent(transform, false);
            _image = imgGO.AddComponent<Image>();
            _image.color = new Color(1f, 1f, 1f, 0f);
            var rt = _image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public void Flash(Color color, float duration)
        {
            if (_image == null) BuildOverlay();
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(DoFlash(color, duration));
        }

        private IEnumerator DoFlash(Color color, float duration)
        {
            color.a = Mathf.Clamp01(color.a > 0 ? color.a : 0.25f);
            _image.color = color;
            // ease out
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = 1f - (t / duration);
                _image.color = new Color(color.r, color.g, color.b, color.a * k);
                yield return null;
            }
            _image.color = new Color(color.r, color.g, color.b, 0f);
            _active = null;
        }
    }
}
