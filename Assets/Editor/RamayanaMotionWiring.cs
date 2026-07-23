// Ramayana Motion Wiring — Editor scripts that build the starter scenes and wire motion
// files. Mirrors Yuddhakanta's SceneScaffolder but for the Ramayana universe.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Jambudweep.Ramayana.Motion3D;
using Jambudweep.Ramayana.UI;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.Story;
using Jambudweep.Ramayana.Feedback;

namespace Ramayana.Editor
{
    public static class RamayanaMotionWiring
    {
        private const string SceneFolder = "Assets/Scenes";

        [MenuItem("MB_Yuddhakanta/Ramayana/Build All Motion Scenes")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(SceneFolder);
            BuildBootstrapScene();
            BuildTitleScene();
            // Build one scene per act (7 kāṇḍas + return) so each act has its own playable space
            BuildActScene("BalaKanda",      "Ayodhya Palace",     11, "bala-birth");
            BuildActScene("AyodhyaKanda",    "Ayodhya Streets",     12, "ayodhya-dharma");
            BuildActScene("AranyaKanda",     "Dandaka Forest",      13, "panchavati-golden-deer");
            BuildActScene("KishkindhaKanda", "Pampa Lake",          14, "kishkindha-alliance");
            BuildActScene("SundaraKanda",    "Mahendra Peak",       15, "sundarakanda-leap");
            BuildActScene("YuddhaKanda",     "Lanka Battlefield",   16, "yuddhakanda-war");
            BuildActScene("ReturnKanda",     "Ayodhya Streets",     17, "return-ayodhya");
            BuildActScene("UttaraKanda",     "Valmiki Ashram",      18, "uttara-earth-return");
            AssetDatabase.SaveAssets();
            Debug.Log("[RamayanaMotionWiring] All scenes built.");
        }

        [MenuItem("MB_Yuddhakanta/Ramayana/Build Title Scene")]
        public static void BuildTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Bootstrap
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.GameBootstrap, Assembly-CSharp"));
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Story.StoryEngine, Assembly-CSharp"));
                        bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Audio.RagaAudioEngine, Assembly-CSharp"));

            // Camera
            var cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.10f, 0.06f, 0.04f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            // Background (a sepia-tinted plane)
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "TitleBackground";
            bg.transform.position = new Vector3(0f, 0f, 1f);
            bg.transform.localScale = new Vector3(20f, 30f, 1f);
            var bgRenderer = bg.GetComponent<Renderer>();
            bgRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.55f, 0.30f, 0.10f, 1f) };

            // Canvas with title
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Title text
            var titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.7f);
            titleRT.anchorMax = new Vector2(0.5f, 0.7f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(360f, 80f);
            var titleText = titleGO.AddComponent<Text>();
            titleText.text = "RĀMĀYAṆA";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
            titleText.fontStyle = FontStyle.Bold;

            // Subtitle
            var subGO = new GameObject("Subtitle");
            subGO.transform.SetParent(canvasGO.transform, false);
            var subRT = subGO.AddComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 0.5f);
            subRT.anchorMax = new Vector2(0.5f, 0.5f);
            subRT.pivot = new Vector2(0.5f, 0.5f);
            subRT.sizeDelta = new Vector2(360f, 50f);
            var subText = subGO.AddComponent<Text>();
            subText.text = "jambudweep.tech  ·  Curated Ramayana pilot (98 sarga-cited moments)";
            subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subText.fontSize = 13;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.85f, 0.75f, 0.55f, 1f);

            // Begin button with title pulse
            var btnGO = new GameObject("BeginButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0.30f);
            btnRT.anchorMax = new Vector2(0.5f, 0.30f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.sizeDelta = new Vector2(260f, 60f);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.92f, 0.66f, 0.32f, 0.95f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGO = new GameObject("Label");
            btnLabelGO.transform.SetParent(btnGO.transform, false);
            var labelRT = btnLabelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var labelText = btnLabelGO.AddComponent<Text>();
            labelText.text = "BEGIN RĀMA'S STORY";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 20;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(0.10f, 0.05f, 0.02f, 1f);
            labelText.fontStyle = FontStyle.Bold;

            btn.onClick.AddListener(() => {
                SceneManager.LoadScene("BalaKanda");
            });

            // Mark title screen as visited (runtime HUD setup happens in Play mode)
            VerseSaveState.RecordPlayed("title_visit", 0);
            GameObject hudBootstrap = new GameObject("RamayanaHudBootstrap");
            hudBootstrap.AddComponent(System.Type.GetType("Ramayana.Runtime.RamayanaHudBootstrap, Assembly-CSharp"));

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/TitleScreen.unity");
        }

        [MenuItem("MB_Yuddhakanta/Ramayana/Build Bala Kanda Scene")]
        public static void BuildBalaKandaScene()
        {
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
            cam.transform.position = new Vector3(0f, 4f, -8f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.20f, 0.12f, 0.05f, 1f);
            cam.fieldOfView = 60f;

            // Add CinematicThirdPersonCamera
            var camController = camGO.AddComponent<CinematicThirdPersonCamera>();
            camController.SetTarget(GetOrCreatePlayerRig().transform);

            // Ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ayodhya_Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            var groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.55f, 0.40f, 0.20f, 1f) };

            // Palace backdrop
            var palace = GameObject.CreatePrimitive(PrimitiveType.Cube);
            palace.name = "Palace_Backdrop";
            palace.transform.position = new Vector3(0f, 5f, 20f);
            palace.transform.localScale = new Vector3(30f, 12f, 1f);
            var palaceRenderer = palace.GetComponent<Renderer>();
            palaceRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.70f, 0.45f, 0.20f, 1f) };

            // Player rig
            var playerRig = GetOrCreatePlayerRig();
            playerRig.transform.position = new Vector3(0f, 0.1f, 0f);

            // Add motion components
            var motion = playerRig.GetComponent<ThirdPersonMotionController>() ?? playerRig.AddComponent<ThirdPersonMotionController>();
            motion.enabled = true;

            // Add joystick
            var joystick = playerRig.GetComponent<AnalogJoystick3D>() ?? playerRig.AddComponent<AnalogJoystick3D>();
            joystick.enabled = true;

            // Add motion-triggered dialogue
            var dialogue = playerRig.GetComponent<MotionTriggeredDialogue>() ?? playerRig.AddComponent<MotionTriggeredDialogue>();

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

            // First dialogue trigger zone (player must walk forward to "court")
            var triggerGO = new GameObject("DialogueTrigger_Court");
            triggerGO.transform.position = new Vector3(0f, 1f, 8f);
            var trigger = triggerGO.AddComponent<MotionTriggeredDialogue>();
            try
            {
                trigger.Configure("Bala_birth_intro",
                    "King Dasharatha of Ayodhya has no heir. The gods hear his prayer and bless him with four sons — Rama, Bharata, Lakshmana, and Shatrughna.",
                    "Dasharatha",
                    2f);
            } catch (System.Exception ex)
            {
                Debug.LogWarning("[RamayanaMotionWiring] Configure failed: " + ex.Message);
            }

            // Add collider for trigger
            var col = triggerGO.AddComponent<SphereCollider>();
            col.radius = 2f;
            col.isTrigger = true;

            // Wire motion's onEmbassyReached to advance
            var onReached = new UnityEngine.Events.UnityEvent();
            onReached.AddListener(() => {
                Debug.Log("[BalaKanda] Player reached the court.");
            });
            // (UnityEvent wiring is done via SerializeField in the Editor)

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/BalaKanda.unity");
        }

        [MenuItem("MB_Yuddhakanta/Ramayana/Build Ayodhya Kanda Scene")]
        public static void BuildAyodhyaKandaScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.GameBootstrap, Assembly-CSharp"));
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Story.StoryEngine, Assembly-CSharp"));

            var cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 4f, -8f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.10f, 0.05f, 1f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            var playerRig = GetOrCreatePlayerRig();
            playerRig.transform.position = new Vector3(0f, 0.1f, 0f);
            playerRig.AddComponent<ThirdPersonMotionController>();
            playerRig.AddComponent<AnalogJoystick3D>();

            // Forest background
            for (int i = 0; i < 10; i++)
            {
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tree.transform.position = new Vector3(Random.Range(-12f, 12f), 2f, Random.Range(8f, 25f));
                tree.transform.localScale = new Vector3(0.6f, 4f, 0.6f);
                tree.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.20f, 0.40f, 0.15f, 1f) };
            }

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/AyodhyaKanda.unity");
        }

        [MenuItem("MB_Yuddhakanta/Ramayana/Build Bootstrap Scene")]
        public static void BuildBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.GameBootstrap, Assembly-CSharp"));
            bootstrap.AddComponent(System.Type.GetType("Jambudweep.Ramayana.Story.StoryEngine, Assembly-CSharp"));
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Bootstrap.unity");
        }


        [MenuItem("MB_Yuddhakanta/Ramayana/Build Act Scene (current)")]
        public static void BuildActScene()
        {
            BuildActScene("BalaKanda", "Ayodhya Palace", 11, "bala-birth");
        }

        // Generic per-act scene builder — places one MotionTriggeredDialogue per objective.
        public static void BuildActScene(string sceneName, string location, int sceneBuildIndex, string corpusActId)
        {
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

            // Load the act's objectives from the corpus using simple string searches.
            TextAsset corpusAsset = Resources.Load<TextAsset>("corpus_data");
            string[] oids = new string[0];
            string[] titles = new string[0];
            string[] cues = new string[0];
            if (corpusAsset != null)
            {
                string corpusText = corpusAsset.text;
                // The C# string we build is:  "actId": "<id>"
                // In the file's raw bytes this is stored as: \"actId\": \"
                // We use simple string.IndexOf with a literal that includes the space after the colon.
                string needle = "\\\\\"actId\\\\\": \\\\\"";
                int actStart = corpusText.IndexOf(needle + corpusActId + "\\\\\"", System.StringComparison.Ordinal);
                if (actStart < 0)
                {
                    // Fallback: try without trailing quote
                    actStart = corpusText.IndexOf(needle + corpusActId, System.StringComparison.Ordinal);
                }
                if (actStart > 0)
                {
                    int actEnd = corpusText.IndexOf("\\\\\"actId\\\\\": ", actStart + 30, System.StringComparison.Ordinal);
                    if (actEnd < 0) actEnd = corpusText.Length;
                    string actBlock = corpusText.Substring(actStart, actEnd - actStart);
                    oids = ExtractJsonStrings(actBlock, "id");
                    titles = ExtractJsonStrings(actBlock, "title");
                    cues = ExtractJsonStrings(actBlock, "cue");
                    Debug.Log("[RamayanaMotionWiring] act=" + corpusActId + " ids=" + oids.Length + " titles=" + titles.Length);
                }
            }

            int objCount = System.Math.Min(oids.Length, titles.Length);
            Debug.Log("[RamayanaMotionWiring] " + sceneName + " hosts " + objCount + " objectives from act " + corpusActId);

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

        private static string[] ExtractJsonStrings(string text, string key)
        {
            var result = new System.Collections.Generic.List<string>();
            string marker = "\"" + key + "\":\"";
            int idx = 0;
            while (idx < text.Length)
            {
                int m = text.IndexOf(marker, idx, System.StringComparison.Ordinal);
                if (m < 0) break;
                int start = m + marker.Length;
                int end = text.IndexOf('\"', start);
                if (end < 0) break;
                result.Add(text.Substring(start, end - start));
                idx = end + 1;
            }
            return result.ToArray();
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
    }
}


