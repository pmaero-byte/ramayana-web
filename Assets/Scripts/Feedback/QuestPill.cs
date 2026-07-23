// Round 31 — QuestPill: lower-right pill showing the current active
// objective count + a single one-line hint. Sourced from StoryEngine's
// completedObjectiveIds + the current StoryMomentPlayer.CurrentObjectiveId.
// Distinct from VersesProgressHUD (which tracks total verses heard).

using UnityEngine;
using UnityEngine.UI;
using Jambudweep.Ramayana.Story;

namespace Jambudweep.Ramayana.Feedback
{
    public sealed class QuestPill : MonoBehaviour
    {
        public static QuestPill Instance { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private Text _title;
        private Text _hint;
        private StoryEngine _engine;
        private StoryMomentPlayer _player;
        private int _lastCompletedCount = -1;
        private string _lastObjectiveId = "";

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("QuestPill");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<QuestPill>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4710;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var pill = new GameObject("Pill");
            pill.transform.SetParent(transform, false);
            var rt = pill.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(280f, 70f);
            rt.anchoredPosition = new Vector2(-20f, 100f);

            _bg = pill.AddComponent<Image>();
            _bg.color = new Color(0.07f, 0.04f, 0.02f, 0.78f);

            _title = MakeText(pill.transform, "Title", 14, TextAnchor.UpperLeft,
                new Vector2(10f, 4f), new Vector2(260f, 18f));
            _title.text = "Quest";
            _title.color = new Color(0.95f, 0.85f, 0.55f, 1f);

            _hint = MakeText(pill.transform, "Hint", 16, TextAnchor.LowerLeft,
                new Vector2(10f, 6f), new Vector2(260f, 44f));
            _hint.text = "—";
            _hint.color = new Color(0.95f, 0.92f, 0.85f, 1f);
        }

        private Text MakeText(Transform parent, string name, int fontPx, TextAnchor align,
            Vector2 pos, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = pos;
            rt.offsetMax = new Vector2(pos.x + rectSize.x, pos.y + rectSize.y);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontPx;
            t.alignment = align;
            t.color = Color.white;
            return t;
        }

        void Awake()
        {
            if (_engine == null) _engine = FindFirstObjectByType<StoryEngine>();
            if (_player == null) _player = FindFirstObjectByType<StoryMomentPlayer>();
        }

        public void SetEngine(StoryEngine engine) { _engine = engine; }
        public void SetMomentPlayer(StoryMomentPlayer player) { _player = player; }

        void Update()
        {
            if (_engine == null) _engine = FindFirstObjectByType<StoryEngine>();
            if (_player == null) _player = FindFirstObjectByType<StoryMomentPlayer>();
            int completed = _engine != null ? _engine.state.completedObjectiveIds.Count : 0;
            string objectiveId = _player != null ? _player.CurrentObjectiveId : null;
            if (completed != _lastCompletedCount)
            {
                _lastCompletedCount = completed;
                int total = _player != null ? _player.TotalObjectives : 0;
                _title.text = total > 0
                    ? $"Quest  ({completed}/{total})"
                    : "Quest";
            }
            if (objectiveId != _lastObjectiveId)
            {
                _lastObjectiveId = objectiveId;
                _hint.text = !string.IsNullOrEmpty(objectiveId) ? objectiveId : "—";
            }
        }
    }
}