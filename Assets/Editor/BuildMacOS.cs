// Ramayana PS5 → macOS Standalone (arm64) build script.
// Usage:
//   Editor menu: Build > Ramayana > macOS (arm64)
//   CLI: Unity -batchmode -quit -projectPath . -executeMethod Ramayana.Editor.BuildMacOS.BuildFromCli

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ramayana.Editor
{
    public static class BuildMacOS
    {
        private const string OutputDir = "Build/macOS";
        private const string BundleName = "RamayanaPS5.app";
        private const string BundleId = "com.jambudweepgames.ramayanaps5";
        private const string CompanyName = "Jambudweep Games";
        private const string ProductName = "Ramayana";

        [MenuItem("Build/Ramayana/macOS (arm64)")]
        public static void BuildFromMenu()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1 /* arm64 */);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, BundleId);
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.macOS.buildNumber = "1";

            var scenes = CollectEnabledScenes();
            var report = BuildPipeline.BuildPlayer(
                scenes,
                Path.Combine(OutputDir, BundleName),
                BuildTarget.StandaloneOSX,
                BuildOptions.None);
            LogReport(report);
        }

        public static void BuildFromCli()
        {
            try { BuildFromMenu(); }
            catch (System.Exception ex) { Debug.LogError($"[BuildMacOS] FAILED: {ex}"); EditorApplication.Exit(1); }
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
                $"[BuildMacOS] result={summary.result} totalSize={summary.totalSize} " +
                $"totalTime={summary.totalTime} errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                $"outputPath={summary.outputPath}");
        }
    }
}