// UnityAssetYaml.cs — Generate Unity 2022.3 LTS .asset YAML files
// Format reference: docs.unity3d.com/Manual/ClassIDReference.html + Asset Serialization
//
// Unity .asset files for ScriptableObjects are YAML 1.1 with these required elements:
// 1. %YAML 1.1 header
// 2. %TAG !u! tag:unity3d.com,2011:
// 3. --- !u!114 &<fileID> MonoBehaviour: block
// 4. m_Script: {fileID: 11500000, guid: <GUID>, type: 3}
// 5. m_Name: <asset name>
// 6. m_EditorClassIdentifier:
// 7. <serialized fields, MonoBehaviour-style>
//
// Script GUID is in the .meta file (NOT in the .asset itself).
// We emit a placeholder GUID — Unity will match the class name on import via
// the .cs file's .meta which Unity will generate on first import.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RamayanaPS5.SanityCheck
{
    public static class UnityAssetYaml
    {
        /// <summary>
        /// Generate a Unity-compatible .asset YAML file for one ActData ScriptableObject.
        /// Matches the Unity 2022.3 LTS MonoBehaviour serialization format.
        /// Reference: docs.unity3d.com/Manual/script-Serialization.html
        /// </summary>
        public static string GenerateActAsset(Program.Act act)
        {
            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!114 &11400000");
            sb.AppendLine("MonoBehaviour:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
            sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
            sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
            sb.AppendLine("  m_GameObject: {fileID: 0}");
            sb.AppendLine("  m_Enabled: 1");
            sb.AppendLine("  m_EditorHideFlags: 0");
            sb.AppendLine("  m_Script: {fileID: 11500000, guid: ACTDATA_SCRIPT_GUID_PLACEHOLDER, type: 3}");
            sb.AppendLine($"  m_Name: Act_{act.actNumber}_{act.actId}");
            sb.AppendLine("  m_EditorClassIdentifier: ");
            // Identity
            sb.AppendLine($"  actId: {YamlQuote(act.actId)}");
            sb.AppendLine($"  actNumber: {act.actNumber}");
            sb.AppendLine($"  title: {YamlQuote(act.title)}");
            sb.AppendLine($"  location: {YamlQuote(act.location ?? "")}");
            // Scene enum
            sb.AppendLine($"  scene: {SceneEnumValue(act.scene)}");
            sb.AppendLine($"  setup: {YamlQuote(act.setup ?? "")}");
            sb.AppendLine($"  lesson: {YamlQuote(act.lesson ?? "")}");
            sb.AppendLine($"  playerRole: {YamlQuote(act.playerRole ?? "")}");
            // Objectives list
            sb.AppendLine("  objectives:");
            if (act.objectives != null)
            {
                foreach (var o in act.objectives)
                {
                    sb.AppendLine($"  - id: {YamlQuote(o.id)}");
                    sb.AppendLine($"    type: {ObjectiveTypeEnumValue(o.type)}");
                    sb.AppendLine($"    title: {YamlQuote(o.title ?? "")}");
                    sb.AppendLine($"    marker: {YamlQuote(o.marker ?? "")}");
                    sb.AppendLine($"    cue: {YamlQuote(o.cue ?? "")}");
                    sb.AppendLine($"    actionLabel: {YamlQuote(o.actionLabel ?? "")}");
                    sb.AppendLine($"    completedLine: {{speaker: {YamlQuote(o.completedLine?.speaker)}, text: {YamlQuote(o.completedLine?.text)}}}");
                    sb.AppendLine($"    target: {o.target}");
                    if (o.position != null)
                        sb.AppendLine($"    position: {{x: {o.position.x}, y: {o.position.y}, z: {o.position.z}}}");
                    else
                        sb.AppendLine("    position: {x: 0, y: 0, z: 0}");
                }
            }
            // Dialogue list
            sb.AppendLine("  dialogue:");
            if (act.dialogue != null)
            {
                foreach (var d in act.dialogue)
                {
                    sb.AppendLine($"  - speaker: {YamlQuote(d.speaker)}");
                    sb.AppendLine($"    text: {YamlQuote(d.text ?? "")}");
                    sb.AppendLine("    voice: 10"); // kathaka (default narrator)
                }
            }
            // Empty lists for items not in corpus yet
            sb.AppendLine("  consequenceEchoes: []");
            sb.AppendLine("  shlokaStones: []");
            sb.AppendLine("  hiddenCollectibles: []");
            // Reward
            sb.AppendLine("  reward: {badge: \"\", lore: \"\", dharma: 0}");
            return sb.ToString();
        }

        static string YamlQuote(string s)
        {
            if (s == null) return "\"\"";
            // Escape backslashes and double quotes
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        static int SceneEnumValue(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "ayodhya": return 0;
                case "panchavati": return 1;
                case "kishkindha": return 2;
                case "lanka": return 3;
                case "setu": return 4;
                case "return": return 5;
                case "bala": return 6;
                case "janakpur": return 7;
                case "ashram": return 8;
                case "ocean": return 9;
                case "ravana-court": return 10;
                case "earth-return": return 11;
                case "mithila": return 12;
                case "chitrakuta": return 13;
                case "nandigram": return 14;
                case "sarayu": return 15;
                case "lanka-palace": return 16;
                case "lanka-garden": return 17;
                default: return 0;
            }
        }

        static int ObjectiveTypeEnumValue(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "choice": return 0;
                case "talk": return 1;
                case "travel": return 2;
                case "collect": return 3;
                case "use": return 4;
                case "witness": return 5;
                case "combat": return 6;
                default: return 5;
            }
        }

        // VoiceRegister enum string → int (matches RamayanaTypes.cs order)
        static int VoiceRegisterEnumValue(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "sloka": return 0;
                case "vaachaka": return 1;
                case "shastriya": return 2;
                case "praarthana": return 3;
                case "sheershata": return 4;
                case "vairagya": return 5;
                case "maatru": return 6;
                case "pratishedha": return 7;
                case "dainya": return 8;
                case "niti": return 9;
                case "kathaka": return 10;
                default: return 10;  // kathaka = narrator default
            }
        }

        // CharacterAlignment enum string → int
        static int CharacterAlignmentEnumValue(string s)
        {
            switch ((s ?? "").ToLower())
            {
                case "hero": return 0;
                case "villain": return 1;
                case "sage": return 2;
                case "supporting": return 3;
                case "divine": return 4;
                case "tragic": return 5;
                case "ally": return 6;
                default: return 3;
            }
        }

        // Voice register mapping (parity with elgods characterVoices.ts)
        // Used to derive CharacterData.voice from character ID
        static string GetVoiceForCharacter(string characterId)
        {
            switch ((characterId ?? "").ToLower())
            {
                case "rama": return "sloka";
                case "hanuman": return "vaachaka";
                case "ravana": case "kumbhakarna": return "shastriya";
                case "sita": case "kausalya": return "praarthana";
                case "lakshmana": case "shatrughna": case "bharata": return "sheershata";
                case "kaikeyi": return "vairagya";
                case "sumitra": return "maatru";
                case "jatayu": case "sampati": return "pratishedha";
                case "sugriva": case "tara": case "shabari": return "dainya";
                case "vibhishana": case "mandodari": return "niti";
                case "valmiki": case "narada": case "kathaka": return "kathaka";
                default: return "kathaka";
            }
        }

        // CharacterAlignment mapping from role string
        static string GetAlignmentForRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return "supporting";
            var r = role.ToLower();
            if (r.Contains("villain") || r.Contains("demon") || r.Contains("rakshas")) return "villain";
            if (r.Contains("sage") || r.Contains("rishi") || r.Contains("maharishi") || r.Contains("ascetic")) return "sage";
            if (r.Contains("avatar") || r.Contains("divine") || r.Contains("god")) return "divine";
            if (r.Contains("queen") || r.Contains("king") || r.Contains("prince") || r.Contains("princess")) return "hero";
            if (r.Contains("spy") || r.Contains("messenger")) return "supporting";
            return "supporting";
        }

        /// <summary>
        /// Generate a Unity-compatible .asset YAML file for one CharacterData ScriptableObject.
        /// Matches the Unity 2022.3 LTS MonoBehaviour serialization format.
        /// </summary>
        public static string GenerateCharacterAsset(Program.Character character)
        {
            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!114 &11400000");
            sb.AppendLine("MonoBehaviour:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
            sb.AppendLine("  m_PrefabInstance: {fileID: 0}");
            sb.AppendLine("  m_PrefabAsset: {fileID: 0}");
            sb.AppendLine("  m_GameObject: {fileID: 0}");
            sb.AppendLine("  m_Enabled: 1");
            sb.AppendLine("  m_EditorHideFlags: 0");
            sb.AppendLine("  m_Script: {fileID: 11500000, guid: CHARACTERDATA_SCRIPT_GUID_PLACEHOLDER, type: 3}");
            sb.AppendLine($"  m_Name: Character_{character.characterId}");
            sb.AppendLine("  m_EditorClassIdentifier: ");
            sb.AppendLine($"  characterId: {YamlQuote(character.characterId)}");
            sb.AppendLine($"  characterName: {YamlQuote(character.displayName)}");
            sb.AppendLine($"  tamilName: {YamlQuote(character.displayNameTamil ?? "")}");
            sb.AppendLine($"  sanskritName: {YamlQuote("")}");  // not in corpus; can be filled in editor
            sb.AppendLine($"  role: {YamlQuote(character.role ?? "")}");
            // Voice: derive from characterId using canonical mapping
            string voice = GetVoiceForCharacter(character.characterId);
            sb.AppendLine($"  voice: {VoiceRegisterEnumValue(voice)}");
            // Alignment: derive from role
            string alignment = GetAlignmentForRole(character.role);
            sb.AppendLine($"  alignment: {CharacterAlignmentEnumValue(alignment)}");
            sb.AppendLine($"  colorHex: {YamlQuote(character.color ?? "#FFFFFF")}");
            sb.AppendLine($"  icon: {YamlQuote("")}");  // emoji/icon not in corpus
            // Dharma alignment — rough heuristic by alignment type
            int dharma = alignment == "hero" || alignment == "divine" || alignment == "sage" ? 80 :
                         alignment == "villain" ? -50 :
                         alignment == "ally" ? 60 :
                         alignment == "tragic" ? 20 : 30;
            sb.AppendLine($"  dharmaAlignment: {dharma}");
            sb.AppendLine($"  description: {YamlQuote("")}");  // would need full corpus; empty for now
            sb.AppendLine($"  keyQuote: {YamlQuote("")}");
            // Empty relationships list (would need relationship data from corpus)
            sb.AppendLine("  relationships: []");
            sb.AppendLine($"  unlockCondition: {YamlQuote("default:unlocked")}");
            sb.AppendLine($"  unlockedByDefault: {(character.characterId == "rama" ? "1" : "0")}");
            return sb.ToString();
        }
    }
}
