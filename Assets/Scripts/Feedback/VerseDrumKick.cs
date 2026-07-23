// Round 26 — VerseDrumKick: small, brief camera shake on verse complete.
// Pure positional jitter, no transform parenting; restores the camera on stop.

using System.Collections;
using UnityEngine;

namespace Jambudweep.Ramayana.Feedback
{
    public static class VerseDrumKick
    {
        private static float _decay = 0f;
        private static float _magnitude = 0f;
        private static Vector3 _originPos;
        private static Coroutine _active;

        public static void Trigger(float strength = 0.06f, float duration = 0.35f)
        {
            if (_active != null && Camera.main != null) _active = null;
            if (Camera.main == null) return;
            if (_originPos == default) _originPos = Camera.main.transform.localPosition;
            _magnitude = Mathf.Max(_magnitude, strength);
            _decay = duration;
            if (_active == null)
            {
                var go = new GameObject("VerseDrumKickRunner");
                Object.DontDestroyOnLoad(go);
                _active = go.AddComponent<Runner>().StartCoroutine(Run());
            }
        }

        private static IEnumerator Run()
        {
            float t = _decay;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (Camera.main == null) yield break;
                Vector3 offset = Random.insideUnitSphere * (_magnitude * (t / _decay));
                Camera.main.transform.localPosition = _originPos + offset;
                yield return null;
            }
            if (Camera.main != null) Camera.main.transform.localPosition = _originPos;
            _active = null;
            _magnitude = 0f;
        }

        private class Runner : MonoBehaviour { }
    }
}
