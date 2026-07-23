// Day 9 (RamayanaPS5) — DialogueOverlay (enhanced).
// Floating dialogue overlay: speaker name, portrait, line, optional sanskrit.
// Animates in/out via CanvasGroup. Driven by StoryMomentPlayer + MotionTriggeredDialogue.
// Day 9 adds EnsureCreated() runtime factory so Mac GTA bootstrap needs no prefab.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.UI
{
    public sealed class DialogueOverlay : MonoBehaviour
    {
        public static DialogueOverlay Instance { get; private set; }

        [Header("Required refs")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text lineText;
        [SerializeField] private Text sanskritText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image accentImage;

        [Header("Color theme")]
        [SerializeField] private Color panelColor = new Color(0.07f, 0.04f, 0.02f, 0.92f);
        [SerializeField] private Color accentColor = new Color(0.95f, 0.78f, 0.42f, 0.95f);

        [Header("Layout")]
        [SerializeField] private Vector2 anchorOffset = new Vector2(0f, 0f);
        [SerializeField] private int sortOrder = 4720;

        [Header("Timing")]
        [SerializeField, Range(0.05f, 1.5f)] private float fadeInSeconds = 0.18f;
        [SerializeField, Range(0.05f, 1.5f)] private float fadeOutSeconds = 0.45f;

        private Coroutine activeLoop;

        public static DialogueOverlay EnsureCreated()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<DialogueOverlay>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }
            var go = new GameObject("DialogueOverlay");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<DialogueOverlay>();
            Instance.BuildRuntime();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (group == null) BuildRuntime();
        }

        public void Show(string speakerId, string line, string sanskrit, Sprite portrait, float holdSeconds)
        {
            if (group == null) BuildRuntime();
            if (group == null) return;
            if (activeLoop != null) StopCoroutine(activeLoop);
            activeLoop = StartCoroutine(ShowRoutine(speakerId, line, sanskrit, portrait, holdSeconds));
        }

        public void Hide()
        {
            if (activeLoop != null) StopCoroutine(activeLoop);
            if (group != null) StartCoroutine(FadeOutRoutine());
        }

        // ── Runtime UI (GTA bottom-third cinematic panel) ──────────

        private void BuildRuntime()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // Panel root
            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.08f, 0.06f);
            panelRT.anchorMax = new Vector2(0.92f, 0.28f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = panelColor;
            group = panelGO.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            // Accent stripe (left gold bar)
            var accentGO = new GameObject("Accent");
            accentGO.transform.SetParent(panelGO.transform, false);
            var accentRT = accentGO.AddComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0f, 0f);
            accentRT.anchorMax = new Vector2(0f, 1f);
            accentRT.pivot = new Vector2(0f, 0.5f);
            accentRT.sizeDelta = new Vector2(8f, 0f);
            accentRT.anchoredPosition = Vector2.zero;
            accentImage = accentGO.AddComponent<Image>();
            accentImage.color = accentColor;

            // Portrait
            var portGO = new GameObject("Portrait");
            portGO.transform.SetParent(panelGO.transform, false);
            var portRT = portGO.AddComponent<RectTransform>();
            portRT.anchorMin = new Vector2(0f, 0.5f);
            portRT.anchorMax = new Vector2(0f, 0.5f);
            portRT.pivot = new Vector2(0f, 0.5f);
            portRT.sizeDelta = new Vector2(120f, 120f);
            portRT.anchoredPosition = new Vector2(24f, 0f);
            portraitImage = portGO.AddComponent<Image>();
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;

            // Speaker
            speakerNameText = MakeText(panelGO.transform, "Speaker",
                new Vector2(0.18f, 0.62f), new Vector2(0.96f, 0.92f), 28, FontStyle.Bold,
                new Color(0.95f, 0.85f, 0.55f, 1f));

            // Line
            lineText = MakeText(panelGO.transform, "Line",
                new Vector2(0.18f, 0.18f), new Vector2(0.96f, 0.62f), 22, FontStyle.Normal,
                new Color(0.95f, 0.92f, 0.88f, 1f));

            // Sanskrit subtitle
            sanskritText = MakeText(panelGO.transform, "Sanskrit",
                new Vector2(0.18f, 0.02f), new Vector2(0.96f, 0.18f), 16, FontStyle.Italic,
                new Color(0.78f, 0.70f, 0.55f, 1f));
            sanskritText.gameObject.SetActive(false);
        }

        private static Text MakeText(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        private IEnumerator ShowRoutine(string speakerId, string line, string sanskrit, Sprite portrait, float holdSeconds)
        {
            if (speakerNameText != null) speakerNameText.text = speakerId;
            if (lineText != null) lineText.text = line;
            if (sanskritText != null)
            {
                sanskritText.text = sanskrit ?? string.Empty;
                sanskritText.gameObject.SetActive(!string.IsNullOrEmpty(sanskrit));
            }
            if (portraitImage != null)
            {
                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                    portraitImage.color = Color.white;
                }
                else
                {
                    // Keep prior sprite or show neutral.
                    portraitImage.color = new Color(1f, 1f, 1f, 0.35f);
                }
            }
            if (accentImage != null) accentImage.color = accentColor;

            Image panel = group.GetComponent<Image>();
            if (panel != null) panel.color = panelColor;

            float t = 0f;
            while (t < fadeInSeconds)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Clamp01(t / fadeInSeconds);
                yield return null;
            }
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;

            yield return new WaitForSeconds(holdSeconds);
            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            if (group == null) yield break;
            float t = 0f;
            while (t < fadeOutSeconds)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Clamp01(1f - (t / fadeOutSeconds));
                yield return null;
            }
            group.alpha = 0f;
        }
    }
}
