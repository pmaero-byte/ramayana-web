// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Save Slot Picker HUD (Day 24)
// 3-slot save/load UI + autosave indicator + delete, layered above the
// existing Day 4 SaveLoadHud. Does NOT modify SaveLoadHud or SaveSystem.
// ════════════════════════════════════════════════════════════════════════════
//
// Slots:
//   Slot 1 -> Data.SaveKeys.Slot1
//   Slot 2 -> Data.SaveKeys.Slot2
//   Slot 3 -> Data.SaveKeys.Slot3
//   Auto   -> Data.SaveKeys.AutoSave
//
// Each card shows:
//   - slot label
//   - act id / chapter if present
//   - [Save] [Load] [Delete] buttons

using System;
using UnityEngine;
using UnityEngine.UI;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.Feedback;

namespace Jambudweep.Ramayana.UI
{
    [AddComponentMenu("Ramayana/Save Slot Picker HUD")]
    public sealed class SaveSlotPickerHud : MonoBehaviour
    {
        [Header("Slot keys (order matters)")]
        [SerializeField] private string[] slotKeys = new string[]
        {
            SaveKeys.Slot1,
            SaveKeys.Slot2,
            SaveKeys.Slot3,
            SaveKeys.AutoSave
        };

        [Header("Theme")]
        [SerializeField] private Color bgColor = new Color(0.05f, 0.02f, 0.10f, 0.92f);
        [SerializeField] private Color cardColor = new Color(0.14f, 0.08f, 0.20f, 0.95f);
        [SerializeField] private Color accentColor = new Color(0.95f, 0.78f, 0.42f, 1f);
        [SerializeField] private Color dangerColor = new Color(0.85f, 0.25f, 0.25f, 1f);

        [Header("Layout")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);

        private Canvas _canvas;
        private bool _built;
        private Text _statusText;

        void Start() => Build();

        /// <summary>Refresh all slot cards from disk.</summary>
        public void Refresh()
        {
            if (_canvas == null) return;
            for (int i = 0; i < slotKeys.Length; i++)
            {
                var card = _canvas.transform.Find($"Slot_{i}");
                if (card == null) continue;
                var meta = card.Find("Meta");
                if (meta == null) continue;
                var text = meta.GetComponent<Text>();
                if (text == null) continue;
                text.text = BuildSlotMeta(slotKeys[i]);
            }
        }

        private void Build()
        {
            if (_built) return;
            _built = true;

            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4900;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            gameObject.AddComponent<GraphicRaycaster>();

            // Root background
            var bg = new GameObject("Bg");
            bg.transform.SetParent(transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            Stretch(bgRt);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = bgColor;

            // Title
            var title = CreateText("Title", "Choose a save slot", 40, TextAnchor.UpperCenter,
                new Vector2(0f, -60f), new Vector2(1000f, 60f));
            title.color = accentColor;
            title.fontStyle = FontStyle.Bold;

            // Slot cards container
            var list = new GameObject("SlotList");
            list.transform.SetParent(transform, false);
            var listRt = list.AddComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.pivot = new Vector2(0.5f, 0.5f);
            listRt.offsetMin = new Vector2(60f, 140f);
            listRt.offsetMax = new Vector2(-60f, -160f);
            var layout = list.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            for (int i = 0; i < slotKeys.Length; i++)
            {
                BuildSlotCard(list.transform, i, slotKeys[i]);
            }

            // Status line
            var statusGo = new GameObject("Status");
            statusGo.transform.SetParent(transform, false);
            var srt = statusGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.sizeDelta = new Vector2(960f, 40f);
            srt.anchoredPosition = new Vector2(0f, 70f);
            _statusText = statusGo.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 22;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = accentColor;
            _statusText.text = "";
        }

        private void BuildSlotCard(Transform parent, int index, string slotKey)
        {
            var card = new GameObject($"Slot_{index}");
            card.transform.SetParent(parent, false);

            var le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 170f;

            var bg = card.AddComponent<Image>();
            bg.color = cardColor;

            // Slot number / label
            var label = CreateText("Label", $"{index + 1}. {slotKey}", 28, TextAnchor.UpperLeft,
                new Vector2(20f, -20f), new Vector2(600f, 40f));
            label.fontStyle = FontStyle.Bold;
            label.color = accentColor;

            // Meta line: act / timestamp
            var meta = CreateText("Meta", BuildSlotMeta(slotKey), 22, TextAnchor.UpperLeft,
                new Vector2(20f, -65f), new Vector2(900f, 32f));
            meta.color = new Color(0.86f, 0.78f, 0.70f, 1f);

            // Buttons row
            var row = new GameObject("Row");
            row.transform.SetParent(card.transform, false);
            var rowRt = row.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 0f);
            rowRt.anchorMax = new Vector2(1f, 0f);
            rowRt.pivot = new Vector2(0.5f, 0f);
            rowRt.sizeDelta = new Vector2(0f, 64f);
            rowRt.anchoredPosition = new Vector2(0f, 20f);

            BuildSmallButton(row.transform, "Save", new Vector2(-340f, 0f), () => SaveSlot(slotKey));
            BuildSmallButton(row.transform, "Load", new Vector2(-120f, 0f), () => LoadSlot(slotKey));
            BuildSmallButton(row.transform, "Delete", new Vector2(100f, 0f), () => DeleteSlot(slotKey), dangerColor);
        }

        private string BuildSlotMeta(string slotKey)
        {
            var data = SaveSystem.Load(slotKey);
            if (data == null) return "Empty slot";
            string act = string.IsNullOrEmpty(data.currentActId) ? "unknown act" : data.currentActId;
            string time = data.savedAtUnixMs > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(data.savedAtUnixMs).ToLocalTime().ToString("g")
                : "no timestamp";
            return $"Act: {act} | Saved: {time} | Dharma: {data.dharmaScore}";
        }

        private void SaveSlot(string slotKey)
        {
            var data = SaveSystem.Load(slotKey) ?? new SaveData { saveVersion = SaveKeys.CurrentVersion };
            data.savedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            data.saveVersion = SaveKeys.CurrentVersion;
            bool ok = SaveSystem.Save(slotKey, data);
            SetStatus(ok ? $"✓ Saved '{slotKey}'" : $"✗ Save failed '{slotKey}'");
            Refresh();
        }

        private void LoadSlot(string slotKey)
        {
            var data = SaveSystem.Load(slotKey);
            if (data == null)
            {
                SetStatus($"No save in '{slotKey}'");
                return;
            }
            bool ok = VerseSaveState.RestoreFromSaveData(data);
            SetStatus(ok ? $"✓ Loaded '{slotKey}' (act={data.currentActId})" : $"✗ Load failed '{slotKey}'");
        }

        private void DeleteSlot(string slotKey)
        {
            bool ok = SaveSystem.DeleteSlot(slotKey);
            SetStatus(ok ? $"Deleted '{slotKey}'" : $"✗ Delete failed '{slotKey}'");
            Refresh();
        }

        private void BuildSmallButton(Transform parent, string label, Vector2 pos, Action onClick, Color? color = null)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(160f, 52f);
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = color ?? accentColor;

            var txt = new GameObject("Text").AddComponent<Text>();
            txt.transform.SetParent(go.transform, false);
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 22;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.black;
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

        private Text CreateText(string name, string value, int fontSize, TextAnchor align, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = Color.white;
            t.text = value ?? "";
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            return t;
        }

        private void SetStatus(string text)
        {
            if (_statusText != null) _statusText.text = text ?? "";
            Debug.Log($"[SaveSlotPicker] {text}");
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
