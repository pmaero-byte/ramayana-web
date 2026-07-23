// Day 10 (RamayanaPS5) — FormationStrategy.
// Pluggable spawn layouts for WaveController. Pure-KISS C# (no MonoBehaviour):
// Arc (Day 3 default), Chakra (ring), Vyuha (3-row infantry).
// GTA combat spectacle: rakshasas appear in intentional formations, not a blob.

using UnityEngine;

namespace Jambudweep.Ramayana.Combat
{
    public enum FormationKind
    {
        Arc = 0,
        Chakra = 1,
        Vyuha = 2,
    }

    public abstract class FormationStrategy
    {
        /// <summary>World-space spawn positions for one wave.</summary>
        public abstract Vector3[] SpawnPoints(int waveSize, Vector3 playerPos, Vector3 playerForward, float radius);

        public static FormationStrategy For(FormationKind kind)
        {
            switch (kind)
            {
                case FormationKind.Chakra: return new ChakraFormation();
                case FormationKind.Vyuha: return new VyuhaFormation();
                default: return new ArcFormation();
            }
        }

        protected static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            if (v.sqrMagnitude < 0.0001f) return Vector3.forward;
            return v.normalized;
        }
    }

    /// <summary>~117° arc in front of the player (Day 3 default).</summary>
    public sealed class ArcFormation : FormationStrategy
    {
        public override Vector3[] SpawnPoints(int waveSize, Vector3 playerPos, Vector3 playerForward, float radius)
        {
            int n = Mathf.Max(1, waveSize);
            var pts = new Vector3[n];
            Vector3 forward = Flatten(playerForward);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

            float arc = Mathf.PI * 0.65f;
            for (int i = 0; i < n; i++)
            {
                float t = n <= 1 ? 0.5f : (float)i / (n - 1);
                float angle = Mathf.Lerp(-arc * 0.5f, arc * 0.5f, t);
                Vector3 local = forward * (Mathf.Cos(angle) * radius) + right * (Mathf.Sin(angle) * radius);
                pts[i] = playerPos + local;
            }
            return pts;
        }
    }

    /// <summary>Ring (cakra) around the player — full surround pressure.</summary>
    public sealed class ChakraFormation : FormationStrategy
    {
        public override Vector3[] SpawnPoints(int waveSize, Vector3 playerPos, Vector3 playerForward, float radius)
        {
            int n = Mathf.Max(1, waveSize);
            var pts = new Vector3[n];
            float start = Mathf.Atan2(playerForward.x, playerForward.z);
            for (int i = 0; i < n; i++)
            {
                float a = start + (Mathf.PI * 2f * i / n);
                pts[i] = playerPos + new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius);
            }
            return pts;
        }
    }

    /// <summary>Three-row infantry (vyūha): front rank dense, rear ranks wider.</summary>
    public sealed class VyuhaFormation : FormationStrategy
    {
        public override Vector3[] SpawnPoints(int waveSize, Vector3 playerPos, Vector3 playerForward, float radius)
        {
            int n = Mathf.Max(1, waveSize);
            var pts = new Vector3[n];
            Vector3 forward = Flatten(playerForward);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

            int row0 = Mathf.Max(1, (n + 2) / 3);
            int row1 = Mathf.Max(1, (n - row0 + 1) / 2);
            int row2 = Mathf.Max(0, n - row0 - row1);
            int[] rows = { row0, row1, row2 };
            float[] depths = { radius * 0.85f, radius * 1.15f, radius * 1.45f };
            float[] spreads = { radius * 0.55f, radius * 0.85f, radius * 1.15f };

            int idx = 0;
            for (int r = 0; r < 3 && idx < n; r++)
            {
                int count = rows[r];
                if (count <= 0) continue;
                for (int i = 0; i < count && idx < n; i++, idx++)
                {
                    float t = count <= 1 ? 0.5f : (float)i / (count - 1);
                    float x = Mathf.Lerp(-spreads[r], spreads[r], t);
                    pts[idx] = playerPos + forward * depths[r] + right * x;
                }
            }
            while (idx < n)
            {
                pts[idx] = playerPos + forward * radius + right * ((idx - n * 0.5f) * 0.8f);
                idx++;
            }
            return pts;
        }
    }
}
