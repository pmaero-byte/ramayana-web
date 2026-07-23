// Day 8 (RamayanaPS5) — MacGtaFeelDirector.
// One-shot ambient polish for the Mac laptop vertical slice: dusk fog,
// sky tint, stronger sun, and optional post-feel ground recolor.
// KISS — no URP volume profiles required (those are editor-authored).

using UnityEngine;

namespace Jambudweep.Ramayana.Scene
{
    public sealed class MacGtaFeelDirector : MonoBehaviour
    {
        [Header("Lanka dusk palette")]
        [SerializeField] private Color skyColor = new Color(0.08f, 0.06f, 0.12f, 1f);
        [SerializeField] private Color fogColor = new Color(0.18f, 0.10f, 0.08f, 1f);
        [SerializeField, Min(20f)] private float fogEnd = 85f;
        [SerializeField, Min(0.5f)] private float sunIntensity = 1.35f;

        public static MacGtaFeelDirector EnsureCreated()
        {
            var existing = FindFirstObjectByType<MacGtaFeelDirector>();
            if (existing != null) return existing;
            var go = new GameObject("MacGtaFeelDirector");
            return go.AddComponent<MacGtaFeelDirector>();
        }

        void Start()
        {
            Apply();
        }

        public void Apply()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = fogEnd;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.16f, 0.14f, 1f);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = skyColor;
                cam.farClipPlane = Mathf.Max(cam.farClipPlane, 200f);
                cam.fieldOfView = 55f; // slightly cinematic vs default 60
            }

            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l == null || l.type != LightType.Directional) continue;
                l.intensity = sunIntensity;
                l.color = new Color(1f, 0.88f, 0.72f, 1f);
                l.shadows = LightShadows.Soft;
            }
        }
    }
}
