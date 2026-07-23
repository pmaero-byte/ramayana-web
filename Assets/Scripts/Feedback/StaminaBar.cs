// Round 28 — StaminaBar: a circular stamina ring around the screen that fills on
// success and drains on fail. Empty ring = low stamina = lighter feedback.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class StaminaBar : MonoBehaviour
    {
        public static StaminaBar Instance { get; private set; }

        private Canvas _canvas;
        private Image _ring;
        private float _stamina = 0.6f;
        public float Stamina => _stamina;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("StaminaBar");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<StaminaBar>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4505;

            var ringGO = new GameObject("Ring");
            ringGO.transform.SetParent(transform, false);
            var rt = ringGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1200f, 1200f);
            rt.anchoredPosition = Vector2.zero;
            _ring = ringGO.AddComponent<Image>();
            _ring.color = new Color(0.95f, 0.65f, 0.20f, 0.10f);
            // Make it a ring: set fillCenter=false, fillMethod=Radial360, fillAmount
            _ring.type = Image.Type.Filled;
            _ring.fillMethod = Image.FillMethod.Radial360;
            _ring.fillOrigin = (int)Image.Origin360.Top;
            _ring.fillClockwise = true;
            _ring.fillAmount = 0.6f;
        }

        public void OnSuccess() { _stamina = Mathf.Clamp01(_stamina + 0.10f); Refresh(); }
        public void OnFail()    { _stamina = Mathf.Clamp01(_stamina - 0.18f); Refresh(); }

        private void Refresh()
        {
            if (_ring == null) Build();
            // Color shifts red as stamina drops
            _ring.color = _stamina > 0.5f
                ? new Color(0.45f, 0.85f, 0.45f, 0.10f)
                : _stamina > 0.2f
                    ? new Color(0.95f, 0.75f, 0.20f, 0.10f)
                    : new Color(0.95f, 0.30f, 0.20f, 0.10f);
            _ring.fillAmount = _stamina;
        }
    }
}
