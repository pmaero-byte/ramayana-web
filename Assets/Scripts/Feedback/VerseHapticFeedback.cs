// Round 25 — VerseHapticFeedback: cross-platform haptics for verse success/fail feedback.
// Uses Unity's built-in Handheld.Vibrate on Android, and a P/Invoke / native bridge for iOS.
// Falls back to a screen flash on desktop.

using UnityEngine;

namespace Jambudweep.Ramayana.Feedback
{
    public enum FeedbackKind { Light, Medium, Heavy, Success, Warning, Error }

    public static class VerseHapticFeedback
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _rama_impact(int style);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _rama_notify(int kind);

        // iOS UIImpactFeedbackStyle: light=0, medium=1, heavy=2, soft=3, rigid=4
        // UINotificationFeedbackType: success=0, warning=1, error=2
        public static void Trigger(FeedbackKind kind)
        {
            switch (kind)
            {
                case FeedbackKind.Light:    _rama_impact(0); break;
                case FeedbackKind.Medium:   _rama_impact(1); break;
                case FeedbackKind.Heavy:    _rama_impact(2); break;
                case FeedbackKind.Success:  _rama_notify(0); _rama_impact(2); break;
                case FeedbackKind.Warning:  _rama_notify(1); _rama_impact(1); break;
                case FeedbackKind.Error:    _rama_notify(2); break;
            }
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static void Trigger(FeedbackKind kind)
        {
            // Unity's built-in vibration is the simplest cross-version path.
            int ms = kind switch
            {
                FeedbackKind.Light => 10,
                FeedbackKind.Medium => 20,
                FeedbackKind.Heavy => 35,
                FeedbackKind.Success => 25,
                FeedbackKind.Warning => 30,
                FeedbackKind.Error => 45,
                _ => 15
            };
            Handheld.Vibrate();
            VibrationExtra(ms);
        }

        private static void VibrationExtra(int ms)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", (long)ms);
            }
            catch { /* ignore */ }
        }
#else
        public static void Trigger(FeedbackKind kind)
        {
            // Editor / desktop: no-op (we use visual feedback instead).
        }
#endif

        public static void ScreenFlash(Color color, float durationSeconds = 0.4f)
        {
            // Round 25 — overlays a brief color tint on the camera via VerseFlashOverlay singleton.
            if (VerseFlashOverlay.Instance == null) return;
            VerseFlashOverlay.Instance.Flash(color, durationSeconds);
        }
    }
}
