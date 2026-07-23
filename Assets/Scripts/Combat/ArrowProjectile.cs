// Day 7 (RamayanaPS5) — ArrowProjectile.
// Kinematic visible arrow. ArcherAutoFire spawns these instead of applying
// instant Damage(). Pure-KISS: no raycast / spherecast API —
// position += direction * speed * deltaTime, then either OnTriggerEnter
// or a proximity check against RakshasaTarget.FindAllActive() applies damage
// and recycles the arrow.
//
// Style matches RakshasaTarget / WaveController: sealed MonoBehaviour,
// runtime CreatePrimitive fallback, no inspector wiring required.

using UnityEngine;

namespace Jambudweep.Ramayana.Combat
{
    public sealed class ArrowProjectile : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField, Min(0.5f)] private float defaultSpeed = 18f;
        [SerializeField, Min(0.1f)] private float defaultLifetime = 2.5f;
        [SerializeField, Min(0.05f)] private float hitRadius = 0.55f;

        private Vector3 _direction = Vector3.forward;
        private float _speed;
        private int _damage = 1;
        private float _lifetime;
        private bool _spent;
        private Transform _owner;

        public bool IsSpent => _spent;
        public int Damage => _damage;

        void Awake()
        {
            // Default visual if spawned bare (no prefab mesh).
            if (GetComponentInChildren<Renderer>() == null)
            {
                // Cylinder is a decent thin shaft; rotate so local Y points along flight.
                // Caller can override with a prefab.
            }
        }

        /// <summary>
        /// Arm the arrow and begin flight. Call once after Instantiate.
        /// </summary>
        public void Initialize(Vector3 origin, Vector3 direction, float speed, int damage, float maxLifetime)
        {
            transform.position = origin;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            _speed = speed > 0f ? speed : defaultSpeed;
            _damage = Mathf.Max(1, damage);
            _lifetime = maxLifetime > 0f ? maxLifetime : defaultLifetime;
            _spent = false;
            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
        }

        /// <summary>Optional: ignore self-collision with the archer's own collider tree.</summary>
        public void SetOwner(Transform owner) => _owner = owner;

        void Update()
        {
            if (_spent) return;

            transform.position += _direction * (_speed * Time.deltaTime);
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                Recycle();
                return;
            }

            // KISS proximity hit — no engine raycast API. Works for capsule
            // rakshasas even when triggers never fire (no Rigidbody).
            TryProximityHit();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_spent || other == null) return;
            if (_owner != null && other.transform.IsChildOf(_owner)) return;
            var target = other.GetComponentInParent<RakshasaTarget>();
            if (target == null || target.IsDead) return;
            ApplyHit(target);
        }

        // ── Internals ──────────────────────────────────────────────

        private void TryProximityHit()
        {
            float r2 = hitRadius * hitRadius;
            Vector3 pos = transform.position;
            foreach (var r in RakshasaTarget.FindAllActive())
            {
                if (r == null || r.IsDead) continue;
                if ((r.transform.position - pos).sqrMagnitude <= r2)
                {
                    ApplyHit(r);
                    return;
                }
            }
        }

        private void ApplyHit(RakshasaTarget target)
        {
            if (_spent || target == null) return;
            target.Damage(_damage);
            Recycle();
        }

        private void Recycle()
        {
            if (_spent) return;
            _spent = true;
            Destroy(gameObject);
        }

        // ── Factory ────────────────────────────────────────────────

        /// <summary>
        /// Build a thin cylinder arrow at runtime when no prefab is wired.
        /// </summary>
        public static ArrowProjectile CreateProcedural(Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "ArrowProjectile";
            if (parent != null) go.transform.SetParent(parent, false);
            // Default capsule/cylinder is tall on Y; scale thin and rotate so
            // local Z (LookRotation forward) is the flight axis after Initialize.
            go.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
            // Cylinder mesh is Y-up; rotate 90° so the shaft lies along local Z.
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Soft gold/bronze shaft colour so it's visible on dark Lanka ground.
            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = rend.material;
                if (mat != null) mat.color = new Color(0.85f, 0.72f, 0.35f, 1f);
            }

            var arrow = go.AddComponent<ArrowProjectile>();
            return arrow;
        }
    }
}
