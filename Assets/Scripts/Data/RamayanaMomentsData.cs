// RamayanaMomentsData.cs — POCO bindings for Ramayana/moments_bala_kanda.json
// Day 12: ELGODS Bala Kanda moment corpus (36 kid-friendly moments, sarga 1-73).
// Mirrors Assets/Resources/Ramayana/moments_bala_kanda.json schema.
//
// Used by JsonUtility.FromJson<RamayanaMomentsCorpus>(text) in the runtime
// (analogous to RamayanaCharactersData.cs from Day 11 — same namespace
// Jambudweep.Ramayana.Data, parallel-list fields, not Dictionary<string,T>).
//
// Design notes:
// - 36 Bala Kanda-grounded moments cross-referenced against a 36-character
//   whitelist (26 Day 11 roster + 10 Bala Kanda additions: vasishtha,
//   dasharatha, mariachi, brahma, sumitra, menaka, kausalya, tadaka,
//   janaka, trishanku). The whitelist is exposed at corpus level so the
//   runtime can validate references without recomputing it.
// - Voice cue IDs are reserved placeholders (vc_bala_<verb>_<id>) for Day 13.
// - No graphic violence in narrative fields — the corpus enforces
//   kid-friendly retellings at the source.

using System;
using System.Collections.Generic;

namespace Jambudweep.Ramayana.Data
{
    [Serializable]
    public class RamayanaMoment
    {
        public string momentId;
        public string kanda;          // always "bala-kanda" for Day 12
        public int adhyaya;           // 1-77 for Bala Kanda
        public string verb;           // appear / speak / face-challenge / witness / chant
        public string protagonist;    // primary character ID for this moment
        public string voiceCueId;     // placeholder for Day 13 voice cues
        public string narrative;      // kid-friendly retelling (1-2 sentences)
        public string moralLesson;    // one-line takeaway
        public List<string> characters;
        public int durationSec;       // 12-30s kid pacing
        public int sargaStart;        // Valmiki sarga citation start
        public int sargaEnd;          // Valmiki sarga citation end (>= sargaStart)
    }

    [Serializable]
    public class RamayanaChoiceOption
    {
        public string id;
        public string text;
        public string lesson;
    }

    [Serializable]
    public class RamayanaChoiceDilemma
    {
        public string prompt;
        public List<RamayanaChoiceOption> options;
    }

    [Serializable]
    public class RamayanaMomentsMeta
    {
        public string schemaVersion;     // "day12-v1"
        public string kanda;             // "bala-kanda"
        public int momentsTotal;
        public List<int> adhyayasCovered;
        public int charactersTotal;
        public List<string> charactersAdditions;
        public List<string> protagonists;
        public int sargaRangeStart;
        public int sargaRangeEnd;
        public string notes;
    }

    [Serializable]
    public class RamayanaMomentsCorpus
    {
        public string project;
        public string version;
        public string sourceProject;
        public string sourceFile;
        public string exportedAt;
        public List<string> designPillars;
        public List<string> characterWhitelist;
        public string kanda;
        public List<RamayanaMoment> moments;
        public RamayanaMomentsMeta _meta;
    }
}