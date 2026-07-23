// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — EditMode Tests (Day 25)
// Validates canonical data structures, save-system keys, and KandaTree
// registry without requiring a running Unity instance or scene load.
// ════════════════════════════════════════════════════════════════════════════
//
// Run in batchmode:
//   Unity -batchmode -nographics -runTests -testPlatform EditMode \
//          -testResults results.xml -projectPath .
//
// Or via Unity Test Framework package in the Editor.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.Data;
using Jambudweep.Ramayana.Gameplay;

namespace Ramayana.Tests
{
    /// <summary>
    /// EditMode tests for the canonical data layer, save system keys,
    /// KandaTree registry, and VerseSaveState bridge.
    /// No scene load required; runs entirely in EditMode.
    /// </summary>
    public class Day25EditModeTests
    {
        private const string CORPUS_BALA = "Ramayana/moments_bala_kanda";
        private const string CORPUS_AYODHYA = "Ramayana/moments_ayodhya_kanda";
        private const string RESOURCES_DIR = "Assets/Resources/Ramayana";
        private const string SCENES_DIR = "Assets/Scenes";

        // ── SaveKeys ────────────────────────────────────────────────────

        [Test]
        public void SaveKeys_AllSlotConstantsPresent()
        {
            Assert.IsNotNull(SaveKeys.Slot1, "SaveKeys.Slot1 should exist");
            Assert.IsNotNull(SaveKeys.Slot2, "SaveKeys.Slot2 should exist");
            Assert.IsNotNull(SaveKeys.Slot3, "SaveKeys.Slot3 should exist");
            Assert.IsNotNull(SaveKeys.AutoSave, "SaveKeys.AutoSave should exist");
            CollectionAssert.AreAllUnique(new[] { SaveKeys.Slot1, SaveKeys.Slot2, SaveKeys.Slot3, SaveKeys.AutoSave },
                "Save slot keys must be unique");
        }

        [Test]
        public void SaveData_CanBeSerializedAndDeserialized()
        {
            var original = new SaveData
            {
                saveVersion = SaveKeys.CurrentVersion,
                episodeId = "test-episode",
                currentActId = "bala-kanda",
                currentMomentIndex = 5,
                selectedCharacterId = "rama",
                dharmaScore = 42,
                completedObjectiveIds = new[] { "obj-1", "obj-2" },
                unlockedCharacterIds = new[] { "hanuman" },
                collectedShlokaIds = Array.Empty<string>(),
                collectedCollectibleIds = Array.Empty<string>(),
                visitedScenes = new[] { "BalaKanda" },
                totalPlayTimeSec = 123.4f,
                savedAtUnixMs = 1700000000000
            };

            string json = JsonUtility.ToJson(original, prettyPrint: false);
            Assert.IsFalse(string.IsNullOrEmpty(json), "Serialized JSON should not be empty");

            var roundtrip = JsonUtility.FromJson<SaveData>(json);
            Assert.IsNotNull(roundtrip);
            Assert.AreEqual(original.saveVersion, roundtrip.saveVersion);
            Assert.AreEqual(original.episodeId, roundtrip.episodeId);
            Assert.AreEqual(original.currentActId, roundtrip.currentActId);
            Assert.AreEqual(original.dharmaScore, roundtrip.dharmaScore);
            Assert.Greater(roundtrip.totalPlayTimeSec, 0f);
        }

        // ── VerseSaveState ──────────────────────────────────────────────

        [Test]
        public void VerseSaveState_RestoreFromSaveData_UpdatesPlayerPrefs()
        {
            PlayerPrefs.DeleteKey("verse.history.v1");
            PlayerPrefs.DeleteKey("verse.best_streak.v1");
            PlayerPrefs.DeleteKey("verse.total.v1");
            PlayerPrefs.Save();

            var data = new SaveData
            {
                episodeId = "rama-arc",
                dharmaScore = 77,
                totalPlayTimeSec = 300f
            };

            bool ok = VerseSaveState.RestoreFromSaveData(data);
            Assert.IsTrue(ok, "RestoreFromSaveData should return true for valid data");

            Assert.AreEqual("rama-arc", PlayerPrefs.GetString("verse.history.v1", ""));
            Assert.AreEqual(300, PlayerPrefs.GetInt("verse.total.v1", 0));
            Assert.AreEqual(77, PlayerPrefs.GetInt("verse.best_streak.v1", 0));

            // Cleanup
            PlayerPrefs.DeleteKey("verse.history.v1");
            PlayerPrefs.DeleteKey("verse.best_streak.v1");
            PlayerPrefs.DeleteKey("verse.total.v1");
            PlayerPrefs.Save();
        }

        [Test]
        public void VerseSaveState_RestoreFromNull_ReturnsFalse()
        {
            bool ok = VerseSaveState.RestoreFromSaveData(null);
            Assert.IsFalse(ok, "RestoreFromSaveData should reject null");
        }

        // ── KandaTree ───────────────────────────────────────────────────

        [Test]
        public void KandaTree_HasEightEntries()
        {
            Assert.AreEqual(8, KandaTree.KandaCount, "KandaTree should expose exactly 8 kandas");
        }

        [Test]
        public void KandaTree_EntriesAreInCanonicalOrder()
        {
            var entries = KandaTree.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                Assert.AreEqual(i + 1, entries[i].Order, $"Entry {i} should have order {i + 1}");
            }
        }

        [Test]
        public void KandaTree_GetEntry_ReturnsNullForEmptyId()
        {
            Assert.IsNull(KandaTree.GetEntry(""));
            Assert.IsNull(KandaTree.GetEntry(null));
        }

        [Test]
        public void KandaTree_GetSceneName_MatchesExistingSceneFile()
        {
            foreach (var entry in KandaTree.Entries)
            {
                string scenePath = Path.Combine(SCENES_DIR, entry.SceneName + ".unity");
                Assert.IsTrue(File.Exists(scenePath),
                    $"Scene file for kanda '{entry.KandaId}' missing: {scenePath}");
            }
        }

        [Test]
        public void KandaTree_GetNextKanda_ReturnsSequentialOrder()
        {
            string first = KandaTree.Entries[0].KandaId;
            string last = KandaTree.Entries[KandaTree.KandaCount - 1].KandaId;
            Assert.IsNotNull(KandaTree.GetNextKanda(first), "First kanda should have a next kanda");
            Assert.IsNull(KandaTree.GetNextKanda(last), "Last kanda should have no next kanda");
        }

        [Test]
        public void KandaTree_EachKanda_HasMomentsJson()
        {
            foreach (var entry in KandaTree.Entries)
            {
                string path = Path.Combine(RESOURCES_DIR, entry.CorpusFileName + ".json");
                Assert.IsTrue(File.Exists(path),
                    $"Moments JSON missing for kanda '{entry.KandaId}': {path}");
            }
        }

        [Test]
        public void KandaTree_MomentsJson_HasValidKandaField()
        {
            foreach (var entry in KandaTree.Entries)
            {
                string path = Path.Combine(RESOURCES_DIR, entry.CorpusFileName + ".json");
                Assert.IsTrue(File.Exists(path), $"Missing: {path}");

                string text = File.ReadAllText(path);
                Assert.IsFalse(string.IsNullOrEmpty(text), $"JSON empty: {path}");

                bool hasKandaField = text.Contains("\"kanda\":") &&
                                      text.Contains($"\"{entry.KandaId}\"");
                Assert.IsTrue(hasKandaField,
                    $"kanda field mismatch in {entry.CorpusFileName}.json (expected {entry.KandaId})");
            }
        }

        // ── Moments corpus data validation ──────────────────────────────

        [Test]
        public void MomentsBalaKanda_LoadsFromResources()
        {
            TextAsset ta = Resources.Load<TextAsset>(CORPUS_BALA);
            Assert.IsNotNull(ta, "moments_bala_kanda.json should load from Resources/");
            Assert.IsFalse(string.IsNullOrEmpty(ta.text));
        }

        [Test]
        public void MomentsAyodhyaKanda_LoadsFromResources()
        {
            TextAsset ta = Resources.Load<TextAsset>(CORPUS_AYODHYA);
            Assert.IsNotNull(ta, "moments_ayodhya_kanda.json should load from Resources/");
            Assert.IsFalse(string.IsNullOrEmpty(ta.text));
        }

        // ── SaveSlotPicker default keys ────────────────────────────────

        [Test]
        public void SaveSlotPicker_DefaultSlotKeys_AreValidSaveKeys()
        {
            // Validate that the SaveSlotPickerHud default array matches
            // canonical SaveKeys constants. We inspect the source file directly
            // because SaveSlotPickerHud is a MonoBehaviour whose serialized
            // fields only exist at runtime.
            string sourcePath = "Assets/Scripts/UI/SaveSlotPickerHud.cs";
            Assert.IsTrue(File.Exists(sourcePath), $"Source missing: {sourcePath}");

            string src = File.ReadAllText(sourcePath);
            Assert.IsTrue(src.Contains("SaveKeys.Slot1"), "SaveSlotPicker should reference SaveKeys.Slot1");
            Assert.IsTrue(src.Contains("SaveKeys.Slot2"), "SaveSlotPicker should reference SaveKeys.Slot2");
            Assert.IsTrue(src.Contains("SaveKeys.Slot3"), "SaveSlotPicker should reference SaveKeys.Slot3");
            Assert.IsTrue(src.Contains("SaveKeys.AutoSave"), "SaveSlotPicker should reference SaveKeys.AutoSave");
        }
    }
}
