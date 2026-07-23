using UnityEngine;

namespace Jambudweep.Ramayana.Motion3D
{
    public sealed class CinematicThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 2.35f, -7.2f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.15f, 0f);
        [SerializeField] private float positionSmooth = 5.5f;
        [SerializeField] private float rotationSmooth = 7f;
        [SerializeField] private float recenterDelay = 1.4f;
        [SerializeField] private float recenterSpeed = 2.25f;
        [SerializeField] private float orbitSensitivity = 1f;
        [SerializeField] private float collisionRadius = 0.24f;
        [SerializeField] private float minimumFollowDistance = 3.8f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float orbitYaw;
        [SerializeField] private float orbitPitch;

        private bool cinematicLocked;
        private Vector3 cinematicPosition;
        private Vector3 cinematicLookAt;
        private float lastOrbitInputTime;
        private Vector3 currentVelocity;
        private readonly RaycastHit[] collisionHits = new RaycastHit[8];

        private void Start()
        {
            if (target == null) return;
            Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 desiredPosition = target.position + orbitRotation * followOffset;
            Vector3 desiredLookAt = target.position + lookOffset;
            transform.position = EnforceMinimumDistance(desiredLookAt, desiredPosition);
            transform.rotation = Quaternion.LookRotation(desiredLookAt - transform.position, Vector3.up);
        }

        private void LateUpdate()
        {
            if (cinematicLocked)
            {
                MoveCamera(cinematicPosition, cinematicLookAt);
                return;
            }

            if (target == null) return;
            if (Time.time - lastOrbitInputTime > recenterDelay)
            {
                orbitYaw = Mathf.Lerp(orbitYaw, 0f, Time.deltaTime * recenterSpeed);
                orbitPitch = Mathf.Lerp(orbitPitch, 0f, Time.deltaTime * recenterSpeed * 0.55f);
            }

            Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 desiredPosition = target.position + orbitRotation * followOffset;
            Vector3 desiredLookAt = target.position + lookOffset;
            desiredPosition = ResolveCollision(desiredLookAt, desiredPosition);
            desiredPosition = EnforceMinimumDistance(desiredLookAt, desiredPosition);
            MoveCamera(desiredPosition, desiredLookAt);
        }

        public void Focus(Vector3 position, Vector3 lookAt)
        {
            cinematicLocked = true;
            cinematicPosition = position;
            cinematicLookAt = lookAt;
        }

        public void AddOrbitInput(float yawDelta, float pitchDelta)
        {
            if (cinematicLocked) return;
            lastOrbitInputTime = Time.time;
            orbitYaw = Mathf.Clamp(orbitYaw + yawDelta * orbitSensitivity, -42f, 42f);
            orbitPitch = Mathf.Clamp(orbitPitch + pitchDelta * orbitSensitivity, -7f, 18f);
        }

        public void SetOrbitSensitivity(float value)
        {
            orbitSensitivity = Mathf.Clamp(value, 0.35f, 1.85f);
        }

        public void ReleaseFocus()
        {
            cinematicLocked = false;
            lastOrbitInputTime = Time.time;
        }

        private Vector3 ResolveCollision(Vector3 lookAt, Vector3 desiredPosition)
        {
            Vector3 toCamera = desiredPosition - lookAt;
            float distance = toCamera.magnitude;
            if (distance <= 0.01f) return desiredPosition;

            int hitCount = Physics.SphereCastNonAlloc(lookAt, collisionRadius, toCamera.normalized, collisionHits, distance, collisionMask, QueryTriggerInteraction.Ignore);
            float nearestDistance = float.MaxValue;
            for (int index = 0; index < hitCount; index += 1)
            {
                RaycastHit hit = collisionHits[index];
                if (target != null && hit.transform.root == target.root) continue;
                if (hit.distance >= nearestDistance) continue;
                nearestDistance = hit.distance;
            }

            if (nearestDistance < float.MaxValue)
            {
                return lookAt + toCamera.normalized * Mathf.Max(0.65f, nearestDistance - collisionRadius);
            }

            return desiredPosition;
        }

        private Vector3 EnforceMinimumDistance(Vector3 lookAt, Vector3 desiredPosition)
        {
            Vector3 toCamera = desiredPosition - lookAt;
            float distance = toCamera.magnitude;
            if (distance >= minimumFollowDistance || distance <= 0.01f) return desiredPosition;
            return lookAt + toCamera.normalized * minimumFollowDistance;
        }

        private void MoveCamera(Vector3 desiredPosition, Vector3 desiredLookAt)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / Mathf.Max(0.01f, positionSmooth));
            Quaternion desiredRotation = Quaternion.LookRotation(desiredLookAt - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmooth);
        }

        // Public setter used by Ramayana scaffolder
        public void SetTarget(Transform t)
        {
            var f = typeof(CinematicThirdPersonCamera).GetField("target",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (f != null) f.SetValue(this, t);
        }
    }
}
