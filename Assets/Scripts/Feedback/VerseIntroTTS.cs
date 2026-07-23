// Round 28 — VerseIntroTTS: brief spoken intro for each verse using AVSpeechSynthesizer on iOS.
// Speaks "Day X. Parva name. Action." then voice cue plays. Native bridge.

using UnityEngine;

namespace Jambudweep.Ramayana.Feedback
{
    public static class VerseIntroTTS
    {
        public static void Speak(int day, string parva, string verb)
        {
            string text = BuildText(day, parva, verb);
            if (string.IsNullOrEmpty(text)) return;
#if UNITY_IOS && !UNITY_EDITOR
            _rama_intro_speak(text);
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                    using var tts = activity.Call<AndroidJavaObject>("getSystemService", "text_to_speech");
                }));
            } catch { }
#else
            // Editor: log only
            Debug.Log($"[VerseIntroTTS] {text}");
#endif
        }

        private static string BuildText(int day, string parva, string verb)
        {
            string s = "";
            if (day > 0) s += "Day " + day + ". ";
            if (!string.IsNullOrEmpty(parva)) s += parva + ". ";
            if (!string.IsNullOrEmpty(verb)) s += verb + ".";
            return s;
        }

#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _rama_intro_speak(string text);
#endif
    }
}

