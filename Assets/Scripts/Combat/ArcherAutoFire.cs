// Day 3 + Day 7 (RamayanaPS5) — ArcherAutoFire.
// Player-side auto-archer that scans forward for the nearest live
// RakshasaTarget on a fire interval and spawns a kinematic ArrowProjectile.
// Day 3 applied Damage() instantly; Day 7 plumbs a real visible arrow.
//
// Mirrors the existing motion-pivot rule: motion (walking, strafing)
// remains the primary input. The archer simply helps when the player
// has cleared the path to an enemy.

using UnityEngine;

namespace Jambudweep.Ramayana.Combat
{
    public sealed class ArcherAutoFire : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField, Min(0.5f)] private float range = 12f;
        [SerializeField, Range(0f, 180f)] private float coneAngleDegrees = 60f;

        [Header("Fire rate")]
        [SerializeField, Min(0.05f)] private float fireInterval = 0.6f;
        [SerializeField, Min(1)] private int damagePerShot = 1;

        [Header("Projectile")]
        [Tooltip("Optional arrow prefab. If null, a procedural Cylinder is used.")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField, Min(1f)] private float arrowSpeed = 18f;
        [SerializeField, Min(0.1f)] private float arrowLifetime = 2.5f;

        [Header("Optional refs")]
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private BowCooldown bowCooldown;

        public float Range => range;
        public int DamagePerShot => damagePerShot;
        public float FireInterval => fireInterval;

        void Awake()
        {
            if (aimOrigin == null) aimOrigin = transform;
            if (bowCooldown == null) bowCooldown = GetComponent<BowCooldown>();
            if (bowCooldown == null) bowCooldown = gameObject.AddComponent<BowCooldown>();
            bowCooldown.Interval = fireInterval;
        }

        void Update()
        {
            if (bowCooldown == null || !bowCooldown.CanFire()) return;
            var target = FindTarget();
            if (target == null) return;
            FireAt(target);
            bowCooldown.Consume(fireInterval);
        }

        // ── Internals ──────────────────────────────────────────────

        private void FireAt(RakshasaTarget target)
        {
            Vector3 origin = aimOrigin.position + Vector3.up * 1.1f;
            Vector3 to = target.transform.position + Vector3.up * 0.9f - origin;
            Vector3 dir = to.sqrMagnitude > 0.0001f ? to.normalized : aimOrigin.forward;

            ArrowProjectile arrow;
            if (arrowPrefab != null)
            {
                var go = Instantiate(arrowPrefab, origin, Quaternion.LookRotation(dir));
                arrow = go.GetComponent<ArrowProjectile>();
                if (arrow == null) arrow = go.AddComponent<ArrowProjectile>();
            }
            else
            {
                arrow = ArrowProjectile.CreateProcedural(null);
                arrow.transform.position = origin;
            }

            arrow.SetOwner(transform);
            arrow.Initialize(origin, dir, arrowSpeed, damagePerShot, arrowLifetime);
        }

        private RakshasaTarget FindTarget()
        {
            RakshasaTarget best = null;
            float bestSqr = range * range;
            Vector3 origin = aimOrigin.position;
            Vector3 forward = aimOrigin.forward;
            float cosLimit = Mathf.Cos(coneAngleDegrees * 0.5f * Mathf.Deg2Rad);

            foreach (var r in RakshasaTarget.FindAllActive())
            {
                Vector3 to = r.transform.position - origin;
                float sqr = to.sqrMagnitude;
                if (sqr > bestSqr) continue;
                Vector3 dir = to.normalized;
                if (Vector3.Dot(forward, dir) < cosLimit) continue;
                bestSqr = sqr;
                best = r;
            }
            return best;
        }
    }
}
