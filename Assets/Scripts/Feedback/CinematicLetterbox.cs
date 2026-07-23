// Day 8 (RamayanaPS5) — CinematicLetterbox.
// GTA-style cinematic letterbox bars. Pure runtime UI, no prefab.
// sortOrder in the 4700s range to sit under dialogue but over world.

using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public sealed class CinematicLetterbox : MonoBehaviour
    {
        public static CinematicLetterbox Instance { get; private set; }

        [Header("Look")]
        [SerializeField, Range(0.04f, 0.18f)] private float barHeightFraction = 0.09f;
        [SerializeField] private Color barColor = new Color(0f, 0f, 0f, 0.92f);
        [SerializeField] private int sortOrder = 4705;

        private RectTransform _top;
        private RectTransform _bottom;
        private Canvas _canvas;
        private bool _visible = true;

        public static CinematicLetterbox EnsureCreated()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("CinematicLetterbox");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<CinematicLetterbox>();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Build();
            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_top != null) _top.gameObject.SetActive(visible);
            if (_bottom != null) _bottom.gameObject.SetActive(visible);
        }

        public void SetBarHeight(float fraction)
        {
            barHeightFraction = Mathf.Clamp(fraction, 0.04f, 0.18f);
            ApplyHeights();
        }

        // ── Internals ──────────────────────────────────────────────

        private void Build()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = sortOrder;

            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _top = MakeBar("TopBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            _bottom = MakeBar("BottomBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            ApplyHeights();
        }

        private RectTransform MakeBar(string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 100f);
            var img = go.AddComponent<Image>();
            img.color = barColor;
            img.raycastTarget = false;
            return rt;
        }

        private void ApplyHeights()
        {
            if (_top == null || _bottom == null) return;
            float h = Mathf.Max(40f, Screen.height * barHeightFraction);
            // Stretch full width via anchors left-right.
            StretchFullWidth(_top, top: true, h);
            StretchFullWidth(_bottom, top: false, h);
        }

        private static void StretchFullWidth(RectTransform rt, bool top, float height)
        {
            if (top)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
            }
            rt.offsetMin = new Vector2(0f, top ? -height : 0f);
            rt.offsetMax = new Vector2(0f, top ? 0f : height);
        }

        void OnRectTransformDimensionsChange()
        {
            if (_visible) ApplyHeights();
        }
    }
}
