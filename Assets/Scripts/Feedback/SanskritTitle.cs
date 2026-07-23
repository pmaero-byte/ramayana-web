// Round 29 — SanskritTitle: a very brief, atmospheric Sanskrit-style transliteration of
// the day name that fades in/out for ~3 seconds on each verse start.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Feedback
{
    public class SanskritTitle : MonoBehaviour
    {
        public static SanskritTitle Instance { get; private set; }

        private Canvas _canvas;
        private Text _title;
        private Text _sub;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            var go = new GameObject("SanskritTitle");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<SanskritTitle>();
            Instance.Build();
        }

        private void Build()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 4725;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            // Big Sanskrit-styled title in the lower-center
            _title = new GameObject("Title").AddComponent<Text>();
            _title.transform.SetParent(transform, false);
            _title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _title.fontSize = 64;
            _title.alignment = TextAnchor.MiddleCenter;
            _title.color = new Color(0.95f, 0.85f, 0.45f, 0.95f);
            _title.fontStyle = FontStyle.Bold;
            _title.text = "";
            _title.canvasRenderer.SetAlpha(0f);
            var rt = _title.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(900f, 80f);
            rt.anchoredPosition = new Vector2(0f, 250f);

            // Subtitle (Romanization)
            _sub = new GameObject("Sub").AddComponent<Text>();
            _sub.transform.SetParent(transform, false);
            _sub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _sub.fontSize = 22;
            _sub.alignment = TextAnchor.MiddleCenter;
            _sub.color = new Color(0.85f, 0.75f, 0.55f, 0.95f);
            _sub.text = "";
            _sub.canvasRenderer.SetAlpha(0f);
            var srt = _sub.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.sizeDelta = new Vector2(900f, 32f);
            srt.anchoredPosition = new Vector2(0f, 200f);
        }

        // Map of day -> {sa-style title, romanization}
        public static (string sa, string en) TitleForDay(int day)
        {
            switch (day)
            {
                case 0: return ("", "");
                case 1: return ("प्रथम दिवस", "Prathama Divasa");
                case 2: return ("द्वितीय दिवस", "Dvitīya Divasa");
                case 3: return ("तृतीय दिवस", "Tr̥tīya Divasa");
                case 4: return ("चतुर्थ दिवस", "Caturtha Divasa");
                case 5: return ("पञ्चम दिवस", "Pañcama Divasa");
                case 6: return ("षष्ठ दिवस", "Ṣaṣṭha Divasa");
                case 7: return ("सप्तम दिवस", "Saptama Divasa");
                case 8: return ("अष्टम दिवस", "Aṣṭama Divasa");
                case 9: return ("नवम दिवस", "Navama Divasa");
                case 10: return ("दशम दिवस", "Daśama Divasa — Bhīṣma-patana");
                case 11: return ("एकादश दिवस", "Ekādaśa Divasa");
                case 12: return ("द्वादश दिवस", "Dvādaśa Divasa");
                case 13: return ("त्रयोदश दिवस", "Trayodaśa Divasa — Abhimanyu-vadha");
                case 14: return ("चतुर्दश दिवस", "Caturdaśa Divasa");
                case 15: return ("पञ्चदश दिवस", "Pañcadaśa Divasa — Drona-vadha");
                case 16: return ("षोडश दिवस", "Ṣoḍaśa Divasa");
                case 17: return ("सप्तदश दिवस", "Saptadaśa Divasa — Karṇa-vadha");
                case 18: return ("अष्टादश दिवस", "Aṣṭādaśa Divasa — Bhīma-mace");
                default: return ($"दिवस {day}", $"Day {day}");
            }
        }

        public void Show(int day)
        {
            if (_title == null) Build();
            var t = TitleForDay(day);
            _title.text = t.sa;
            _sub.text = t.en;
            StopAllCoroutines();
            StartCoroutine(ShowAnim());
        }

        private IEnumerator ShowAnim()
        {
            float t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float k = t / 0.7f;
                _title.canvasRenderer.SetAlpha(k);
                _sub.canvasRenderer.SetAlpha(k * 0.9f);
                yield return null;
            }
            yield return new WaitForSeconds(2.2f);
            t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float k = 1f - t / 0.7f;
                _title.canvasRenderer.SetAlpha(k);
                _sub.canvasRenderer.SetAlpha(k * 0.9f);
                yield return null;
            }
            _title.canvasRenderer.SetAlpha(0f);
            _sub.canvasRenderer.SetAlpha(0f);
        }
    }
}
