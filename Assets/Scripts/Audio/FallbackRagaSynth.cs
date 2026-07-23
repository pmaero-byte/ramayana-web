// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Fallback Voice/SFX Synth (Day 33)
// Procedural placeholder audio for voice cue IDs until real recordings are
// available.
//
// Generates an AudioClip per cueId using additive synthesis with raga
// parameterization, then plays it through a user-assigned AudioSource.
// Zero asset dependencies: verified by hermes-verify-audio-day33.sh.
// ════════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Audio
{
    // Keep ordinal values stable and aligned with RagaAudioEngine.Raga.
    public enum RagaSynthKind
    {
        Bhairavi   = 0,
        Kalyani    = 1,
        Hamsadhwani = 2,
        Desh       = 3,
        Bhairav    = 4,
        Poorvikalyan = 5
    }

    // Deterministic pseudo-random from a cueId string.
    internal static class CueHash
    {
        public static int From(string cueId)
        {
            if (string.IsNullOrEmpty(cueId)) return 0;
            uint h = 2166136261u;
            foreach (char c in cueId) { h ^= (uint)c; h *= 16777619u; }
            return (int)h;
        }
    }

    // Lightweight config for a generated clip.
    [System.Serializable]
    public struct SynthDescriptor
    {
        public RagaSynthKind raga;
        public float baseFreq;    // Hz (Sa)
        public float durationSec;
        public float amplitude;
        public int harmonicCount;
    }

    // Deterministic synth descriptor → raga-preset mapping.
    internal static class RagaPreset
    {
        private static readonly System.Collections.Generic.Dictionary<RagaSynthKind, (float root, float[] ratios)> _table
            = new System.Collections.Generic.Dictionary<RagaSynthKind, (float, float[])>
        {
            { RagaSynthKind.Bhairavi,    (220f, new float[]{1f, 1.059f, 1.122f, 1.189f, 1.335f, 1.498f, 1.587f}) },
            { RagaSynthKind.Kalyani,     (247f, new float[]{1f, 1.059f, 1.122f, 1.189f, 1.335f, 1.498f, 1.587f}) },
            { RagaSynthKind.Hamsadhwani, (262f, new float[]{1f, 1.059f, 1.122f, 1.335f, 1.498f}) },
            { RagaSynthKind.Desh,        (234f, new float[]{1f, 1.059f, 1.122f, 1.189f, 1.26f, 1.498f, 1.681f}) },
            { RagaSynthKind.Bhairav,     (208f, new float[]{1f, 1.059f, 1.122f, 1.189f, 1.26f, 1.498f, 1.587f}) },
            { RagaSynthKind.Poorvikalyan,(220f, new float[]{1f, 1.059f, 1.122f, 1.189f, 1.335f, 1.498f, 1.682f}) }
        };

        public static SynthDescriptor For(RagaSynthKind raga, int seed)
        {
            var (root, ratios) = _table[raga];
            int hCount = 2 + ((seed >> 3) & 3); // 2..5
            float amp = 0.6f + ((seed % 100) / 400f);
            float dur = 2.0f + ((seed % 7) * 0.05f); // 2.0s..2.3s
            return new SynthDescriptor
            {
                raga = raga,
                baseFreq = root,
                durationSec = dur,
                amplitude = Mathf.Clamp(amp, 0.05f, 0.35f),
                harmonicCount = hCount
            };
        }
    }

    /// <summary>
    /// One-shot synthesizer. Produces a fresh AudioClip once per cueId,
    /// then caches it for the rest of the session.
    /// </summary>
    public sealed class FallbackRagaSynth : MonoBehaviour
    {
        public static FallbackRagaSynth Instance { get; private set; }

        [Header("Routing")]
        [Tooltip("Auto-assigned from RagaAudioEngine.voSource at runtime.")]
        [SerializeField] private AudioSource sinkSource;

        private readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();
        private const int SampleRate = 44100;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>
        /// Returns an AudioClip for this cueId, creating it if necessary.
        /// Also plays it through sinkSource if assigned.
        /// </summary>
        public AudioClip GetOrCreate(string cueId, bool autoPlay = true)
        {
            if (string.IsNullOrEmpty(cueId)) return null;
            if (_cache.TryGetValue(cueId, out var cached)) return cached;

            var desc = GenerateDescriptor(cueId);
            var clip = RenderClip(desc);
            _cache[cueId] = clip;
            if (autoPlay && sinkSource != null)
            {
                sinkSource.PlayOneShot(clip, desc.amplitude);
            }
            return clip;
        }

        public void SetSink(AudioSource source) => sinkSource = source;

        public bool Has(string cueId) => _cache.ContainsKey(cueId);

        public int CachedCount => _cache.Count;

        // ── Internals ──────────────────────────────────────────────────

        private SynthDescriptor GenerateDescriptor(string cueId)
        {
            int seed = CueHash.From(cueId);
            RagaSynthKind raga = (RagaSynthKind)(seed % 6);
            return RagaPreset.For(raga, seed);
        }

        private AudioClip RenderClip(SynthDescriptor desc)
        {
            int totalSamples = (int)(SampleRate * desc.durationSec);
            var clip = AudioClip.Create(
                "synth_" + desc.raga + "_" + desc.baseFreq,
                totalSamples,
                1,
                SampleRate,
                false);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float sample = 0f;
                float masterGain = 0.18f * desc.amplitude;
                for (int h = 1; h <= desc.harmonicCount; h++)
                {
                    float ratio = h;
                    float lfo = 1f + 0.35f * Mathf.Sin(2f * Mathf.PI * (0.25f + 0.11f * h) * t + h);
                    float env = Mathf.Exp(-t * 0.55f - h * 0.25f);
                    sample += (1f / ratio) * lfo * env * Mathf.Sin(2f * Mathf.PI * desc.baseFreq * ratio * t);
                }
                data[i] = masterGain * sample;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
