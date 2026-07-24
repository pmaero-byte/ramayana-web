// Day 1 (RamayanaPS5) — MainMenuScreenController.
// Loads corpus_data.json from Resources and renders 8 act (kanda) cards
// the player can tap to begin that kanda. Matches the
// TitleScreenTapZone / VerseSceneScreenController pattern: a
// MonoBehaviour with a Build() factory that constructs the UI at
// runtime, no inspector wiring required.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Jambudweep.Ramayana.Gameplay;

namespace Jambudweep.Ramayana.UI
{
    public sealed class MainMenuScreenController : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("corpus_data.json lives at Assets/Resources/corpus_data.json")]
        [SerializeField] private string corpusResourcePath = "corpus_data";

        [Header("Theme")]
        [SerializeField] private Color bgColor = new Color(0.05f, 0.02f, 0.10f, 0.96f);
        [SerializeField] private Color titleColor = new Color(0.95f, 0.78f, 0.42f, 1f);
        [SerializeField] private Color cardColor = new Color(0.12f, 0.06f, 0.18f, 0.85f);
        [SerializeField] private Color cardEdgeColor = new Color(0.85f, 0.65f, 0.35f, 1f);

        [Header("Layout")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [Header("Events")]
        public UnityEvent<string> onKandaSelected = new UnityEvent<string>();

        private Canvas _canvas;
        private bool _built;

        [Serializable]
        private class CorpusData
        {
            public List<ActRecord> acts;
        }

        [Serializable]
        private class ActRecord
        {
            public string actId;
            public int actNumber;
            public string title;
            public string location;
            public string playerRole;
            public string setup;
            public string lesson;
        }

        void Start()
        {
            Build();
            Populate();
        }

        private void Build()
        {
            Debug.Log("[MainMenu] Build() start");
            if (_built)
            {
                Debug.Log("[MainMenu] Build() already built, returning");
                return;
            }
            _built = true;
            Debug.Log("[MainMenu] Build() proceeding");
            try
            {
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var es = new GameObject("EventSystem");
                    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    Debug.Log("[MainMenu] created EventSystem");
                }
                // Removed debug overlay to avoid blocking card clicks.
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 5000;
            }

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = referenceResolution;
            }

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // Background panel
            var bg = new GameObject("Bg");
            bg.transform.SetParent(transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            Stretch(bgRt);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = bgColor;

            // Title bar
            var titleBar = MakePanel("TitleBar", new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -200f));
            var titleText = MakeText(titleBar.transform, "Title", 56, TextAnchor.MiddleCenter,
                new Vector2(0f, -110f), new Vector2(900f, 100f));
            titleText.text = "Rāmāyaṇa — Vālmīki";
            titleText.color = titleColor;
            titleText.fontStyle = FontStyle.Bold;

            var sub = MakeText(titleBar.transform, "Sub", 24, TextAnchor.MiddleCenter,
                new Vector2(0f, -180f), new Vector2(900f, 40f));
            sub.text = "Choose a kāṇḍa to begin";
            sub.color = new Color(0.85f, 0.75f, 0.65f, 1f);

            // Bottom bar: version only until shared BuildSmallButton helper exists.
            var bottomBar = MakePanel("BottomBar", new Vector2(0f, 0f),
                new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 90f));
            var verText = MakeText(bottomBar.transform, "Ver", 18, TextAnchor.LowerRight,
                new Vector2(-20f, 15f), new Vector2(300f, 30f));
            verText.text = "v0.1.0";
            verText.color = new Color(0.55f, 0.50f, 0.45f, 1f);

            BuildCardList();
            Debug.Log("[MainMenu] Build() UI construction complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[MainMenu] Build() exception: " + ex);
            }
            Debug.Log("[MainMenu] Build() end");
        }

        private void BuildCardList()
        {
            var list = new GameObject("KandaList");
            list.transform.SetParent(transform, false);
            var listRt = list.AddComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.pivot = new Vector2(0.5f, 0.5f);
            listRt.offsetMin = new Vector2(40f, 80f);
            listRt.offsetMax = new Vector2(-40f, -260f);
            var listLayout = list.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 18f;
            listLayout.padding = new RectOffset(20, 20, 20, 20);
            listLayout.childAlignment = TextAnchor.UpperCenter;
            listLayout.childForceExpandHeight = false;
            listLayout.childForceExpandWidth = true;
            listLayout.childControlHeight = false;
            listLayout.childControlWidth = true;
        }

        private void Populate()
        {
            Debug.Log("[MainMenu] Populate() start");
            var ta = Resources.Load<TextAsset>(corpusResourcePath);
            if (ta == null)
            {
                Debug.LogWarning($"[MainMenu] corpus_data.json not found at Resources/{corpusResourcePath}");
                return;
            }
            CorpusData data;
            try { data = JsonUtility.FromJson<CorpusData>(ta.text); }
            catch (Exception e) { Debug.LogError($"[MainMenu] corpus parse failed: {e.Message}"); return; }
            if (data?.acts == null) { Debug.LogWarning("[MainMenu] data.acts is null"); return; }
            Debug.Log($"[MainMenu] corpus loaded: acts={data.acts.Count}");

            var list = transform.Find("KandaList");
            if (list == null) { Debug.LogWarning("[MainMenu] KandaList not found"); return; }
            Debug.Log($"[MainMenu] KandaList found, populating {data.acts.Count} cards");

            foreach (var act in data.acts)
            {
                if (act == null) continue;
                BuildCard(list.transform, act);
            }
        }

        private void BuildCard(Transform parent, ActRecord act)
        {
            var card = new GameObject($"Card_{act.actNumber}");
            card.transform.SetParent(parent, false);
            var cardRt = card.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0f, 1f);
            cardRt.anchorMax = new Vector2(1f, 1f);
            cardRt.pivot = new Vector2(0.5f, 1f);
            cardRt.anchoredPosition = new Vector2(0f, -20f);
            cardRt.sizeDelta = new Vector2(-40f, 220f);

            var le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 220f;

            var bg = card.AddComponent<Image>();
            bg.color = cardColor;
            bg.raycastTarget = true;

            // Left accent stripe
            var stripe = new GameObject("Stripe");
            stripe.transform.SetParent(card.transform, false);
            var stripeRt = stripe.AddComponent<RectTransform>();
            stripeRt.anchorMin = new Vector2(0f, 0f);
            stripeRt.anchorMax = new Vector2(0f, 1f);
            stripeRt.pivot = new Vector2(0f, 0.5f);
            stripeRt.sizeDelta = new Vector2(8f, 0f);
            stripeRt.anchoredPosition = new Vector2(12f, 0f);
            var stripeImg = stripe.AddComponent<Image>();
            stripeImg.color = cardEdgeColor;

            // Text helper using local card-relative anchors so text scales with card width.
            Text MakeCardText(string name, int size, TextAnchor align, Vector2 anchorMin, Vector2 anchorMax, Vector2 offset)
            {
                var go = new GameObject(name);
                go.transform.SetParent(card.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = offset;
                rt.offsetMax = new Vector2(-offset.x, offset.y);
                var t = go.AddComponent<Text>();
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = size;
                t.alignment = align;
                t.color = Color.white;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Truncate;
                return t;
            }

            var numText = MakeCardText("Num", 56, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0.18f, 1f), new Vector2(14f, -80f));
            numText.text = act.actNumber.ToString();
            numText.fontStyle = FontStyle.Bold;
            numText.color = cardEdgeColor;

            var titleText = MakeCardText("Title", 30, TextAnchor.MiddleLeft,
                new Vector2(0.18f, 1f), new Vector2(1f, 0.65f), new Vector2(-14f, -30f));
            titleText.text = act.title ?? "";
            titleText.color = new Color(0.97f, 0.86f, 0.65f, 1f);
            titleText.fontStyle = FontStyle.Bold;

            var locText = MakeCardText("Location", 22, TextAnchor.MiddleLeft,
                new Vector2(0.18f, 0.65f), new Vector2(1f, 0.45f), new Vector2(-14f, 0f));
            locText.text = string.IsNullOrEmpty(act.location) ? "" : "📍 " + act.location;
            locText.color = new Color(0.80f, 0.72f, 0.62f, 1f);

            var setupText = MakeCardText("Setup", 22, TextAnchor.UpperLeft,
                new Vector2(0.18f, 0.45f), new Vector2(1f, 0f), new Vector2(-14f, 10f));
            setupText.text = act.setup ?? "";
            setupText.color = new Color(0.86f, 0.78f, 0.70f, 1f);
            setupText.horizontalOverflow = HorizontalWrapMode.Wrap;
            setupText.verticalOverflow = VerticalWrapMode.Truncate;

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.ColorTint;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
            btn.colors = new ColorBlock
            {
                normalColor = new Color(0.12f, 0.06f, 0.18f, 1f),
                highlightedColor = new Color(0.22f, 0.14f, 0.28f, 1f),
                pressedColor = new Color(0.30f, 0.20f, 0.40f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };
            string kandaId = ResolveKandaId(act.actId);
            bool unlocked = KandaPermissions.IsUnlocked(kandaId);
            if (!unlocked)
            {
                btn.interactable = false;
                titleText.color = new Color(0.55f, 0.52f, 0.50f, 1f);
                setupText.color = new Color(0.55f, 0.52f, 0.50f, 1f);
                numText.color = new Color(0.55f, 0.52f, 0.50f, 1f);
                bg.color = new Color(0.25f, 0.22f, 0.20f, 1f);
            }
            else
            {
                btn.interactable = true;
            }
            btn.onClick.AddListener(() => SelectKanda(act.actId, act.title));
            Debug.Log($"[MainMenu] card built: {act.actNumber} {act.title} unlocked={unlocked}");
        }

        private string ResolveKandaId(string actId)
        {
            if (string.IsNullOrEmpty(actId))
                return actId;
            return Jambudweep.Ramayana.UI.KandaLaunchBridge.ResolveKandaId(actId);
        }

        private void SelectKanda(string actId, string title)
        {
            Debug.Log($"[MainMenu] Kanda selected: {actId} ({title})");
            onKandaSelected?.Invoke(actId);
            Jambudweep.Ramayana.UI.KandaLaunchBridge.Select(actId);
        }

        private void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private GameObject MakePanel(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            return go;
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            return t;
        }

        private void OnResumeClicked()
        {
            var mostRecent = Jambudweep.Ramayana.Core.SaveSystem.GetMostRecentSave();
            if (mostRecent != null && !string.IsNullOrEmpty(mostRecent.currentActId))
            {
                Debug.Log($"[MainMenu] Resuming save: act={mostRecent.currentActId}, char={mostRecent.selectedCharacterId}");
                Jambudweep.Ramayana.UI.KandaLaunchBridge.Select(mostRecent.currentActId);
            }
            else
            {
                Debug.LogWarning("[MainMenu] No save found to resume.");
            }
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenu] New game selected — returning to TitleScreen.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("[MainMenu] mouse down at " + Input.mousePosition);
            }
            if (Input.anyKeyDown)
            {
                Debug.Log("[MainMenu] anyKeyDown: " + Input.inputString);
            }
        }
    }
}
