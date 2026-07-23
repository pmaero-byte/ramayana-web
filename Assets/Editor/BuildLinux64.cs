// Ramayana PS5 → Linux Standalone64 build script.
// Mirrors BuildWindows64.cs / BuildMacOS.cs for the 3rd platform.
// Usage:
//   Editor menu: Build > Ramayana > Linux64
//   CLI: Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.BuildLinux64.BuildFromCli

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ramayana.Editor
{
    public static class BuildLinux64
    {
        private const string OutputDir = "Build/Linux64";
        private const string ExecutableName = "RamayanaPS5.x86_64";

        [MenuItem("Build/Ramayana/Linux64")]
        public static void BuildFromMenu()
        {
            var scenes = CollectEnabledScenes();
            var report = BuildPipeline.BuildPlayer(
                scenes,
                Path.Combine(OutputDir, ExecutableName),
                BuildTarget.StandaloneLinux64,
                BuildOptions.None);
            LogReport(report);
        }

        public static void BuildFromCli()
        {
            try { BuildFromMenu(); }
            catch (System.Exception ex) { Debug.LogError($"[BuildLinux64] FAILED: {ex}"); EditorApplication.Exit(1); }
        }

        private static string[] CollectEnabledScenes()
        {
            var scenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled && !string.IsNullOrEmpty(s.path) && File.Exists(s.path))
                    scenes.Add(s.path);
            return scenes.ToArray();
        }

        private static void LogReport(BuildReport report)
        {
            var summary = report.summary;
            Debug.Log(
                $"[BuildLinux64] result={summary.result} totalSize={summary.totalSize} " +
                $"totalTime={summary.totalTime} errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"outputPath={summary.outputPath}");
        }
    }
}