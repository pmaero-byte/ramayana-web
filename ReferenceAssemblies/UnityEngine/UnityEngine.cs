// ════════════════════════════════════════════════════════════════════════════
// UnityEngine reference assembly stubs
// ════════════════════════════════════════════════════════════════════════════
// These are minimal type definitions that allow RamayanaPS5 C# code to compile
// OUTSIDE of Unity Editor (for CI / Hermes verification).
//
// When the same code is opened in Unity Editor, the real UnityEngine.dll takes
// priority and these stubs are ignored.
//
// Reference: Unity 2022.3 LTS API surface (subset used by RamayanaPS5).
// Source: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;

namespace UnityEngine
{
    // ── Core types ────────────────────────────────────────────────────────

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0, 0);
        public static Vector2 one => new Vector2(1, 1);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 down => new Vector3(0, -1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 back => new Vector3(0, 0, -1);
        public static Vector3 left => new Vector3(-1, 0, 0);
        public static Vector3 right => new Vector3(1, 0, 0);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);
        public Vector3 normalized => magnitude > 0 ? this * (1f / magnitude) : zero;
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
    }

    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1, 1, 1, 1);
        public static Color black => new Color(0, 0, 0, 1);
        public static Color red => new Color(1, 0, 0, 1);
        public static Color green => new Color(0, 1, 0, 1);
        public static Color blue => new Color(0, 0, 1, 1);
    }

    // ── JSON utility ──────────────────────────────────────────────────────

    public static class JsonUtility
    {
        // Unity's JsonUtility serializes PUBLIC FIELDS by name (not properties).
        // It outputs lowercase-first variant (camelCase) when going FROM System.Text.Json
        // property output, but Unity's actual JsonUtility preserves field case.
        // We use a custom converter that emits field-name output for parity.
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null) return "null";
            // Emit field-based JSON (Unity-compatible)
            var sb = new System.Text.StringBuilder();
            var t = obj.GetType();
            sb.Append("{");
            bool first = true;
            foreach (var f in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"").Append(f.Name).Append("\":");
                AppendValue(sb, f.GetValue(obj));
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static void AppendValue(System.Text.StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            var t = v.GetType();
            if (t == typeof(string)) { sb.Append(System.Text.Json.JsonSerializer.Serialize(v)); return; }
            if (t == typeof(bool) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double)) { sb.Append(v.ToString()); return; }
            if (t.IsArray)
            {
                var arr = (System.Array)v;
                sb.Append("[");
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    AppendValue(sb, arr.GetValue(i));
                }
                sb.Append("]");
                return;
            }
            // Fallback: serialize as object
            sb.Append(ToJson(v));
        }

        public static T FromJson<T>(string json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var instance = new T();
            var t = typeof(T);
            foreach (var f in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!root.TryGetProperty(f.Name, out var prop)) continue;
                var val = ParseValue(prop, f.FieldType);
                if (val != null || f.FieldType == typeof(string)) f.SetValue(instance, val);
            }
            return instance;
        }

        private static object ParseValue(System.Text.Json.JsonElement prop, Type target)
        {
            if (prop.ValueKind == System.Text.Json.JsonValueKind.Null) return null;
            if (target == typeof(string)) return prop.GetString();
            if (target == typeof(bool)) return prop.GetBoolean();
            if (target == typeof(int)) return prop.GetInt32();
            if (target == typeof(long)) return prop.GetInt64();
            if (target == typeof(float)) return (float)prop.GetDouble();
            if (target == typeof(double)) return prop.GetDouble();
            if (target.IsArray)
            {
                var elemType = target.GetElementType();
                var arr = System.Array.CreateInstance(elemType, prop.GetArrayLength());
                int i = 0;
                foreach (var item in prop.EnumerateArray())
                {
                    arr.SetValue(ParseValue(item, elemType), i++);
                }
                return arr;
            }
            return null;
        }

        public static T FromJsonOverwrite<T>(string json, T obj) where T : new()
        {
            var parsed = FromJson<T>(json);
            return parsed == null ? obj : parsed;
        }
    }

    public static class ColorUtility
    {
        public static bool TryParseHtmlString(string htmlString, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(htmlString)) return false;
            try
            {
                string hex = htmlString.TrimStart('#');
                if (hex.Length == 6)
                {
                    color = new Color(
                        int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        1f
                    );
                    return true;
                }
                if (hex.Length == 8)
                {
                    color = new Color(
                        int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
                        int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f
                    );
                    return true;
                }
            }
            catch { }
            return false;
        }
    }

    // ── Object lifecycle ──────────────────────────────────────────────────

    public class Object
    {
        public string name { get; set; } = "";
        public override string ToString() => name;
    }

    public class Component : Object
    {
        public GameObject gameObject { get; set; } = new GameObject();
        public Transform transform { get; set; } = new Transform();
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
    }

    public static class Time
    {
        public static float time => 0f;
        public static float deltaTime => 0.016f;
        public static float unscaledDeltaTime => 0.016f;
        public static float realtimeSinceStartup => 0f;
        public static int frameCount => 0;
    }

    public class MonoBehaviour : Behaviour
    {
        public T GetComponent<T>() where T : Component => null;
        public T[] GetComponents<T>() where T : Component => new T[0];
        public bool TryGetComponent<T>(out T component) where T : Component { component = null; return false; }
        public static T FindObjectOfType<T>() where T : Component, new() => null;
        public Coroutine StartCoroutine(System.Collections.IEnumerator routine) => null;
        public void StopCoroutine(Coroutine coroutine) { }
        public void StopAllCoroutines() { }
        public void Invoke(string methodName, float time) { }
        public void InvokeRepeating(string methodName, float time, float repeatRate) { }
        public bool IsInvoking(string methodName) => false;
        public void CancelInvoke(string methodName = null) { }
    }

    public class Coroutine { }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    public class GameObject : Object
    {
        public Transform transform { get; set; } = new Transform();
        public GameObject() { name = "GameObject"; }
        public GameObject(string name) { this.name = name; transform = new Transform { name = name }; }
        public T AddComponent<T>() where T : Component, new() => new T();
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; } = Vector3.zero;
        public Quaternion rotation { get; set; } = Quaternion.identity;
        public Vector3 localScale { get; set; } = Vector3.one;
    }

    // ── Attributes used in our code ───────────────────────────────────────

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName { get; set; }
        public string menuName { get; set; }
        public int order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HeaderAttribute : Attribute
    {
        public string header { get; set; }
        public HeaderAttribute(string header) { this.header = header; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class TooltipAttribute : Attribute
    {
        public string tooltip { get; set; }
        public TooltipAttribute(string tooltip) { this.tooltip = tooltip; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class TextAreaAttribute : Attribute
    {
        public int minLines { get; set; } = 3;
        public int maxLines { get; set; } = 3;
        public TextAreaAttribute(int min, int max) { minLines = min; maxLines = max; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RangeAttribute : Attribute
    {
        public float min { get; set; }
        public float max { get; set; }
        public RangeAttribute(float min, float max) { this.min = min; this.max = max; }
    }

    public static class Mathf
    {
        public static float Max(float a, float b) => a > b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static int Min(int a, int b) => a < b ? a : b;
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
        public static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    }

    // ── Audio stubs ───────────────────────────────────────────────────────

    public class AudioSource : Component
    {
        public float volume { get; set; } = 1f;
        public bool isPlaying { get; set; }
        public AudioClip clip { get; set; }
        public void Play() { isPlaying = true; }
        public void Stop() { isPlaying = false; }
        public void PlayOneShot(AudioClip clip) { }
    }

    public class AudioClip : Object
    {
        public float length { get; set; }
        public bool loadInBackground { get; set; }
    }

    public enum AudioSpeakerMode { Mono, Stereo, Surround }
    public struct AudioConfiguration
    {
        public AudioSpeakerMode speakerMode;
        public int numRealVoices;
        public int numVirtualVoices;
    }

    public static class AudioSettings
    {
        public static AudioConfiguration GetConfiguration() => new AudioConfiguration { speakerMode = AudioSpeakerMode.Stereo };
        public static bool Reset(AudioConfiguration config) => true;
    }

    // ── Application paths ─────────────────────────────────────────────────

    public static class Application
    {
        public static string persistentDataPath => System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "RamayanaPS5");
    }

    public static class Debug
    {
        public static void Log(object message) => System.Console.WriteLine($"[LOG] {message}");
        public static void LogWarning(object message) => System.Console.WriteLine($"[WARN] {message}");
        public static void LogError(object message) => System.Console.WriteLine($"[ERROR] {message}");
    }
}

namespace UnityEngine.SceneManagement
{
    public class Scene { public string name { get; set; } }
}

namespace UnityEngine.SceneManagementInternal
{
    // reserved for future use
}

namespace UnityEditor
{
    using UnityEngine;
    public class EditorWindow : ScriptableObject
    {
        public Vector2 minSize { get; set; }
        public string titleContent { get; set; }
    }

    public static class EditorUtility
    {
        public static bool DisplayDialog(string title, string message, string ok) { System.Console.WriteLine($"[DIALOG] {title}: {message}"); return true; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MenuItemAttribute : Attribute
    {
        public string menuItem { get; set; }
        public bool validate { get; set; }
        public int priority { get; set; }
        public MenuItemAttribute(string menuItem) { this.menuItem = menuItem; }
        public MenuItemAttribute(string menuItem, bool validate) { this.menuItem = menuItem; this.validate = validate; }
        public MenuItemAttribute(string menuItem, bool validate, int priority) { this.menuItem = menuItem; this.validate = validate; this.priority = priority; }
    }

    public static class EditorGUILayout
    {
        public static void Label(string text, object style = null) => System.Console.WriteLine($"[Label] {text}");
        public static void Space() { }
        public static string TextField(string label, string value) { System.Console.Write($"[TextField] {label} [{value}]: "); return System.Console.ReadLine() ?? value; }
        public static bool Button(string text, object options = null, float height = 0) { System.Console.WriteLine($"[Button] {text}"); return true; }
    }

    public static class GUILayout
    {
        public static object Height(float h) => null;
    }

    public static class AssetDatabase
    {
        public static void CreateAsset(ScriptableObject asset, string path)
        {
            System.Console.WriteLine($"[AssetDatabase] Would create: {path}");
        }
        public static void SaveAssets() { }
        public static void Refresh() { }
    }

    public static class EditorStyles
    {
        public static object boldLabel => null;
    }

    public class HelpBoxAttribute : Attribute
    {
        public string text { get; set; }
        public MessageType type { get; set; }
        public HelpBoxAttribute(string text, MessageType type) { this.text = text; this.type = type; }
    }

    public enum MessageType { None, Info, Warning, Error }

    public static class EditorStylesExtensions
    {
        // not real, just here to satisfy references
    }

    // MessageType ref:
    public class MessageTypeRef { }
}

// Helper extension since GUILayout.Button signature differs
namespace UnityEngine
{
    public static class GUILayoutExtensions
    {
        // no-op
    }
}
