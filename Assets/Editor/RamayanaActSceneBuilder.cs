// RamayanaActSceneBuilder — Round 32.
// Parses corpus_data.json to find each act's objectives, then generates one Unity scene per act
// with player rig, motion controls, and one MotionTriggeredDialogue per objective (sarga-cited).

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Jambudweep.Ramayana.Motion3D;
using Jambudweep.Ramayana.UI;

namespace Ramayana.Editor
{
    public static class RamayanaActSceneBuilder
    {
        private const string SceneFolder = "Assets/Scenes";

        [MenuItem("MB_Yuddhakanta/Ramayana/Build Act Scene (current)")]
        public static void BuildActScene() {
            Build("BalaKanda", "Ayodhya Palace", 11, "bala-birth");
        }

        public static void Build(string sceneName, string location, int sceneBuildIndex, string corpusActId)
        {
            _playerRig = null;  // reset between acts so each scene gets a fresh rig
            _ = new GameObject("RamayanaMarker_" + sceneName);  // unique scene marker
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Bootstrap
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.GameBootstrap, Assembly-CSharp"));
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Story.StoryEngine, Assembly-CSharp"));
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Audio.RagaAudioEngine, Assembly-CSharp"));

            // Camera
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 5f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.06f, 0.04f, 1f);

            // Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_" + sceneName;
            ground.transform.localScale = new Vector3(12f, 1f, 20f);
            var groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.50f, 0.36f, 0.18f, 1f) };

            // Backdrop
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backdrop.name = "Backdrop_" + location;
            backdrop.transform.position = new Vector3(0f, 5f, 30f);
            backdrop.transform.localScale = new Vector3(40f, 12f, 1f);
            backdrop.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.65f, 0.42f, 0.18f, 1f) };

            // Player rig
            var playerRig = GetOrCreatePlayerRig();
            playerRig.transform.position = new Vector3(0f, 0.1f, 0f);
            playerRig.AddComponent<ThirdPersonMotionController>();
            playerRig.AddComponent<AnalogJoystick3D>();
            var camController = camGO.AddComponent<CinematicThirdPersonCamera>();
            camController.SetTarget(playerRig.transform);

            // Parse the corpus using a simple string-based approach to avoid JSONUtility issues.
            // The C# string "actId": "<id>" needs to be searched literally.
            // In a C# non-verbatim string, the backslash + quote escape is \".
            // So to find "actId": "bala-birth" in the corpus, the C# literal is:
            //   string needle = "\"actId\": \"" + corpusActId + "\"";
            // In source, that's: "  \"  actId  \"  : <space>  \"  "
            // Which is encoded as: "  \\"  actId  \\"  :   \\"  "
            // Each \\ in source = 1 backslash in C# string; each \" in source = 1 quote.
            string[] oids = new string[0];
            string[] titles = new string[0];
            string[] cues = new string[0];
            TextAsset corpusAsset = Resources.Load<TextAsset>("corpus_data");
            if (corpusAsset != null)
            {
                string corpusText = corpusAsset.text;
                // Use string.IndexOf with a literal: "actId": "<id>"
                // Build the needle in a way that produces a clean C# string
                string quote = "\"";
                string needle = quote + "actId" + quote + ": " + quote + corpusActId + quote;
                int actStart = corpusText.IndexOf(needle, System.StringComparison.Ordinal);
                if (actStart > 0)
                {
                    string endMarker = quote + "actId" + quote + ": ";
                    int actEnd = corpusText.IndexOf(endMarker, actStart + needle.Length, System.StringComparison.Ordinal);
                    if (actEnd < 0) actEnd = corpusText.Length;
                    string actBlock = corpusText.Substring(actStart, actEnd - actStart);
                    oids = ExtractJsonStrings(actBlock, "id");
                    titles = ExtractJsonStrings(actBlock, "title");
                    cues = ExtractJsonStrings(actBlock, "cue");
                    Debug.Log("[RamayanaActSceneBuilder] act=" + corpusActId + " ids=" + oids.Length + " titles=" + titles.Length);
                }
                else
                {
                    Debug.LogWarning("[RamayanaActSceneBuilder] act not found: " + corpusActId);
                }
            }

            int objCount = System.Math.Min(oids.Length, titles.Length);
            Debug.Log("[RamayanaActSceneBuilder] " + sceneName + " hosts " + objCount + " objectives from act " + corpusActId);

            // Place triggers along the +z axis, 4 units apart
            for (int i = 0; i < objCount; i++)
            {
                string oid = oids[i];
                string title = titles[i];
                string cue = i < cues.Length ? cues[i] : "";
                var triggerGO = new GameObject("Trigger_" + oid);
                triggerGO.transform.position = new Vector3(0f, 1f, 6f + i * 4f);
                var trigger = triggerGO.AddComponent<MotionTriggeredDialogue>();
                try
                {
                    trigger.Configure(oid, cue.Length > 0 ? cue : title, "Narrator", 2.5f);
                } catch (System.Exception ex) { Debug.LogWarning("Configure failed: " + ex.Message); }
                var col = triggerGO.AddComponent<SphereCollider>();
                col.radius = 2.5f;
                col.isTrigger = true;
            }

            // Dialogue overlay
            var canvasGO = new GameObject("DialogueCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            canvasGO.AddComponent<GraphicRaycaster>();
            var overlayGO = new GameObject("DialogueOverlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayRT = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = new Vector2(0.5f, 0.25f);
            overlayRT.anchorMax = new Vector2(0.5f, 0.25f);
            overlayRT.pivot = new Vector2(0.5f, 0.5f);
            overlayRT.sizeDelta = new Vector2(360f, 140f);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.05f, 0.02f, 0.01f, 0.65f);
            overlayGO.AddComponent<DialogueOverlay>();

            // HUD bootstrap
            var hudGO = new GameObject("RamayanaHudBootstrap");
            hudGO.AddComponent(System.Type.GetType("Ramayana.Runtime.RamayanaHudBootstrap, Assembly-CSharp"));

            EditorSceneManager.SaveScene(scene, SceneFolder + "/" + sceneName + ".unity");
        }

        private static GameObject _playerRig;
        private static GameObject GetOrCreatePlayerRig()
        {
            if (_playerRig != null) return _playerRig;
            _playerRig = new GameObject("PlayerRig");
            _playerRig.AddComponent<CharacterController>();
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(_playerRig.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
            var visualRenderer = visual.GetComponent<Renderer>();
            visualRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.30f, 0.55f, 0.85f, 1f) };
            return _playerRig;
        }

        // Extract all string values for a given JSON key from a text block.
        // Simple but effective: matches "key": "value" with optional whitespace.
        private static string[] ExtractJsonStrings(string text, string key)
        {
            var result = new List<string>();
            // Build a regex pattern that captures the value.
            // The key may have a colon, then optional whitespace, then "value".
            string quote = "\"";
            // Search for "key": " (with optional whitespace after colon)
            string findStart = quote + key + quote + ": ";
            int idx = 0;
            while (idx < text.Length)
            {
                int m = text.IndexOf(findStart, idx, System.StringComparison.Ordinal);
                if (m < 0) break;
                int start = m + findStart.Length;
                if (start >= text.Length || text[start] != '\"') break;
                start++; // skip opening quote
                int end = start;
                while (end < text.Length)
                {
                    if (text[end] == '\\' && end + 1 < text.Length)
                    {
                        end += 2; // skip escaped char
                        continue;
                    }
                    if (text[end] == '\"') break;
                    end++;
                }
                if (end >= text.Length) break;
                result.Add(text.Substring(start, end - start));
                idx = end + 1;
            }
            return result.ToArray();
        }

        // Build all 8 act scenes at once
        [MenuItem("MB_Yuddhakanta/Ramayana/Build All 8 Act Scenes")]
        public static void BuildAllActScenes()
        {
            Build("BalaKanda",      "Ayodhya Palace",     11, "bala-birth");
            Build("AyodhyaKanda",    "Ayodhya Streets",     12, "ayodhya-dharma");
            Build("AranyaKanda",     "Dandaka Forest",      13, "panchavati-golden-deer");
            Build("KishkindhaKanda", "Pampa Lake",          14, "kishkindha-alliance");
            Build("SundaraKanda",    "Mahendra Peak",       15, "sundarakanda-leap");
            Build("YuddhaKanda",     "Lanka Battlefield",   16, "yuddhakanda-war");
            Build("ReturnKanda",     "Ayodhya Streets",     17, "return-ayodhya");
            Build("UttaraKanda",     "Valmiki Ashram",      18, "uttara-earth-return");

            AssetDatabase.SaveAssets();
            Debug.Log("[RamayanaActSceneBuilder] All 8 act scenes built.");
        }
    }
}
