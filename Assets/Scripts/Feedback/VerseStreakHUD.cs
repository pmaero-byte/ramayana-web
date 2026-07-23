// Round 26 — VerseStreakHUD: shows consecutive success count at the right of the screen.
// Pulses on each new success. Resets on fail.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class VerseStreakHUD : MonoBehaviour
    {
        public static VerseStreakHUD Instance { get; private set; }

        private Canvas _canvas;
        private Text _streakText;
        private Image _bg;
        private int _streak = 0;
        private Coroutine _pulse;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("VerseStreakHUD");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<VerseStreakHUD>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4750;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var top = new GameObject("StreakBox");
            top.transform.SetParent(transform, false);
            var rt = top.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(220f, 70f);
            rt.anchoredPosition = new Vector2(-30f, -30f);

            _bg = top.AddComponent<Image>();
            _bg.color = new Color(0.04f, 0.02f, 0.01f, 0.5f);

            _streakText = new GameObject("Text").AddComponent<Text>();
            _streakText.transform.SetParent(top.transform, false);
            _streakText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _streakText.fontSize = 36;
            _streakText.alignment = TextAnchor.MiddleCenter;
            _streakText.color = new Color(1f, 0.85f, 0.50f, 1f);
            _streakText.fontStyle = FontStyle.Bold;
            var tRt = _streakText.rectTransform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            _streakText.text = "0 streak";
        }

        public void OnSuccess()
        {
            _streak += 1;
            Refresh();
            if (_pulse != null) StopCoroutine(_pulse);
            _pulse = StartCoroutine(PulseAnim());
        }

        public void OnFail()
        {
            _streak = 0;
            Refresh();
        }

        public int CurrentStreak => _streak;

        private void Refresh()
        {
            if (_streakText == null) Build();
            if (_streak == 0) _streakText.text = "";
            else if (_streak < 3) _streakText.text = _streak + " streak";
            else _streakText.text = _streak + "× streak!";
        }

        private IEnumerator PulseAnim()
        {
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = 1f + 0.30f * Mathf.Sin(t * 6.0f) * (1f - t / 0.45f);
                transform.localScale = new Vector3(k, k, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }
    }
}
