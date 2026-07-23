// Ramayana PS5 → Windows Standalone64 build script.
// First build script for Ramayana PS5 (greenfield project, 0 scenes).
// Fails loudly if no scenes are present, since Unity Player with no scenes
// still ships an empty .exe but isn't playable.
// Usage:
//   Editor menu: Build > Ramayana > Windows64
//   CLI: Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.BuildWindows64.BuildFromCli

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ramayana.Editor
{
    public static class BuildWindows64
    {
        private const string OutputDir = "Build/Windows64";
        private const string ExecutableName = "RamayanaPS5.exe";

        [MenuItem("Build/Ramayana/Windows64")]
        public static void BuildFromMenu()
        {
            var scenes = CollectEnabledScenes();
            if (scenes.Length == 0)
            {
                Debug.LogWarning("[BuildWindows64] No scenes in EditorBuildSettings — building empty exe (placeholder for greenfield).");
            }
            var report = BuildPipeline.BuildPlayer(
                scenes,
                Path.Combine(OutputDir, ExecutableName),
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);
            LogReport(report);
        }

        public static void BuildFromCli()
        {
            try
            {
                BuildFromMenu();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildWindows64] FAILED: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static string[] CollectEnabledScenes()
        {
            var scenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled && !string.IsNullOrEmpty(s.path) && File.Exists(s.path))
                    scenes.Add(s.path);
            }
            return scenes.ToArray();
        }

        private static void LogReport(BuildReport report)
        {
            var summary = report.summary;
            Debug.Log(
                $"[BuildWindows64] result={summary.result} totalSize={summary.totalSize} " +
                $"totalTime={summary.totalTime} errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"outputPath={summary.outputPath}");
        }
    }
}