// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Core Data Types
// C# port of `src/game/ramayana/types.ts` (Next.js / Three.js web build)
// Target: Unity 2022.3 LTS + PS5 + Xbox Series X|S
// Reference: types.ts lines 7-499
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Data
{
    // ── Enums ──────────────────────────────────────────────────────────────

    public enum ObjectiveType
    {
        Choice,
        Talk,
        Travel,
        Collect,
        Use,
        Witness,
        Combat
    }

    public enum StoryMomentType
    {
        Cutscene,    // Camera pans, text appears, characters act
        Dialogue,    // Character portrait + text + typewriter
        Choice,      // Multiple options with dharma impact
        Exploration, // "Find [object/person]" in the 3D world
        Witness,     // Watch an event unfold
        Combat,      // Simplified combat sequence
        Reflection,  // Internal monologue, lesson learned
        Transition,  // Time/location change with narration
        Shloka       // Display a sacred verse with translation
    }

    public enum RamayanaScene
    {
        Ayodhya,
        Panchavati,
        Kishkindha,
        Lanka,
        Setu,
        Return,
        Bala,
        Janakpur,
        Ashram,
        Ocean,
        RavanaCourt,
        EarthReturn,
        Mithila,
        Chitrakuta,
        Nandigram,
        Sarayu,
        LankaPalace,
        LankaGarden
    }

    public enum CameraMoveType
    {
        Pan,
        Zoom,
        Orbit,
        Shake,
        Focus,
        Flyover
    }

    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public enum MusicMood
    {
        Devotional,
        Tense,
        Peaceful,
        Triumphant,
        Sorrowful,
        Mysterious,
        Epic,
        Joyful,
        Hopeful
    }

    public enum Instrument
    {
        Veena,
        Flute,
        Mridangam,
        Sitar,
        Tanpura,
        Ensemble
    }

    /// <summary>
    /// 11 voice registers from the Sanskrit alankarashastra tradition.
    /// See types.ts lines 134-146 for canonical definitions.
    /// Reference: voices mapping in characterVoices.ts.
    /// </summary>
    public enum VoiceRegister
    {
        Sloka,        // Measured, even syllables — kings (Rama)
        Vaachaka,     // Proclamatory, rhythmic refrains — devotees (Hanuman)
        Shastriya,    // Scholarly, cites scripture — scholars (Ravana)
        Praarthana,   // Devotional, supplicating — devotees (Sita)
        Sheershata,   // Service-oriented — devoted servants (Lakshmana)
        Vairagya,     // Renunciate, mournful (Bharata)
        Maatru,       // Maternal, blessing (Kausalya, Sumitra)
        Pratishedha,  // Counsel who warns against (Jatayu, Sampati)
        Dainya,       // Pleading, seeking help (Sugriva, Tara)
        Niti,         // Statecraft counsel (Vibhishana, Mandodari)
        Kathaka       // Narrator (Valmiki, Narada)
    }

    public enum DharmaCategory
    {
        Compassion,
        Duty,
        Courage,
        Wisdom
    }

    public enum CharacterAlignment
    {
        Hero,
        Villain,
        Sage,
        Supporting,
        Divine,
        Tragic,
        Ally
    }

    public enum EmotionalTone
    {
        Hopeful,
        Tense,
        Joyful,
        Sorrowful,
        Triumphant,
        Peaceful
    }

    public enum KnowledgeCategory
    {
        Mythology,
        History,
        Culture,
        Geography,
        Philosophy,
        Art,
        Language,
        Science
    }

    public enum HiddenCollectibleType
    {
        MurtiFragment,
        TempleStamp,
        Relic
    }

    public enum StoryEngineMode
    {
        Idle,
        CharacterSelect,
        Prologue,
        Playing,
        Paused,
        Cutscene,
        Dialogue,
        Choice,
        Exploration,
        Combat,
        Transition,
        Summary
    }

    public enum RelationshipType
    {
        Spouse,
        Sibling,
        Parent,
        Child,
        Ally,
        Enemy,
        Rival,
        Mentor,
        Devotee,
        Servant,
        Friend
    }

    // ── Structs ─────────────────────────────────────────────────────────────

    [Serializable]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
        public static Vec3 FromVector3(Vector3 v) => new Vec3(v.x, v.y, v.z);
    }

    [Serializable]
    public struct RamayanaChoice
    {
        public string id;
        public string label;
        public string description;
        public int dharmaImpact;
        public DharmaCategory dharmaCategory;
        public string consequenceText;
        public string nextMomentId;
        public string unlocksCharacter;
    }

    [Serializable]
    public struct CameraMove
    {
        public CameraMoveType type;
        public Vec3 from;
        public Vec3 to;
        public Vec3 target;
        public float durationMs;
        public EasingType easing;
    }

    [Serializable]
    public struct MusicCue
    {
        public string raga;
        public MusicMood mood;
        public Instrument instrument;
        public float fadeInMs;
        public float fadeOutMs;
    }

    [Serializable]
    public struct ShlokaStone
    {
        public string id;
        public string title;
        [TextArea(2, 4)] public string sanskrit;
        [TextArea(2, 4)] public string transliteration;
        public string phonetic;
        [TextArea(2, 4)] public string translation;
        public string source;
        public Vec3 position;
        public RamayanaScene scene;
    }

    [Serializable]
    public struct HiddenCollectible
    {
        public string id;
        public HiddenCollectibleType type;
        public string title;
        public string encyclopediaLink;
        public string description;
        public Vec3 position;
        public RamayanaScene scene;
        public string glowColor;
    }

    [Serializable]
    public struct ConsequenceEcho
    {
        public string actId;
        public string requiredChoiceActId;
        public string requiredChoiceLabel;
        public string speaker;
        [TextArea(2, 4)] public string text;
    }

    [Serializable]
    public struct ObjectiveCompletedLine
    {
        public string speaker;
        [TextArea(2, 4)] public string text;
    }

    [Serializable]
    public struct ObjectiveChoice
    {
        public string label;
        public string description;
        public int dharmaImpact;
        public DharmaCategory dharmaCategory;
    }

    [Serializable]
    public struct CharacterRelationship
    {
        public string from;
        public string to;
        public RelationshipType type;
        public string description;
        [Range(1, 10)] public int strength;
    }

    [Serializable]
    public struct KeyChoice
    {
        public string momentId;
        public string description;
    }

    // ── Save System Constants ──────────────────────────────────────────────

    /// <summary>
    /// Local + PS5 cloud save slot keys.
    /// Reference: ps5-certification-checklist.md line 14
    /// </summary>
    public static class SaveKeys
    {
        public const string SaveVersion = "ramayana-save-version";
        public const int CurrentVersion = 1;

        public const string Slot1 = "ramayana-slot-1";
        public const string Slot2 = "ramayana-slot-2";
        public const string Slot3 = "ramayana-slot-3";
        public const string AutoSave = "ramayana-autosave";
        public const string QuickSave = "ramayana-quicksave";
    }
}
