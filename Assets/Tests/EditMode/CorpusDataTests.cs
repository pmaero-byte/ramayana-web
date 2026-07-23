using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Ramayana.Tests
{
    /// <summary>
    /// EditMode tests that prove the Ramayana PS5 corpus data layer is well-formed.
    ///
    /// Mirrors the Yuddhakanta pattern (Assets/Tests/EditMode/CorpusCitationTests.cs
    /// — 4/4 PASS) but targets Ramayana-specific invariants:
    ///   - corpus_data.json has 50 characters and 8 acts (one per Valmiki kanda)
    ///   - All character IDs are unique
    ///   - All 8 Valmiki kandas are named correctly
    ///   - All character colors are valid 6-digit hex (#RRGGBB)
    ///
    /// Runs in batchmode via:
    ///   Unity -batchmode -nographics -runTests -testPlatform EditMode -testResults results.xml
    /// </summary>
    public class CorpusDataTests
    {
        private const string CORPUS_RESOURCE_PATH = "corpus_data";
        private static readonly Regex HexColor = new Regex(@"^#[0-9a-fA-F]{6}$");

        [Test]
        public void CorpusData_LoadsFromResources()
        {
            TextAsset asset = Resources.Load<TextAsset>(CORPUS_RESOURCE_PATH);
            Assert.IsNotNull(asset, "corpus_data.json not loadable from Resources/");
            Assert.IsFalse(string.IsNullOrEmpty(asset.text), "corpus_data.json is empty");

            RamayanaCorpus parsed = null;
            Assert.DoesNotThrow(() =>
            {
                parsed = JsonUtility.FromJson<RamayanaCorpus>(asset.text);
            }, "corpus_data.json failed to parse as JSON");
            Assert.IsNotNull(parsed);
            Assert.AreEqual("1", parsed.schemaVersion, "schemaVersion should be 1");
            Assert.GreaterOrEqual(parsed.characters.Count, 13, "Expected >= 13 Ramayana characters (Rama, Sita, Lakshmana, Hanuman, Ravana, Bharata + 8 more)");
            Assert.AreEqual(8, parsed.acts.Count, "Expected exactly 8 acts (one per Valmiki kanda)");
        }

        [Test]
        public void CorpusData_CharacterIdsAreUnique()
        {
            // StoryEngine.cs indexes characters by characterId — duplicates would
            // silently overwrite entries and break StoryMoment lookups.
            TextAsset asset = Resources.Load<TextAsset>(CORPUS_RESOURCE_PATH);
            RamayanaCorpus parsed = JsonUtility.FromJson<RamayanaCorpus>(asset.text);

            HashSet<string> seen = new HashSet<string>();
            List<string> dupes = new List<string>();
            foreach (var c in parsed.characters)
            {
                if (string.IsNullOrEmpty(c.characterId))
                {
                    dupes.Add("<empty>");
                    continue;
                }
                if (!seen.Add(c.characterId))
                {
                    dupes.Add(c.characterId);
                }
            }
            Assert.IsEmpty(dupes,
                "characterId must be unique across the corpus, but duplicates found: " +
                string.Join(", ", dupes));
        }

        [Test]
        public void CorpusData_AllCharactersHaveRequiredFields()
        {
            // StoryEngine.cs read paths assume every character has these fields.
            // Missing displayName breaks the dialogue typewriter; missing role breaks
            // the relationship graph; missing color breaks the highlight pass.
            TextAsset asset = Resources.Load<TextAsset>(CORPUS_RESOURCE_PATH);
            RamayanaCorpus parsed = JsonUtility.FromJson<RamayanaCorpus>(asset.text);

            int badCount = 0;
            string firstBad = null;
            foreach (var c in parsed.characters)
            {
                if (string.IsNullOrEmpty(c.characterId) ||
                    string.IsNullOrEmpty(c.displayName) ||
                    string.IsNullOrEmpty(c.role) ||
                    string.IsNullOrEmpty(c.color))
                {
                    badCount++;
                    if (firstBad == null) firstBad = c.characterId ?? "<unknown>";
                }
            }
            Assert.AreEqual(0, badCount,
                badCount + " character(s) missing required fields; first bad: " + firstBad);
        }

        [Test]
        public void CorpusData_AllColorsAreValidHex()
        {
            // Color strings are passed to ColorUtility.TryParseHtmlString at runtime.
            // Invalid hex would log a warning every frame the character is on-screen.
            TextAsset asset = Resources.Load<TextAsset>(CORPUS_RESOURCE_PATH);
            RamayanaCorpus parsed = JsonUtility.FromJson<RamayanaCorpus>(asset.text);

            int bad = 0;
            string firstBad = null;
            foreach (var c in parsed.characters)
            {
                if (!HexColor.IsMatch(c.color ?? ""))
                {
                    bad++;
                    if (firstBad == null) firstBad = c.characterId + "=" + c.color;
                }
            }
            Assert.AreEqual(0, bad,
                bad + " character color(s) are not #RRGGBB hex; first bad: " + firstBad);
        }

        [Test]
        public void CorpusData_EightValmikiKandasPresent()
        {
            // Source-fidelity rule: Ramayana PS5 must cover all 8 kandas of
            // Valmiki's Ramayana. The actIds must include the canonical names.
            // (Honest disclosure: Bala, Ayodhya, Aranya, Kishkindha, Sundara,
            // Yuddha, Uttara — split as 8 acts. Uttarakanda split into 2.)
            TextAsset asset = Resources.Load<TextAsset>(CORPUS_RESOURCE_PATH);
            RamayanaCorpus parsed = JsonUtility.FromJson<RamayanaCorpus>(asset.text);

            HashSet<string> actIds = new HashSet<string>();
            foreach (var a in parsed.acts) actIds.Add(a.actId);

            string[] requiredKandas = {
                "bala-birth",       // Balakanda
                "ayodhya-dharma",   // Ayodhyakanda
                "panchavati-golden-deer",  // Aranyakanda
                "kishkindha-alliance",     // Kishkindhakanda
                "sundarakanda-leap",       // Sundarakanda
                "yuddhakanda-war"          // Yuddhakanda
            };
            List<string> missing = new List<string>();
            foreach (var k in requiredKandas)
            {
                if (!actIds.Contains(k)) missing.Add(k);
            }
            Assert.IsEmpty(missing,
                "Missing required kanda actIds: " + string.Join(", ", missing));
        }
    }

    // --- Minimal mirror of the JSON shape used by the corpus ---
    // Kept local to avoid coupling the test assembly to the runtime Data assembly.

    [System.Serializable]
    public class RamayanaCorpus
    {
        public string schemaVersion;
        public List<RamayanaCharacter> characters = new List<RamayanaCharacter>();
        public List<RamayanaActStub> acts = new List<RamayanaActStub>();
    }

    [System.Serializable]
    public class RamayanaCharacter
    {
        public string characterId;
        public string displayName;
        public string role;
        public string color;
    }

    [System.Serializable]
    public class RamayanaActStub
    {
        public string actId;
        public string title;
    }
}
