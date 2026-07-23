// RamayanaVoicesData.cs — POCO bindings for Ramayana/voices.json
// Day 13: ELGODS alankarashastra voice register corpus (11 registers + 55
// character bindings + 36 Bala Kanda voice cues).
// Mirrors Assets/Resources/Ramayana/voices.json schema.
//
// Alankarashastra source: Dandin, Kavyadarsha III.1-15; Bhamaha, Kavyalankara.
// Each of the 11 voice registers has a fixed Sanskrit label + English meaning +
// a canonical exemplar character (e.g., Rama=sloka, Hanuman=vaachaka,
// Ravana=shastriya).
//
// Used by JsonUtility.FromJson<RamayanaVoicesCorpus>(text) in the runtime
// (same pattern as RamayanaCharactersData.cs Day 11 + RamayanaMomentsData.cs
// Day 12 — namespace Jambudweep.Ramayana.Data, parallel-list fields, not
// Dictionary<string,T>).
//
// characterVoiceBindings is exposed as a parallel list of (characterId,
// registerId) pairs because JsonUtility cannot deserialize a Dictionary.
// The runtime can rebuild the dictionary by walking the parallel list at
// Build time (one extra pass, mirroring the Day 11 characters pattern).
//
// Voice cue slots (one per Day 12 Bala Kanda moment) carry kid-friendly
// verbal texture descriptions. Audio content (audioFile) is reserved for
// the Day 14+ voice pipeline; runtime can fall back to TTS engine reading
// narrationLine + texture.

using System;
using System.Collections.Generic;

namespace Jambudweep.Ramayana.Data
{
    [Serializable]
    public class RamayanaVoiceRegister
    {
        public string id;            // e.g., "sloka", "vaachaka"
        public string english;       // e.g., "Measured", "Proclamatory"
        public string sanskrit;      // e.g., "श्लोक", "वाचक"
        public string description;   // alankarashastra definition
        public string exemplar;      // canonical character who uses this register
        public string family;        // royal / devotional / scholarly / etc.
    }

    [Serializable]
    public class RamayanaCharacterVoiceBinding
    {
        public string characterId;   // e.g., "rama", "hanuman"
        public string registerId;    // references RamayanaVoiceRegister.id
    }

    [Serializable]
    public class RamayanaVoiceCue
    {
        public string cueId;                 // vc_bala_<verb>_<momentId>
        public string momentId;              // cross-reference to moments_bala_kanda.json
        public string kanda;                 // "bala-kanda"
        public string register;              // register ID
        public string registerEnglish;
        public string registerSanskrit;
        public string speaker;               // character ID who narrates this cue
        public string speakerVoiceFamily;    // register.family
        public int durationSec;
        public string texture;               // kid-friendly verbal texture
        public string audioFile;             // null until Day 14+ voice pipeline
        public string narrationLine;         // mirrors the moment's narrative
    }

    [Serializable]
    public class RamayanaVoicesMeta
    {
        public string schemaVersion;
        public int registersTotal;
        public int charactersTotal;
        public int voiceCuesTotal;
        public int charactersWithAudioPending;
        public string notes;
    }

    [Serializable]
    public class RamayanaVoicesCorpus
    {
        public string project;
        public string version;
        public string sourceProject;
        public string sourceFile;
        public string exportedAt;
        public List<string> designPillars;
        public List<RamayanaVoiceRegister> registers;
        public List<RamayanaCharacterVoiceBinding> characterVoiceBindings;
        public string kanda;
        public List<RamayanaVoiceCue> voiceCues;
        public RamayanaVoicesMeta _meta;
    }
}