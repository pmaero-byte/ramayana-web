// Round 27 — WarDrumBeat: ambient low rumble + visual pulse that throbs with the day cycle.
// Stops automatically when day ends.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class WarDrumBeat : MonoBehaviour
    {
        public static WarDrumBeat Instance { get; private set; }

        private Canvas _canvas;
        private Image _ring;
        private Coroutine _run;
        private int _day = 1;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("WarDrumBeat");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<WarDrumBeat>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4500;

            // Outer ring centered on screen
            var ringGO = new GameObject("Ring");
            ringGO.transform.SetParent(transform, false);
            var rt = ringGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900f, 900f);
            rt.anchoredPosition = Vector2.zero;
            _ring = ringGO.AddComponent<Image>();
            _ring.color = new Color(0.95f, 0.40f, 0.20f, 0f);
        }

        public void StartDay(int day)
        {
            _day = day;
            if (_run != null) StopCoroutine(_run);
            _run = StartCoroutine(Beat());
        }

        private IEnumerator Beat()
        {
            float t = 0f;
            while (true)
            {
                t += Time.deltaTime;
                float k = (Mathf.Sin(t * 1.4f) + 1f) * 0.5f;  // 0..1
                float intensity = Mathf.Lerp(0.04f, 0.14f, k) * (1f - Mathf.Clamp01((_day - 1) / 18f));
                if (_ring != null)
                {
                    _ring.color = new Color(0.95f, 0.40f, 0.20f, intensity);
                    float scale = 0.92f + 0.06f * k;
                    _ring.rectTransform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }
        }
    }
}
