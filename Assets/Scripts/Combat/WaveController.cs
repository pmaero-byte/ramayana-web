// Day 3 + Day 10 (RamayanaPS5) — WaveController.
// Spawns waves of RakshasaTargets around the player and tracks progress.
// Day 10: pluggable FormationStrategy (Arc / Chakra / Vyuha).

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Jambudweep.Ramayana.Combat
{
    public sealed class WaveController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Transform used as the spawn anchor for each wave.")]
        [SerializeField] private Transform player;

        [Header("Wave shape")]
        [SerializeField, Min(1)] private int rakshasasPerWave = 3;
        [SerializeField, Min(0.1f)] private float spawnRadius = 6f;
        [SerializeField, Min(0.5f)] private float secondsBetweenWaves = 2.5f;
        [SerializeField] private FormationKind formationKind = FormationKind.Arc;

        [Header("Prefab (optional)")]
        [Tooltip("If null, a basic capsule with RakshasaTarget is created at runtime.")]
        [SerializeField] private RakshasaTarget rakshasaPrefab;

        [Header("Events")]
        public UnityEvent<int> onWaveStarted = new UnityEvent<int>();   // 1-based wave index
        public UnityEvent<int> onWaveCompleted = new UnityEvent<int>();
        public UnityEvent onAllWavesCompleted = new UnityEvent();

        private readonly List<RakshasaTarget> _alive = new List<RakshasaTarget>();
        private FormationStrategy _formation;
        private int _currentWave = 0;
        private int _totalWaves = 1;
        private bool _running;

        public int CurrentWave => _currentWave;
        public int TotalWaves => _totalWaves;
        public int AliveCount => _alive.FindAll(r => r != null && !r.IsDead).Count;
        public bool Running => _running;
        public FormationKind Formation => formationKind;

        void Awake()
        {
            if (player == null) player = FindFirstObjectByType<Jambudweep.Ramayana.Motion3D.ThirdPersonMotionController>()?.transform;
            _formation = FormationStrategy.For(formationKind);
        }

        // ── Public API ─────────────────────────────────────────────

        public void StartWaves(int totalWaves = 1)
        {
            _totalWaves = Mathf.Max(1, totalWaves);
            _currentWave = 0;
            _alive.Clear();
            _running = true;
            if (_formation == null) _formation = FormationStrategy.For(formationKind);
            StartCoroutine(RunWaves());
        }

        public void StopWaves()
        {
            _running = false;
            StopAllCoroutines();
        }

        /// <summary>Swap formation mid-session (e.g. later waves escalate to Chakra).</summary>
        public void SetFormation(FormationKind kind)
        {
            formationKind = kind;
            _formation = FormationStrategy.For(kind);
        }

        // ── Internals ──────────────────────────────────────────────

        private IEnumerator RunWaves()
        {
            while (_running && _currentWave < _totalWaves)
            {
                _currentWave++;
                // Escalate spectacle: wave 1 Arc, wave 2 Vyuha, wave 3+ Chakra.
                if (_totalWaves >= 2)
                {
                    if (_currentWave == 1) SetFormation(FormationKind.Arc);
                    else if (_currentWave == 2) SetFormation(FormationKind.Vyuha);
                    else SetFormation(FormationKind.Chakra);
                }
                SpawnWave(_currentWave);
                onWaveStarted?.Invoke(_currentWave);

                while (AliveCount > 0) yield return null;

                onWaveCompleted?.Invoke(_currentWave);
                yield return new WaitForSeconds(secondsBetweenWaves);
            }
            if (_running) onAllWavesCompleted?.Invoke();
            _running = false;
        }

        private void SpawnWave(int waveIndex)
        {
            _alive.Clear();
            Vector3 origin = player != null ? player.position : transform.position;
            Vector3 forward = player != null ? player.forward : transform.forward;
            if (_formation == null) _formation = FormationStrategy.For(formationKind);
            Vector3[] points = _formation.SpawnPoints(rakshasasPerWave, origin, forward, spawnRadius);

            for (int i = 0; i < points.Length; i++)
            {
                var rakshasa = InstantiateRakshasa(points[i], waveIndex, i);
                if (rakshasa != null) _alive.Add(rakshasa);
            }
        }

        private RakshasaTarget InstantiateRakshasa(Vector3 position, int wave, int slot)
        {
            RakshasaTarget target;
            if (rakshasaPrefab != null)
            {
                target = Instantiate(rakshasaPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"Rakshasa_w{wave}_{slot}";
                go.transform.SetParent(transform, false);
                go.transform.position = position;
                target = go.AddComponent<RakshasaTarget>();
                target.Configure(3 + wave);
            }
            return target;
        }
    }
}
