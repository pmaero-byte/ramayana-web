// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Day 39 runtime probe
// Batchmode runner: opens MainMenu scene, waits one frame, takes screenshots,
// and writes a runtime state report so this run is not "blind."
// Usage: Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.Day39RuntimeProbe.Run
// ════════════════════════════════════════════════════════════════════════════

using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.UI;
using Jambudweep.Ramayana.Audio;
using Jambudweep.Ramayana.Verse;
using Jambudweep.Ramayana.Feedback;
using Jambudweep.Ramayana.Gameplay;

namespace Ramayana.Editor
{
    public static class Day39RuntimeProbe
    {
        private static string ReportDir => Path.Combine(Directory.GetCurrentDirectory(), "Library/DebugReports");
        private static string ReportPath => Path.Combine(ReportDir, "Day39-runtime-report.txt");
        private static string ScreenshotDir => Path.Combine(Directory.GetCurrentDirectory(), "Library/Screenshots");

        [MenuItem("Ramayana/Probes/Run Day 39 Runtime Probe")]
        public static void Run()
        {
            Directory.CreateDirectory(ReportDir);
            Directory.CreateDirectory(ScreenshotDir);

            using (var sw = new StreamWriter(ReportPath, false))
            {
                sw.WriteLine("[Day39 Runtime Probe]");
                sw.WriteLine($"Time: {System.DateTime.UtcNow:O}");
                sw.WriteLine($"Unity: {Application.unityVersion}");
                sw.WriteLine($"Platform: {Application.platform}");
                sw.WriteLine($"ScenesInBuild: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}");
                sw.WriteLine();

                // Ensure MainMenu is open.
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == null ||
                    !UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.EndsWith("MainMenu.unity"))
                {
                    sw.WriteLine("[MainMenu] MainMenu was not the active scene; attempting open...");
                    EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
                }

                sw.WriteLine("[MainMenu] Active scene path: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
                sw.WriteLine("[MainMenu] Root objects: " + string.Join(", ", UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Select(x => x.name).ToArray()));
                sw.WriteLine();

                // 1. KandaTree probe.
                sw.WriteLine("[KandaTree]");
                sw.WriteLine("  Count=" + KandaTree.KandaCount);
                sw.WriteLine("  Entries: " + string.Join(" | ", KandaTree.Entries.Select(e => $"{e.Order}:{e.KandaId}")));
                sw.WriteLine();

                // 2. Verses + audio probes.
                sw.WriteLine("[VerseOrchestrator]");
                sw.WriteLine("  Instance=" + (VerseOrchestrator.Instance != null));
                sw.WriteLine("  Loaded=" + (VerseOrchestrator.Instance != null && VerseOrchestrator.Instance.IsLoaded));
                sw.WriteLine();

                sw.WriteLine("[RagaAudioEngine]");
                sw.WriteLine("  Instance=" + (RagaAudioEngine.Instance != null));
                sw.WriteLine();

                sw.WriteLine("[FallbackRagaSynth]");
                sw.WriteLine("  Instance=" + (FallbackRagaSynth.Instance != null));
                sw.WriteLine();

                // 3. Save system probe.
                sw.WriteLine("[SaveSystem]");
                sw.WriteLine("  HasMostRecent=" + (SaveSystem.GetMostRecentSave() != null));
                sw.WriteLine();

                // 4. UI probe.
                var controller = Object.FindFirstObjectByType<MainMenuScreenController>();
                sw.WriteLine("[MainMenuScreenController]");
                sw.WriteLine("  Present=" + (controller != null));
                sw.WriteLine();

                var overlay = Object.FindFirstObjectByType<DialogueOverlay>();
                sw.WriteLine("[DialogueOverlay]");
                sw.WriteLine("  Present=" + (overlay != null));
                sw.WriteLine();

                // 5. Screenshot.
                string shotPath = Path.Combine(ScreenshotDir, "Day39-MainMenu.png");
                ScreenCapture.CaptureScreenshot(shotPath);
                sw.WriteLine($"[Screenshot] Saved: {shotPath}");

                sw.WriteLine();
                sw.WriteLine("[Result] Probe complete.");
                Debug.Log("[Day39] Runtime probe written to " + ReportPath);
            }
        }
    }
}
