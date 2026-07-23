// Day 4 (RamayanaPS5) — SaveLoadHud.
// Minimal in-game Save / Load / Autosave controls. Three buttons on a
// strip at the top-center of the screen. Calls into the existing static
// SaveSystem (Core namespace) so the rest of the game stays untouched.
//
// Default slot keys: slot_0, slot_1, slot_2, autosave.

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Jambudweep.Ramayana.Core;

namespace Jambudweep.Ramayana.UI
{
    public sealed class SaveLoadHud : MonoBehaviour
    {
        [Header("Slot")]
        [SerializeField] private string slotKey = "autosave";

        [Header("Theme")]
        [SerializeField] private Color bgColor = new Color(0.05f, 0.02f, 0.10f, 0.65f);
        [SerializeField] private Color btnColor = new Color(0.16f, 0.10f, 0.22f, 0.95f);
        [SerializeField] private Color btnEdge = new Color(0.95f, 0.78f, 0.42f, 1f);

        [Header("Events")]
        public UnityEvent<string> onSaved = new UnityEvent<string>();
        public UnityEvent<string> onLoaded = new UnityEvent<string>();
        public UnityEvent<string> onSaveFailed = new UnityEvent<string>();

        private Canvas _canvas;
        private Text _status;
        private bool _built;

        void Start() => Build();

        public void Save()
        {
            var data = BuildSaveData();
            if (SaveSystem.Save(slotKey, data))
            {
                onSaved?.Invoke(slotKey);
                SetStatus($"✓ Saved to '{slotKey}'");
            }
            else
            {
                onSaveFailed?.Invoke(slotKey);
                SetStatus("✗ Save failed");
            }
        }

        public void Load()
        {
            var data = SaveSystem.Load(slotKey);
            if (data == null)
            {
                SetStatus($"No save in '{slotKey}'");
                return;
            }
            ApplySaveData(data);
            onLoaded?.Invoke(slotKey);
            SetStatus($"✓ Loaded '{slotKey}' (act={data.currentActId})");
        }

        public void DeleteSave()
        {
            if (SaveSystem.DeleteSlot(slotKey)) SetStatus($"Deleted '{slotKey}'");
        }

        public void SetSlot(string newSlot)
        {
            slotKey = string.IsNullOrEmpty(newSlot) ? "autosave" : newSlot;
            SetStatus($"Slot: {slotKey}");
        }

        // ── Internals ──────────────────────────────────────────────

        private SaveData BuildSaveData()
        {
            var engine = FindFirstObjectByType<Story.StoryEngine>();
            var state = engine != null ? engine.state : null;
            return new SaveData
            {
                saveVersion = 1,
                episodeId = engine?.episode != null ? engine.episode.episodeId : null,
                currentActId = state?.currentActId,
                currentMomentIndex = state?.currentMomentIndex ?? 0,
                selectedCharacterId = state?.selectedCharacterId,
                dharmaScore = state?.dharmaScore ?? 0,
                completedObjectiveIds = state?.completedObjectiveIds != null
                    ? state.completedObjectiveIds.ToArray() : Array.Empty<string>(),
                unlockedCharacterIds = state?.unlockedCharacterIds != null
                    ? state.unlockedCharacterIds.ToArray() : Array.Empty<string>(),
                collectedShlokaIds = state?.collectedShlokaIds != null
                    ? state.collectedShlokaIds.ToArray() : Array.Empty<string>(),
                collectedCollectibleIds = state?.collectedCollectibleIds != null
                    ? state.collectedCollectibleIds.ToArray() : Array.Empty<string>(),
                visitedScenes = state?.visitedScenes != null
                    ? state.visitedScenes.ConvertAll(s => s.ToString()).ToArray() : Array.Empty<string>(),
                totalPlayTimeSec = state?.totalPlayTime ?? 0f,
            };
        }

        private void ApplySaveData(SaveData data)
        {
            var engine = FindFirstObjectByType<Story.StoryEngine>();
            if (engine == null) return;
            engine.state.currentActId = data.currentActId;
            engine.state.currentMomentIndex = data.currentMomentIndex;
            engine.state.selectedCharacterId = data.selectedCharacterId;
            engine.state.dharmaScore = data.dharmaScore;
            engine.state.totalPlayTime = data.totalPlayTimeSec;
            engine.state.completedObjectiveIds.Clear();
            if (data.completedObjectiveIds != null)
                engine.state.completedObjectiveIds.AddRange(data.completedObjectiveIds);
            engine.state.unlockedCharacterIds.Clear();
            if (data.unlockedCharacterIds != null)
                engine.state.unlockedCharacterIds.AddRange(data.unlockedCharacterIds);
            engine.state.collectedShlokaIds.Clear();
            if (data.collectedShlokaIds != null)
                engine.state.collectedShlokaIds.AddRange(data.collectedShlokaIds);
        }

        private void Build()
        {
            if (_built) return;
            _built = true;

            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4800;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            gameObject.AddComponent<GraphicRaycaster>();

            // Bar container
            var bar = new GameObject("Bar");
            bar.transform.SetParent(transform, false);
            var barRt = bar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 1f);
            barRt.anchorMax = new Vector2(0.5f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.sizeDelta = new Vector2(720f, 100f);
            barRt.anchoredPosition = new Vector2(0f, -260f);
            var barImg = bar.AddComponent<Image>();
            barImg.color = bgColor;

            // Three buttons in a row
            BuildButton(bar.transform, "Save",  new Vector2(-200f, 0f), Save);
            BuildButton(bar.transform, "Load",  new Vector2(0f, 0f), Load);
            BuildButton(bar.transform, "Slot: " + slotKey, new Vector2(200f, 0f),
                () => SetSlot(SlotCycler()));

            // Status line below the bar
            var status = new GameObject("Status");
            status.transform.SetParent(transform, false);
            var srt = status.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.sizeDelta = new Vector2(720f, 32f);
            srt.anchoredPosition = new Vector2(0f, -370f);
            _status = status.AddComponent<Text>();
            _status.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _status.fontSize = 20;
            _status.alignment = TextAnchor.MiddleCenter;
            _status.color = new Color(0.95f, 0.78f, 0.42f, 1f);
            _status.text = "";
        }

        private void BuildButton(Transform parent, string label, Vector2 pos, Action onClick)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(180f, 60f);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = btnColor;

            // Edge accent line
            var edge = new GameObject("Edge");
            edge.transform.SetParent(go.transform, false);
            var ert = edge.AddComponent<RectTransform>();
            ert.anchorMin = new Vector2(0f, 0f);
            ert.anchorMax = new Vector2(1f, 0f);
            ert.pivot = new Vector2(0.5f, 0f);
            ert.sizeDelta = new Vector2(0f, 3f);
            ert.anchoredPosition = new Vector2(0f, 0f);
            var eimg = edge.AddComponent<Image>();
            eimg.color = btnEdge;

            var txt = new GameObject("Text").AddComponent<Text>();
            txt.transform.SetParent(go.transform, false);
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 22;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = btnEdge;
            txt.text = label;
            var trt = txt.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());
        }

        private void SetStatus(string text)
        {
            if (_status == null) Build();
            _status.text = text ?? "";
            Debug.Log("[SaveLoadHud] " + text);
        }

        private int _slotIndex;
        private static readonly string[] s_slots = { "autosave", "slot_0", "slot_1", "slot_2" };
        private string SlotCycler()
        {
            _slotIndex = (_slotIndex + 1) % s_slots.Length;
            return s_slots[_slotIndex];
        }
    }
}
