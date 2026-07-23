// Creates a placeholder scene if none exist. First-run scaffolder for greenfield projects.
// Usage:
//   Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.CreatePlaceholderScene.Create
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Ramayana.Editor
{
    public static class CreatePlaceholderScene
    {
        public static void Create()
        {
            // Create a new empty scene with one camera + one light
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add a Camera
            var cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>();
            cam.tag = "MainCamera";

            // Add a directional light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;

            // Save as Assets/Scenes/MainMenu.unity
            const string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);

            // Add to EditorBuildSettings as the first enabled scene
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(path, true)
            };

            Debug.Log($"[CreatePlaceholderScene] Created and registered {path}");
        }
    }
}