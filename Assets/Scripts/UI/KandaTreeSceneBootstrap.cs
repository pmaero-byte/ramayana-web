// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — KandaTree Scene Bootstrap (Day 23)
// Runtime bridge: exposes KandaTree navigation without touching Day 1-10
// MainMenuScreenController. Can be attached to a new overlay in the MainMenu
// scene, or used standalone with keyboard shortcuts.
//
// Usage:
//   - Attach to any GameObject in a scene.
//   - Calls KandaTree.TryLoadScene(kandaId) when a kanda is selected.
//   - Falls back to keyboard shortcuts if no UI is wired.
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Jambudweep.Ramayana.Gameplay;

namespace Jambudweep.Ramayana.UI
{
    [AddComponentMenu("Ramayana/Kanda Tree Bootstrap")]
    public sealed class KandaTreeSceneBootstrap : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Optional overlay for kanda selection (auto-created if null)")]
        [SerializeField] private RectTransform kandaOverlay;

        [Header("Events")]
        public UnityEvent<string> onKandaSelected = new UnityEvent<string>();

        private bool _overlayBuilt;
        private const int CardHeight = 90;
        private const int CardSpacing = 8;

        void Start()
        {
            if (kandaOverlay == null)
            {
                BuildOverlay();
            }
        }

        void Update()
        {
            // Keyboard shortcuts: number keys 1-8 select kandas by order.
            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) && i <= KandaTree.KandaCount)
                {
                    var entry = KandaTree.Entries[i - 1];
                    Jambudweep.Ramayana.UI.KandaLaunchBridge.Select(entry.KandaId);
                }
            }

            // Escape returns to MainMenu scene (assuming it exists).
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>Select a kanda by id and launch its scene.</summary>
        public void SelectKanda(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId)) return;

            var entry = KandaTree.GetEntry(kandaId);
            if (entry == null)
            {
                Debug.LogWarning($"[KandaTreeBootstrap] Unknown kanda: {kandaId}");
                return;
            }

            Debug.Log($"[KandaTreeBootstrap] Launching kanda: {entry.DisplayTitle} (scene={entry.SceneName})");
            onKandaSelected?.Invoke(kandaId);
            Jambudweep.Ramayana.UI.KandaLaunchBridge.Select(kandaId);
        }

        private void BuildOverlay()
        {
            if (_overlayBuilt) return;
            _overlayBuilt = true;

            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var go = new GameObject("KandaTreeOverlay");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 5000;
                go.AddComponent<GraphicRaycaster>();
                go.AddComponent<CanvasScaler>();
                kandaOverlay = go.GetComponent<RectTransform>();
            }
            else
            {
                var go = new GameObject("KandaTreeOverlay");
                go.transform.SetParent(canvas.transform, false);
                kandaOverlay = go.AddComponent<RectTransform>();
            }

            // Stretch to bottom of screen.
            kandaOverlay.anchorMin = new Vector2(0f, 0f);
            kandaOverlay.anchorMax = new Vector2(1f, 0f);
            kandaOverlay.pivot = new Vector2(0.5f, 0f);
            kandaOverlay.offsetMin = new Vector2(20f, 20f);
            kandaOverlay.offsetMax = new Vector2(-20f, -20f);

            var list = new GameObject("KandaList");
            list.transform.SetParent(kandaOverlay, false);
            var listRt = list.AddComponent<RectTransform>();
            Stretch(listRt);
            var layout = list.AddComponent<VerticalLayoutGroup>();
            layout.spacing = CardSpacing;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childForceExpandHeight = false;

            foreach (var entry in KandaTree.Entries)
            {
                BuildCard(list.transform, entry);
            }
        }

        private void BuildCard(Transform parent, KandaEntry entry)
        {
            var card = new GameObject($"Card_{entry.Order}");
            card.transform.SetParent(parent, false);

            var le = card.AddComponent<LayoutElement>();
            le.preferredHeight = CardHeight;

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.06f, 0.18f, 0.85f);

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => SelectKanda(entry.KandaId));

            var label = new GameObject("Label");
            label.transform.SetParent(card.transform, false);
            var labelRt = label.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var t = label.AddComponent<Text>();
            t.text = $"{entry.Order}. {entry.DisplayTitle} — {entry.Summary}";
            t.fontSize = 22;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = new Color(0.97f, 0.86f, 0.65f, 1f);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
