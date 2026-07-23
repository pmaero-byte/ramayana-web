// Day 8 (RamayanaPS5) — MacDesktopInput.
// Laptop-first GTA-style orbit + run assist for Mac keyboards/trackpads.
// Pairs with CinematicThirdPersonCamera.AddOrbitInput + ThirdPersonMotionController.
//
// Controls (Mac laptop):
//   WASD / arrows  — move (handled by ThirdPersonMotionController)
//   Shift          — run (handled by motion controller)
//   Space          — jump
//   Right-mouse OR Option(Alt)+drag — orbit camera (GTA third-person peek)
//   Middle-mouse drag — orbit (trackpad users often map this)
//   Escape         — unlock cursor if locked

using UnityEngine;
using Jambudweep.Ramayana.Motion3D;

namespace Jambudweep.Ramayana.Platform
{
    public sealed class MacDesktopInput : MonoBehaviour
    {
        [Header("Refs (auto-resolved)")]
        [SerializeField] private CinematicThirdPersonCamera orbitCamera;
        [SerializeField] private ThirdPersonMotionController motion;

        [Header("Orbit feel")]
        [SerializeField, Min(0.1f)] private float mouseSensitivity = 1.35f;
        [SerializeField] private bool requireOrbitModifier = true;
        [SerializeField] private bool lockCursorOnStart = false;

        public static MacDesktopInput EnsureCreated()
        {
            var existing = FindFirstObjectByType<MacDesktopInput>();
            if (existing != null) return existing;
            var go = new GameObject("MacDesktopInput");
            return go.AddComponent<MacDesktopInput>();
        }

        void Awake()
        {
            if (orbitCamera == null) orbitCamera = FindFirstObjectByType<CinematicThirdPersonCamera>();
            if (motion == null) motion = FindFirstObjectByType<ThirdPersonMotionController>();
            if (lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (orbitCamera == null) return;

            bool orbitHeld =
                Input.GetMouseButton(1) ||
                Input.GetMouseButton(2) ||
                Input.GetKey(KeyCode.LeftAlt) ||
                Input.GetKey(KeyCode.RightAlt) ||
                !requireOrbitModifier;

            if (!orbitHeld) return;

            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            float pitch = -Input.GetAxis("Mouse Y") * mouseSensitivity;
            if (Mathf.Abs(yaw) < 0.0001f && Mathf.Abs(pitch) < 0.0001f) return;
            orbitCamera.AddOrbitInput(yaw, pitch);
        }

        public void Bind(CinematicThirdPersonCamera cam, ThirdPersonMotionController body)
        {
            orbitCamera = cam;
            motion = body;
        }
    }
}
