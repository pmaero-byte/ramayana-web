// RamayanaCharactersData.cs — POCO bindings for Ramayana/characters.json
// Day 11: ELGODS character roster import (26 canonical characters + 5 groups).
// Mirrors Assets/Resources/Ramayana/characters.json schema.
//
// Used by JsonUtility.FromJson<RamayanaCharacterRoster>(text) in the runtime
// (analogous to the existing Verse/VerseData.cs pattern + Yuddhakanta Kids/
// KidsDataModel.cs pattern).

using System;
using System.Collections.Generic;

namespace Jambudweep.Ramayana.Data
{
    [Serializable]
    public class RamayanaCharacter
    {
        public string id;
        public string name;
        public string tamilName;
        public string sanskritName;
        public string role;
        public string voice;
        public string color;   // hex like "#4f8cff"
        public string icon;
        public string alignment;  // hero / sage / divine / ally / tragic / villain / supporting
    }

    [Serializable]
    public class RamayanaCharacterGroup
    {
        public string id;
        public string name;
        public string description;
        public List<string> characters;
        public int charactersTotal;
    }

    [Serializable]
    public class RamayanaCharacterRosterMeta
    {
        public string schemaVersion;
        public int charactersTotal;
        public int groupsTotal;
        public List<string> voicesUsed;
        public string notes;
    }

    [Serializable]
    public class RamayanaCharacterRoster
    {
        public string project;
        public string version;
        public string sourceProject;
        public string sourceFile;
        public string exportedAt;
        public List<string> designPillars;
        public List<RamayanaCharacterGroup> groups;
        // JsonUtility deserializes a JSON object into a Dictionary<string,T>
        // only via a serializable wrapper. To keep things simple, we expose
        // Characters as a parallel-list view that the runtime can rebuild
        // by reading the underlying JsonObject (one extra parse pass at
        // Build time).
        //
        // The existing Scene/Story/StoryMomentPlayer.cs uses the same pattern
        // (ActData has parallel fields, not Dictionary<string,T>) — see how
        // it parses corpus_data.json's `acts` array. We follow that convention.
        public List<RamayanaCharacter> characters;
        public RamayanaCharacterRosterMeta _meta;
    }
}