// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Safe Area Overlay (Day 17)
// Runtime UI overlay that respects Screen.safeArea for notch / Dynamic Island /
// iPad split-view / landscape cutouts. Provides:
//   - Root Canvas that constrains children to the safe rectangle
//   - Visible "notch bar" at top + "home indicator" at bottom (decorative,
//     semi-transparent, can be hidden per platform)
//   - Public API for callers to inset their own RectTransforms (so other
//     HUDs can register and get safe-area respected without manual padding)
//
// Style matches Day 1-10 + Day 14/15/16 patterns:
//   - public sealed class X : MonoBehaviour
//   - public static Instance + EnsureCreated() factory
//   - private Build() at runtime (no prefab wiring)
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.UI
{
    public sealed class SafeAreaOverlay : MonoBehaviour
    {
        public static SafeAreaOverlay Instance { get; private set; }

        [Header("Visual")]
        [Tooltip("Show decorative notch / home-indicator bars over the unsafe area.")]
        [SerializeField] private bool showNotchBars = true;
        [SerializeField] private Color notchBarColor = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private Color homeBarColor = new Color(0f, 0f, 0f, 0.85f);

        [Header("Canvas")]
        [SerializeField] private int sortOrder = 4800; // above all other HUDs
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        // ── Runtime state ──────────────────────────────────────────────────
        private RectTransform _rootSafe;     // container constrained to Screen.safeArea
        private RectTransform _notchBar;     // top decorative bar
        private RectTransform _homeBar;      // bottom decorative bar
        private Canvas _canvas;
        private readonly List<RectTransform> _externalListeners = new List<RectTransform>();

        public Rect SafeArea => Screen.safeArea;
        public bool IsReady => _rootSafe != null;

        // ── Lifecycle ──────────────────────────────────────────────────────

        public static SafeAreaOverlay EnsureCreated()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<SafeAreaOverlay>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }
            var go = new GameObject("SafeAreaOverlay");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<SafeAreaOverlay>();
            Instance.Build();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (_canvas == null) Build();
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Register an external RectTransform so it stays inside the safe area.
        /// The transform's anchorMin/anchorMax are snapped to the current safe rect.
        /// </summary>
        public void RegisterExternal(RectTransform rt)
        {
            if (rt == null) return;
            if (!_externalListeners.Contains(rt)) _externalListeners.Add(rt);
            SnapExternal(rt);
        }

        public void UnregisterExternal(RectTransform rt)
        {
            if (rt == null) return;
            _externalListeners.Remove(rt);
        }

        public void SetShowNotchBars(bool show)
        {
            showNotchBars = show;
            if (_notchBar != null) _notchBar.gameObject.SetActive(show);
            if (_homeBar != null) _homeBar.gameObject.SetActive(show);
        }

        // ── Build ──────────────────────────────────────────────────────────

        private void Build()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = sortOrder;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // Root container — anchored to Screen.safeArea
            var rootGO = new GameObject("RootSafe");
            rootGO.transform.SetParent(transform, false);
            _rootSafe = rootGO.AddComponent<RectTransform>();
            _rootSafe.anchorMin = Vector2.zero;
            _rootSafe.anchorMax = Vector2.one;
            _rootSafe.offsetMin = Vector2.zero;
            _rootSafe.offsetMax = Vector2.zero;

            // Top notch bar — stretched to top unsafe area
            var notchGO = new GameObject("NotchBar");
            notchGO.transform.SetParent(_rootSafe, false);
            _notchBar = notchGO.AddComponent<RectTransform>();
            _notchBar.anchorMin = new Vector2(0f, 1f);
            _notchBar.anchorMax = new Vector2(1f, 1f);
            _notchBar.pivot = new Vector2(0.5f, 1f);
            _notchBar.sizeDelta = new Vector2(0f, 0f);
            _notchBar.anchoredPosition = Vector2.zero;
            var notchImg = notchGO.AddComponent<Image>();
            notchImg.color = notchBarColor;
            notchImg.raycastTarget = false;

            // Bottom home indicator bar — stretched to bottom unsafe area
            var homeGO = new GameObject("HomeBar");
            homeGO.transform.SetParent(_rootSafe, false);
            _homeBar = homeGO.AddComponent<RectTransform>();
            _homeBar.anchorMin = new Vector2(0f, 0f);
            _homeBar.anchorMax = new Vector2(1f, 0f);
            _homeBar.pivot = new Vector2(0.5f, 0f);
            _homeBar.sizeDelta = new Vector2(0f, 0f);
            _homeBar.anchoredPosition = Vector2.zero;
            var homeImg = homeGO.AddComponent<Image>();
            homeImg.color = homeBarColor;
            homeImg.raycastTarget = false;

            ApplySafeArea();
        }

        // ── Internals ──────────────────────────────────────────────────────

        private void ApplySafeArea()
        {
            if (_rootSafe == null) return;
            Rect sa = Screen.safeArea;

            // Convert safe rect (in screen pixels) to normalized 0-1 anchors on
            // the parent canvas (ScreenSpaceOverlay).
            Vector2 anchorMin = sa.position;
            Vector2 anchorMax = sa.position + sa.size;
            if (Screen.width > 0 && Screen.height > 0)
            {
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;
            }

            _rootSafe.anchorMin = anchorMin;
            _rootSafe.anchorMax = anchorMax;
            _rootSafe.offsetMin = Vector2.zero;
            _rootSafe.offsetMax = Vector2.zero;

            // Notch bar — covers the unsafe area above the safe rect
            if (_notchBar != null)
            {
                float topUnsafePx = (anchorMax.y > 0f && anchorMax.y < 1f)
                    ? Screen.height * (1f - anchorMax.y) : 0f;
                _notchBar.sizeDelta = new Vector2(0f, topUnsafePx);
                _notchBar.gameObject.SetActive(showNotchBars && topUnsafePx > 0.5f);
            }

            // Home bar — covers the unsafe area below the safe rect
            if (_homeBar != null)
            {
                float botUnsafePx = (anchorMin.y > 0f)
                    ? Screen.height * anchorMin.y : 0f;
                _homeBar.sizeDelta = new Vector2(0f, botUnsafePx);
                _homeBar.gameObject.SetActive(showNotchBars && botUnsafePx > 0.5f);
            }

            // Re-snap any external listeners
            for (int i = 0; i < _externalListeners.Count; i++)
            {
                SnapExternal(_externalListeners[i]);
            }
        }

        private void SnapExternal(RectTransform rt)
        {
            if (rt == null || _rootSafe == null) return;
            // For external listeners, snap their anchors to the safe rect so they
            // automatically resize when the screen rotates. This is best-effort:
            // callers can override after registration if they need specific anchors.
            Rect sa = Screen.safeArea;
            Vector2 aMin = sa.position;
            Vector2 aMax = sa.position + sa.size;
            if (Screen.width > 0 && Screen.height > 0)
            {
                aMin.x /= Screen.width;
                aMin.y /= Screen.height;
                aMax.x /= Screen.width;
                aMax.y /= Screen.height;
            }
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}