// Day 3 (RamayanaPS5) — RakshasaTarget.
// Simple damageable enemy used by WaveController. Lives in the 3D world,
// exposes HP + a `Damage(int)` API. On death, fires `onDeath` and notifies
// the WaveController that spawned it so the wave can advance.
//
// Style: MonoBehaviour, runtime-instantiable, no inspector wiring required.
// Matches the existing ThirdPersonMotionController / DialogueOverlay pattern.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Jambudweep.Ramayana.Combat
{
    public sealed class RakshasaTarget : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string rakshasaId = "rakshasa";

        [Header("Combat")]
        [SerializeField, Min(1)] private int maxHp = 3;

        [Header("Events")]
        public UnityEvent onDeath = new UnityEvent();
        public UnityEvent<int, int> onDamaged = new UnityEvent<int, int>(); // (currentHp, maxHp)

        private int _currentHp;
        private bool _dead;

        public int CurrentHp => _currentHp;
        public int MaxHp => maxHp;
        public bool IsDead => _dead;
        public string RakshasaId => rakshasaId;

        // ── Static registry ────────────────────────────────────────
        // WaveController is the only producer. ArcherAutoFire is the only
        // consumer. KISS: scan via FindObjectsByType, cache by skipping dead.

        private static readonly System.Collections.Generic.List<RakshasaTarget> s_activeScratch =
            new System.Collections.Generic.List<RakshasaTarget>(64);

        public static System.Collections.Generic.IEnumerable<RakshasaTarget> FindAllActive()
        {
            s_activeScratch.Clear();
            var all = FindObjectsByType<RakshasaTarget>(FindObjectsSortMode.None);
            foreach (var r in all)
            {
                if (r != null && !r.IsDead) s_activeScratch.Add(r);
            }
            return s_activeScratch;
        }

        void Awake()
        {
            _currentHp = maxHp;
        }

        public void Damage(int amount)
        {
            if (_dead || amount <= 0) return;
            _currentHp = Mathf.Max(0, _currentHp - amount);
            onDamaged?.Invoke(_currentHp, maxHp);
            if (_currentHp == 0) Die();
        }

        public void Configure(int hp)
        {
            maxHp = Mathf.Max(1, hp);
            _currentHp = maxHp;
            _dead = false;
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;
            onDeath?.Invoke();
            // Stay in scene so the WaveController can detect the death event.
            // Disable collider + visual for clarity, but keep the GameObject so
            // a future "death FX" prefab can be swapped in without restructuring.
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
    }
}
