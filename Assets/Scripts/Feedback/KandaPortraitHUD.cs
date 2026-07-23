// Round 30 — KandaPortraitHUD: shows the current kanda (Bala/Ayodhya/Aranya/etc) and
// sarga (chapter) number. Maps to the 7 kandas of Valmiki Ramayana.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class KandaPortraitHUD : MonoBehaviour
    {
        public static KandaPortraitHUD Instance { get; private set; }

        private Canvas _canvas;
        private Text _kandaNum;
        private Text _kandaName;
        private Text _sarga;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("KandaPortraitHUD");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<KandaPortraitHUD>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4720;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            var panel = new GameObject("Panel");
            panel.transform.SetParent(transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(380f, 200f);
            rt.anchoredPosition = new Vector2(30f, -180f);

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.02f, 0.10f, 0.5f);

            _kandaNum = MakeText(panel.transform, "KandaNum", 64, TextAnchor.UpperLeft,
                new Vector2(20f, -10f), new Vector2(120f, 80f));
            _kandaNum.fontStyle = FontStyle.Bold;
            _kandaNum.color = new Color(0.45f, 0.65f, 0.95f, 1f);
            _kandaNum.text = "—";

            _kandaName = MakeText(panel.transform, "KandaName", 26, TextAnchor.UpperLeft,
                new Vector2(140f, -25f), new Vector2(230f, 50f));
            _kandaName.text = "";

            _sarga = MakeText(panel.transform, "Sarga", 22, TextAnchor.UpperLeft,
                new Vector2(140f, -75f), new Vector2(230f, 40f));
            _sarga.color = new Color(0.85f, 0.75f, 0.95f, 1f);
            _sarga.text = "";
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
            t.color = new Color(1f, 0.86f, 0.65f, 1f);
            return t;
        }

        // Map kanda number to display name
        public static string KandaName(int k)
        {
            switch (k)
            {
                case 1: return "Bāla Kāṇḍa";   // Birth / childhood
                case 2: return "Ayodhyā Kāṇḍa"; // Exile begins
                case 3: return "Araṇya Kāṇḍa"; // Forest
                case 4: return "Kiṣkindhā Kāṇḍa"; // Alliance with Sugriva
                case 5: return "Sundara Kāṇḍa"; // Hanuman finds Sita
                case 6: return "Yuddha Kāṇḍa";  // War
                case 7: return "Uttara Kāṇḍa";  // Return
                default: return $"Kāṇḍa {k}";
            }
        }

        public void Show(int kanda, int sarga)
        {
            if (_kandaNum == null) Build();
            _kandaNum.text = kanda > 0 ? kanda.ToString() : "—";
            _kandaName.text = KandaName(kanda);
            _sarga.text = sarga > 0 ? $"Sarga {sarga}" : "";
        }
    }
}
