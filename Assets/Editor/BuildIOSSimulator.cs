// Ramayana PS5 → iOS Simulator build script.
// Mirrors BuildMacOS / BuildWindows64 patterns but targets iPhoneSimulator (arm64).
// Usage:
//   Editor menu: Build > Ramayana > iOS Simulator
//   CLI: Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.BuildIOSSimulator.BuildFromCli

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ramayana.Editor
{
    public static class BuildIOSSimulator
    {
        private const string OutputDir = "Build/iOSSimulator";
        private const string ExportName = "RamayanaPS5-iOS-Simulator";

        [MenuItem("Build/Ramayana/iOS Simulator (arm64)")]
        public static void BuildFromMenu()
        {
            var scenes = CollectEnabledScenes();
            // For iOS export we just need the iOS export folder to be created
            // and then xcodebuild will handle the actual .app build.
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);
            PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.SimulatorSDK;
            var report = BuildPipeline.BuildPlayer(
                scenes,
                Path.Combine(OutputDir, ExportName),
                BuildTarget.iOS,
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
                Debug.LogError($"[BuildIOSSimulator] FAILED: {ex}");
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
                $"[BuildIOSSimulator] result={summary.result} totalSize={summary.totalSize} " +
                $"totalTime={summary.totalTime} errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"outputPath={summary.outputPath}");
        }
    }
}
