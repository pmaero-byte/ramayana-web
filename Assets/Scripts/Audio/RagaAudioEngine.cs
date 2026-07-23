// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Audio Engine (Raga Drone System)
// C# port of `audio.ts` (37KB, ~960 LOC) — Web Audio synth → Unity AudioSource
// 6 Carnatic ragas mapped to 12 scenes
// Reference: elgods-nextjs-content-game skill (6 ragas + 11 SFX types)
// ════════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jambudweep.Ramayana.Audio
{
    /// <summary>
    /// 6 Carnatic ragas mapped to scenes. From types.ts lines 470-477 (paraphrased).
    /// bhairavi → bala, janakpur
    /// kalyani → ayodhya
    /// hamsadhwani → panchavati, ashram
    /// desh → kishkindha, ocean, setu
    /// bhairav → lanka, ravana-court
    /// poorvikalyan → return, earth-return
    /// </summary>
    public enum Raga
    {
        Bhairavi,
        Kalyani,
        Hamsadhwani,
        Desh,
        Bhairav,
        Poorvikalyan
    }

    public enum SoundEffectType
    {
        // Original 11 (gameData.ts)
        Collect,
        Place,
        Light,
        Complete,
        Alert,
        Travel,
        Arrow,
        Dharma,
        Shloka,
        Ghanta,
        Footstep,
        // Combat additions (June 18 session)
        MeleeLight,
        MeleeHeavy,
        Parry,
        Dash,
        Dodge,
        BossTelegraph,
        VictoryFanfare
    }

    [CreateAssetMenu(fileName = "Raga_", menuName = "Ramayana/Raga Data", order = 10)]
    public class RagaData : ScriptableObject
    {
        public Raga raga;
        public string displayName;
        [Tooltip("Comma-separated swara sequence, e.g. 'Sa,Ri,Ga,Ma,Pa,Dha,Ni'")]
        public string swaraSequence;
        [Tooltip("Base oscillator frequency in Hz (Sa)")]
        public float baseFrequencyHz = 220f;
        [Tooltip("Drone companions — usually Sa + Pa (5th)")]
        public string droneNotes = "Sa,Pa";
    }

    /// <summary>
    /// Procedural audio engine. Renders Carnatic raga drones + 18 SFX types
    /// using oscillator synthesis. On PS5, swap oscillators for pre-rendered
    /// samples loaded from StreamingAssets for richer timbre.
    /// </summary>
    public class RagaAudioEngine : MonoBehaviour
    {
        [Header("Active Raga")]
        public Raga activeRaga = Raga.Kalyani;
        public RagaData[] allRagas;

        [Header("Audio Sources")]
        public AudioSource droneSourceA;
        public AudioSource droneSourceB;
        public AudioSource ambientSource;
        public AudioSource sfxSource;
        public AudioSource voSource;

        [Header("Volumes")]
        [Range(0f, 1f)] public float droneVolume = 0.4f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float voVolume = 0.9f;

        [Header("VO Ducking")]
        public float voDuckDb = -10f;
        public float voDuckAttackSec = 0.25f;
        public float voDuckReleaseSec = 0.6f;

        private float _duckTimer;

        // ── API ────────────────────────────────────────────────────────

        public void CrossfadeToRaga(Raga target, float fadeSec = 1.5f)
        {
            activeRaga = target;
            if (droneSourceA != null && droneSourceB != null)
            {
                StartCoroutine(FadePair(fadeSec));
            }
            Debug.Log($"[Raga] Crossfading to {target} over {fadeSec}s");
        }

        private bool _droneAActive = true;

        private IEnumerator FadePair(float fadeSec)
        {
            if (fadeSec <= 0f) yield break;
            float fromSource = _droneAActive ? droneSourceA.volume : droneSourceB.volume;
            float toSource   = _droneAActive ? droneSourceB.volume : droneSourceA.volume;
            float t = 0f;
            while (t < fadeSec)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeSec);
                if (_droneAActive)
                {
                    droneSourceA.volume = Mathf.Lerp(fromSource, 0f, k);
                    droneSourceB.volume = Mathf.Lerp(toSource, 1f, k);
                }
                else
                {
                    droneSourceA.volume = Mathf.Lerp(toSource, 1f, k);
                    droneSourceB.volume = Mathf.Lerp(fromSource, 0f, k);
                }
                yield return null;
            }
            if (_droneAActive)
            {
                droneSourceA.volume = 0f;
                droneSourceB.volume = 1f;
            }
            else
            {
                droneSourceA.volume = 1f;
                droneSourceB.volume = 0f;
            }
            _droneAActive = !_droneAActive;
        }

        public void DuckForVO()
        {
            _duckTimer = voDuckAttackSec;
        }

        public void UnduckAfterVO()
        {
            _duckTimer = -voDuckReleaseSec;
        }

        public void PlaySfx(SoundEffectType type)
        {
            sfxSource.PlayOneShot(GenerateSfx(type));
        }

        public static RagaAudioEngine Instance { get; private set; }

        private AudioClip GenerateSfx(SoundEffectType type)
        {
            float durationSec;
            float baseFreq;
            float decayRate;
            int harmonics = 1;
            bool useNoise = false;
            float noiseMix = 0f;
            bool sweep = false;

            switch (type)
            {
                case SoundEffectType.Collect:
                    durationSec = 0.18f; baseFreq = 1200f; harmonics = 2; decayRate = 8f; break;
                case SoundEffectType.Place:
                    durationSec = 0.22f; baseFreq = 90f; decayRate = 12f; break;
                case SoundEffectType.Light:
                    durationSec = 0.35f; baseFreq = 440f; harmonics = 3; decayRate = 3f; break;
                case SoundEffectType.Complete:
                    durationSec = 0.55f; baseFreq = 523f; harmonics = 4; decayRate = 1.8f; sweep = true; break;
                case SoundEffectType.Alert:
                    durationSec = 0.4f; baseFreq = 880f; decayRate = 5f; sweep = true; break;
                case SoundEffectType.Travel:
                    durationSec = 0.55f; baseFreq = 220f; harmonics = 2; decayRate = 1.5f; break;
                case SoundEffectType.Arrow:
                    durationSec = 0.12f; baseFreq = 1800f; decayRate = 20f; useNoise = true; noiseMix = 0.35f; break;
                case SoundEffectType.Dharma:
                    durationSec = 0.7f; baseFreq = 196f; harmonics = 2; decayRate = 1.2f; break;
                case SoundEffectType.Shloka:
                    durationSec = 0.8f; baseFreq = 330f; harmonics = 3; decayRate = 1f; break;
                case SoundEffectType.Ghanta:
                    durationSec = 1.0f; baseFreq = 800f; harmonics = 5; decayRate = 0.8f; break;
                case SoundEffectType.Footstep:
                    durationSec = 0.1f; baseFreq = 60f; decayRate = 25f; useNoise = true; noiseMix = 0.5f; break;
                case SoundEffectType.MeleeLight:
                    durationSec = 0.13f; baseFreq = 320f; harmonics = 2; decayRate = 18f; break;
                case SoundEffectType.MeleeHeavy:
                    durationSec = 0.25f; baseFreq = 70f; harmonics = 3; decayRate = 7f; useNoise = true; noiseMix = 0.25f; break;
                case SoundEffectType.Parry:
                    durationSec = 0.15f; baseFreq = 2400f; harmonics = 2; decayRate = 14f; break;
                case SoundEffectType.Dash:
                    durationSec = 0.2f; baseFreq = 300f; harmonics = 2; decayRate = 9f; sweep = true; break;
                case SoundEffectType.Dodge:
                    durationSec = 0.12f; baseFreq = 500f; decayRate = 13f; break;
                case SoundEffectType.BossTelegraph:
                    durationSec = 0.85f; baseFreq = 55f; harmonics = 2; decayRate = 1f; sweep = true; break;
                case SoundEffectType.VictoryFanfare:
                    durationSec = 1.05f; baseFreq = 440f; harmonics = 4; decayRate = 1.6f; sweep = true; break;
                default:
                    durationSec = 0.2f; baseFreq = 220f; decayRate = 5f; break;
            }

            int totalSamples = (int)(44100 * durationSec);
            var clip = AudioClip.Create("sfx_" + type, totalSamples, 1, 44100, false);
            float[] data = new float[totalSamples];
            float phase = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * decayRate);
                env = Mathf.Clamp01(env);
                float sample = 0f;
                float freqMul = sweep ? (1f - t * 0.6f) : 1f;
                if (useNoise && noiseMix > 0f)
                {
                    // Deterministic-ish noise from phase accumulator.
                    float n = Mathf.Sin(phase * 12.9898f + 78.233f);
                    sample += noiseMix * (n - Mathf.Floor(n + 0.5f) * 2f) * env;
                }
                for (int h = 1; h <= harmonics; h++)
                {
                    float harmonicFreq = Mathf.Max(20f, baseFreq * h * freqMul);
                    phase += harmonicFreq / 44100f;
                    sample += (1f / h) * Mathf.Sin(2f * Mathf.PI * phase) * env;
                }
                data[i] = Mathf.Clamp(sample * 0.35f, -1f, 1f);
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
