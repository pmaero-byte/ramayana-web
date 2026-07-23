// Day 7 (RamayanaPS5) — BowCooldown.
// Tiny module ArcherAutoFire delegates to for fire-rate gating.
// Exposes CanFire() + Consume() so cooldown can be stubbed / tuned
// without touching the auto-fire scan loop.
//
// Style: sealed MonoBehaviour, no events, no inspector wiring required.

using UnityEngine;

namespace Jambudweep.Ramayana.Combat
{
    public sealed class BowCooldown : MonoBehaviour
    {
        [Header("Cooldown")]
        [SerializeField, Min(0.05f)] private float interval = 0.6f;

        private float _remaining;

        public float Interval
        {
            get => interval;
            set => interval = Mathf.Max(0.05f, value);
        }

        public float Remaining => Mathf.Max(0f, _remaining);
        public bool IsReady => _remaining <= 0f;

        public bool CanFire() => _remaining <= 0f;

        public void Consume()
        {
            _remaining = interval;
        }

        public void Consume(float customInterval)
        {
            _remaining = Mathf.Max(0.05f, customInterval);
        }

        public void ResetCooldown()
        {
            _remaining = 0f;
        }

        void Update()
        {
            if (_remaining > 0f)
                _remaining -= Time.deltaTime;
        }
    }
}
