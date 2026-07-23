// Ramayana PS5 — Title scene builder.
// Replaces the placeholder MainMenu.unity with a real title screen:
//   - Background sprite (Assets/Illustrations/atmosphere/ayodhya_palace.png, 16:9 fit)
//   - Title text "RAMAYANA" in world space
//   - Subtitle "Jambudweep.tech presents"
//   - Start button (placeholder keyboard input)
// Usage:
//   Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.CreateTitleScene.Create

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ramayana.Editor
{
    public static class CreateTitleScene
    {
        // Visual constants for title screen layout (1080p reference).
        private const float TitleSize = 96f;
        private const float SubtitleSize = 28f;

        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera (orthographic, looking at 2D plane)
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            // 2. Background sprite: load ayodhya_palace.png (1024×768 RGB PNG) as Texture2D
            //    then convert to Sprite at runtime — the asset's TextureImporter default
            //    is Texture2D, not Sprite, so direct AssetDatabase.Load<Sprite> fails.
            var bgGo = new GameObject("Background");
            var sr = bgGo.AddComponent<SpriteRenderer>();
            var bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Illustrations/atmosphere/ayodhya_palace.png");
            Sprite bgSprite = null;
            if (bgTex != null)
            {
                bgSprite = Sprite.Create(
                    bgTex,
                    new Rect(0f, 0f, bgTex.width, bgTex.height),
                    new Vector2(0.5f, 0.5f),
                    bgTex.height /* PPU = texture height for 1:1 world scale */);
                sr.sprite = bgSprite;
                // Fit to orthographic camera (size 5 → 10-unit-tall viewport)
                // Match visible width (10 * aspect) to texture width (imageAspect * bgTex.height)
                float viewAspect = 1920f / 1080f;  // 16:9
                float bgWorldHeight = 10f;
                float bgWorldWidth = bgWorldHeight * viewAspect;
                float imageAspect = (float)bgTex.width / bgTex.height;
                float scaleX = bgWorldWidth / bgTex.width;
                float scaleY = bgWorldHeight / bgTex.height;
                bgGo.transform.localScale = new Vector3(scaleX * bgTex.height, scaleY * bgTex.height, 1f);
            }
            else
            {
                // Fallback: dark blue background if art missing
                sr.color = new Color(0.05f, 0.1f, 0.2f, 1f);
                bgGo.transform.localScale = new Vector3(20f, 20f, 1f);
            }

            // 3. Title text — "RAMAYANA" using legacy UnityEngine.UI.Text (TMP not yet configured for this project)
            // We place a Canvas in world space anchored to camera view
            var canvasGo = new GameObject("Title Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvasGo.transform, false);
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "RĀMĀYAṆA";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = (int)TitleSize;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.92f, 0.6f, 1f);  // warm golden
            titleText.fontStyle = FontStyle.Bold;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.6f);
            titleRect.anchorMax = new Vector2(0.5f, 0.6f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(1200f, 200f);
            titleRect.anchoredPosition = new Vector2(0f, 0f);

            var subtitleGo = new GameObject("Subtitle");
            subtitleGo.transform.SetParent(canvasGo.transform, false);
            var subtitleText = subtitleGo.AddComponent<Text>();
            // Round 13 (playable-without-reading): removed "Press [Space]" instruction.
            // The game is now tap-to-start via the ConceptArtTapZone added below.
            subtitleText.text = "jambudweep.tech  ·  Source-faithful retelling";
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = (int)SubtitleSize;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            var subtitleRect = subtitleGo.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.4f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.4f);
            subtitleRect.pivot = new Vector2(0.5f, 0.5f);
            subtitleRect.sizeDelta = new Vector2(1600f, 80f);
            subtitleRect.anchoredPosition = new Vector2(0f, 0f);

            // 4. Directional light (subtle 3D fallback)
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.6f;

            // 4b. Round 13 (playable-without-reading) — ConceptArtTapZone.
            // A invisible full-width button covering the lower 60% of the screen.
            // Tap anywhere on the concept art or the bottom of the title to
            // fire OnTap.  In this minimal version, tapping just logs a
            // confirmation; future story scenes will be loaded here.
            var tapGo = new GameObject("ConceptArtTapZone");
            tapGo.transform.SetParent(canvasGo.transform, false);
            var tapImage = tapGo.AddComponent<Image>();
            tapImage.color = new Color(1f, 1f, 1f, 0f);  // invisible
            tapImage.raycastTarget = true;
            var tapRect = tapGo.GetComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(0f, 0.18f);
            tapRect.anchorMax = new Vector2(1f, 0.62f);
            tapRect.offsetMin = Vector2.zero;
            tapRect.offsetMax = Vector2.zero;
            var tapButton = tapGo.AddComponent<Button>();
            var tapHandler = tapGo.AddComponent<Jambudweep.Ramayana.UI.TitleScreenTapZone>();
            tapButton.onClick.AddListener(tapHandler.OnTap);

            // 5. Save scene
            const string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);

            // 6. Register as first enabled build scene
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(path, true)
            };

            Debug.Log($"[CreateTitleScene] Created title scene with background sprite ({(bgSprite != null ? bgSprite.name : "fallback")}) and saved to {path}");
        }
    }
}