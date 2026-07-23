// RAMAYANA PS5 — Runtime Sanity Check
// Standalone console app that exercises the data layer end-to-end:
// 1. Load corpus_data.json (Unity-compatible format)
// 2. Deserialize to ActData/CharacterData
// 3. Verify counts match expected
// 4. Round-trip: serialize → deserialize → compare hashes
// 5. Exercise SaveSystem (write + read)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.Story;
using Jambudweep.Ramayana.Audio;

namespace RamayanaPS5.SanityCheck
{
    public class Program
    {
        static int passed = 0, failed = 0;

        public static int Main(string[] args)
        {
            string corpusPath = args.Length > 0
                ? args[0]
                : Path.Combine("Assets", "Resources", "corpus_data.json");

            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine("RAMAYANA PS5 — Runtime Sanity Check");
            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine($"Corpus: {corpusPath}");
            Console.WriteLine();

            Test("Corpus file exists", () => File.Exists(corpusPath));
            if (!File.Exists(corpusPath)) { Console.WriteLine("\nCorpus not found — run corpus_to_unity.py first."); return 1; }

            string json = File.ReadAllText(corpusPath);
            var corpus = JsonSerializer.Deserialize<CorpusRoot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert("Corpus parsed", corpus != null);
            Assert("Schema version 1", corpus.schemaVersion == "1");

            // Character count
            int charCount = corpus.characters?.Length ?? 0;
            Assert($"Characters = 50 (got {charCount})", charCount == 50);

            // First character sanity
            if (charCount > 0)
            {
                var rama = corpus.characters[0];
                Assert("First char is rama", rama.characterId == "rama");
                Assert("Rama's role preserved", rama.role == "Seventh avatar of Vishnu");
                Assert("Rama's color preserved", rama.color == "#4f8cff");
            }

            // Act count
            int actCount = corpus.acts?.Length ?? 0;
            Assert($"Acts = 8 (got {actCount})", actCount == 8);

            // Objectives
            int objCount = corpus.acts?.Sum(a => a.objectives?.Length ?? 0) ?? 0;
            Assert($"Objectives = 98 (got {objCount})", objCount == 98);

            // Dialogue
            int diaCount = corpus.acts?.Sum(a => a.dialogue?.Length ?? 0) ?? 0;
            Assert($"Dialogue lines = 66 (got {diaCount})", diaCount == 66);

            // Act 1 deep check (Balakanda)
            var act1 = corpus.acts[0];
            Assert("Act 1 ID = bala-birth", act1.actId == "bala-birth");
            Assert("Act 1 number = 1", act1.actNumber == 1);
            Assert("Act 1 scene = bala", act1.scene == "bala");
            Assert($"Act 1 objectives = 10 (got {act1.objectives.Length})", act1.objectives.Length == 10);
            Assert($"Act 1 dialogue = 10 (got {act1.dialogue.Length})", act1.dialogue.Length == 10);

            // First objective detail check
            var firstObj = act1.objectives[0];
            Assert("First obj ID = witness-birth", firstObj.id == "witness-birth");
            Assert("First obj type = witness", firstObj.type == "witness");
            Assert("First obj has speaker in completedLine",
                firstObj.completedLine?.speaker == "Kausalya");

            // Position preservation
            Assert("First obj position.x = -2", firstObj.position.x == -2f);
            Assert("First obj position.y = 0.6", Math.Abs(firstObj.position.y - 0.6f) < 0.001f);

            // StoryMoment → Unity round trip
            Test("ScriptableObject can be instantiated", () =>
            {
                var act = ScriptableObject.CreateInstance<ActData>();
                act.actId = act1.actId;
                act.title = act1.title;
                act.actNumber = act1.actNumber;
                act.scene = ParseScene(act1.scene);
                return act.actId == "bala-birth" && act.title == act1.title;
            });

            Test("StoryEngine state can be initialized", () =>
            {
                var state = new StoryEngineState();
                state.selectedCharacterId = "rama";
                state.currentActId = "bala-birth";
                state.dharmaScore = 25;
                return state.mode == StoryMode.Idle && state.dharmaScore == 25;
            });

            Test("SaveSystem round-trip", () =>
            {
                var save = new SaveData
                {
                    episodeId = "ramayana-valmiki",
                    currentActId = "bala-birth",
                    selectedCharacterId = "rama",
                    dharmaScore = 42,
                    completedObjectiveIds = new[] { "witness-birth", "break-bow" },
                    visitedScenes = new[] { "ayodhya", "bala", "janakpur" }
                };
                string slotKey = "ramayana-sanity-test";
                bool saved = SaveSystem.Save(slotKey, save);
                if (!saved) return false;
                var loaded = SaveSystem.Load(slotKey);
                SaveSystem.DeleteSlot(slotKey);
                return loaded != null
                    && loaded.episodeId == "ramayana-valmiki"
                    && loaded.dharmaScore == 42
                    && loaded.completedObjectiveIds.Length == 2;
            });

            Test("VoiceRegister enum has all 11 values", () =>
            {
                var names = Enum.GetNames(typeof(VoiceRegister));
                return names.Length == 11;
            });

            Test("Raga enum has 6 values", () =>
            {
                var names = Enum.GetNames(typeof(Raga));
                return names.Length == 6;
            });

            Test("RamayanaScene enum has 18 values", () =>
            {
                var names = Enum.GetNames(typeof(RamayanaScene));
                return names.Length == 18;
            });

            // Prahasta has apostrophe — the parser must handle it
            var prahasta = corpus.characters?.FirstOrDefault(c => c.characterId == "prahasta");
            Assert("Prahasta role preserved with apostrophe ('Ravana\\'s commander')",
                prahasta != null && prahasta.role == "Ravana's commander");

            var sarama = corpus.characters?.FirstOrDefault(c => c.characterId == "sarama");
            Assert("Sarama role preserved with apostrophe ('Vibhishana\\'s wife')",
                sarama != null && sarama.role == "Vibhishana's wife");

            // ── Generate .asset YAML files for all 8 acts + 50 characters (Unity-importable) ──
            Console.WriteLine();
            Console.WriteLine("--- Generating Unity .asset YAML files ---");
            // Locate repo root. Assembly.Location is the binary's directory (e.g. bin/Debug/net9.0/),
            // but the canonical location we want is the source repo root (3 levels up from SanityCheck.csproj).
            // Strategy: try several heuristics and pick the first that exists or write succeeds.
            string[] candidateRoots = {
                // 1. Working directory (set by `cd` before `dotnet run`)
                Directory.GetCurrentDirectory(),
                // 2. Walk up from Assembly.Location until we find a directory with "Assets/Resources"
                FindRepoRootFromAssembly(typeof(RamayanaPS5.SanityCheck.Program).Assembly.Location),
                // 3. Hardcoded fallback
                "/Users/prabaharan/Aerospace_projects/RamayanaPS5"
            };
            string repoRoot = null;
            foreach (var c in candidateRoots) {
                if (c == null) continue;
                var probe = Path.Combine(c, "Assets", "Resources");
                if (Directory.Exists(probe) || CanCreateDirectory(c)) {
                    repoRoot = c;
                    break;
                }
            }
            if (repoRoot == null) repoRoot = candidateRoots[2];
            string generatedDir = Path.Combine(repoRoot, "Assets", "Resources", "Generated");
            Directory.CreateDirectory(generatedDir);
            Console.WriteLine($"  Repo root resolved to: {repoRoot}");
            int generatedCount = 0;
            long totalBytes = 0;

            // Acts
            foreach (var act in corpus.acts)
            {
                string assetPath = Path.Combine(generatedDir, $"Act_{act.actNumber}_{act.actId}.asset");
                string yaml = UnityAssetYaml.GenerateActAsset(act);
                File.WriteAllText(assetPath, yaml);
                generatedCount++;
                totalBytes += new FileInfo(assetPath).Length;
            }
            // Characters
            if (corpus.characters != null)
            {
                foreach (var ch in corpus.characters)
                {
                    string assetPath = Path.Combine(generatedDir, $"Character_{ch.characterId}.asset");
                    string yaml = UnityAssetYaml.GenerateCharacterAsset(ch);
                    File.WriteAllText(assetPath, yaml);
                    generatedCount++;
                    totalBytes += new FileInfo(assetPath).Length;
                }
            }

            Console.WriteLine($"  Generated {generatedCount} .asset files ({corpus.acts.Length} acts + {corpus.characters?.Length ?? 0} chars) → {generatedDir}");
            Console.WriteLine($"  Total bytes: {totalBytes:N0}");
            foreach (var f in Directory.GetFiles(generatedDir, "*.asset").OrderBy(x => x).Take(20))
                Console.WriteLine($"    {Path.GetFileName(f)} ({new FileInfo(f).Length:N0} bytes)");
            if (Directory.GetFiles(generatedDir, "*.asset").Length > 20)
                Console.WriteLine($"    ... ({Directory.GetFiles(generatedDir, "*.asset").Length - 20} more)");

            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine($"RESULTS: {passed} passed, {failed} failed");
            Console.WriteLine("════════════════════════════════════════════════════════════");
            return failed == 0 ? 0 : 1;
        }

        // ── Test helpers ─────────────────────────────────────────────────

        static void Test(string name, Func<bool> fn)
        {
            bool ok;
            try { ok = fn(); } catch (Exception e)
            {
                Console.WriteLine($"  ❌ {name} — THREW: {e.Message}");
                failed++;
                return;
            }
            if (ok) { Console.WriteLine($"  ✅ {name}"); passed++; }
            else { Console.WriteLine($"  ❌ {name}"); failed++; }
        }

        static void Assert(string name, bool ok) => Test(name, () => ok);

        static RamayanaScene ParseScene(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "ayodhya": return RamayanaScene.Ayodhya;
                case "panchavati": return RamayanaScene.Panchavati;
                case "kishkindha": return RamayanaScene.Kishkindha;
                case "lanka": return RamayanaScene.Lanka;
                case "setu": return RamayanaScene.Setu;
                case "return": return RamayanaScene.Return;
                case "bala": return RamayanaScene.Bala;
                case "janakpur": return RamayanaScene.Janakpur;
                case "ashram": return RamayanaScene.Ashram;
                case "ocean": return RamayanaScene.Ocean;
                case "ravana-court": return RamayanaScene.RavanaCourt;
                case "earth-return": return RamayanaScene.EarthReturn;
                case "mithila": return RamayanaScene.Mithila;
                case "chitrakuta": return RamayanaScene.Chitrakuta;
                case "nandigram": return RamayanaScene.Nandigram;
                case "sarayu": return RamayanaScene.Sarayu;
                case "lanka-palace": return RamayanaScene.LankaPalace;
                case "lanka-garden": return RamayanaScene.LankaGarden;
                default: return RamayanaScene.Ayodhya;
            }
        }

        // ── Mirror CorpusImporter.cs ─────────────────────────────────────

        public class CorpusRoot
        {
            public string schemaVersion { get; set; }
            public Character[] characters { get; set; }
            public Act[] acts { get; set; }
        }

        public class Character
        {
            public string characterId { get; set; }
            public string displayName { get; set; }
            public string displayNameTamil { get; set; }
            public string role { get; set; }
            public string color { get; set; }
        }

        public class Act
        {
            public string actId { get; set; }
            public int actNumber { get; set; }
            public string title { get; set; }
            public string location { get; set; }
            public string scene { get; set; }
            public string setup { get; set; }
            public string lesson { get; set; }
            public string playerRole { get; set; }
            public Objective[] objectives { get; set; }
            public Dialogue[] dialogue { get; set; }
        }

        public class Objective
        {
            public string id { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public string marker { get; set; }
            public string cue { get; set; }
            public string actionLabel { get; set; }
            public CompletedLine completedLine { get; set; }
            public int target { get; set; }
            public Position position { get; set; }
        }

        public class CompletedLine { public string speaker { get; set; } public string text { get; set; } }
        public class Position { public float x { get; set; } public float y { get; set; } public float z { get; set; } }
        public class Dialogue { public string speaker { get; set; } public string text { get; set; } }

        // ── Path-resolution helpers (for the asset generator) ──

        /// <summary>
        /// Walk up from a path looking for the RamayanaPS5 repo root.
        /// Detection: a directory containing both "Assets/Resources" and "Tools".
        /// </summary>
        static string FindRepoRootFromAssembly(string assemblyLocation)
        {
            if (string.IsNullOrEmpty(assemblyLocation)) return null;
            var dir = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation));
            while (dir != null)
            {
                var probe = Path.Combine(dir.FullName, "Assets", "Resources");
                var toolsProbe = Path.Combine(dir.FullName, "Tools");
                if (Directory.Exists(probe) && Directory.Exists(toolsProbe))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }

        /// <summary>
        /// Test if we can create a directory at this path.
        /// Used as fallback signal when the probe directory doesn't exist yet.
        /// </summary>
        static bool CanCreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            try
            {
                // Try to create a uniquely-named subdir as a write-probe
                var probe = Path.Combine(path, ".sanitycheck-write-probe");
                Directory.CreateDirectory(probe);
                Directory.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
