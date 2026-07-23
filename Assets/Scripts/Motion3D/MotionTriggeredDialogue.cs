using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Jambudweep.Ramayana.UI;

namespace Jambudweep.Ramayana.Motion3D
{
    /// <summary>
    /// Plays a single line of dialogue (text + optional portrait sprite + optional AudioClip)
    /// when the player walks this object's trigger collider. The player is NOT stopped — motion
    /// continues while the overlay fades in/out. This is the keystone of the motion-pivot:
    /// dialogue as a side-effect of motion, never a replacement for motion.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MotionTriggeredDialogue : MonoBehaviour
    {
        [Header("Speaker")]
        [SerializeField] private string speakerId = "Krishna";
        [SerializeField] private Sprite portrait;

        [Header("Line")]
        [TextArea(2, 6)]
        [SerializeField] private string dialogueLine =
            "Duryodhana, the Pandavas seek only what is rightly theirs. Five villages — that is all.";

        [TextArea(2, 4)]
        [SerializeField] private string sanskritSubtitle;

        [Header("Audio")]
        [SerializeField] private AudioClip voiceClip;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;

        [Header("Motion Behavior")]
        [Tooltip("If true, the player may be moving when this fires — the dialogue overlays motion.")]
        [SerializeField] private bool allowMotionDuringDialogue = true;

        [Tooltip("Trigger volume radius (set on the Collider).")]
        [SerializeField] private bool oneShot = true;

        [Header("UI references (optional)")]
        [SerializeField] private DialogueOverlay overlay;
        [SerializeField] private CanvasGroup fallbackGroup;

        [Header("Timing")]
        [SerializeField, Min(0.05f)] private float minimumDisplaySeconds = 2.4f;
        [SerializeField, Range(0.05f, 1.5f)] private float fadeInSeconds = 0.18f;
        [SerializeField, Range(0.05f, 1.5f)] private float fadeOutSeconds = 0.45f;

        private bool hasFired;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (oneShot && hasFired) return;
            if (!other.CompareTag("Player")) return;
            hasFired = true;
            StartCoroutine(PlayLine());
        }

        private IEnumerator PlayLine()
        {
            // Hold motion if the script is configured to gate input.
            ThirdPersonMotionController playerController =
                FindFirstObjectByType<ThirdPersonMotionController>();
            if (playerController != null && !allowMotionDuringDialogue)
            {
                playerController.LockControls();
            }

            float totalSeconds = voiceClip != null ? voiceClip.length : Mathf.Max(minimumDisplaySeconds, dialogueLine.Length * 0.045f);

            // Fire overlay fade-in + VO.
            if (overlay != null)
            {
                overlay.Show(speakerId, dialogueLine, sanskritSubtitle, portrait, totalSeconds);
            }
            else if (fallbackGroup != null)
            {
                fallbackGroup.alpha = 0f;
                Text fallbackText = fallbackGroup.GetComponentInChildren<Text>();
                if (fallbackText != null)
                {
                    fallbackText.text = $"{speakerId}: {dialogueLine}";
                }
                float t = 0f;
                while (t < fadeInSeconds)
                {
                    t += Time.deltaTime;
                    fallbackGroup.alpha = Mathf.Clamp01(t / fadeInSeconds);
                    yield return null;
                }
            }

            if (voiceClip != null)
            {
                AudioSource.PlayClipAtPoint(voiceClip, transform.position, voiceVolume);
            }

            yield return new WaitForSeconds(totalSeconds);

            // Fade out.
            if (overlay != null)
            {
                overlay.Hide();
            }
            else if (fallbackGroup != null)
            {
                float t = 0f;
                while (t < fadeOutSeconds)
                {
                    t += Time.deltaTime;
                    fallbackGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutSeconds));
                    yield return null;
                }
                fallbackGroup.alpha = 0f;
            }

            if (playerController != null && !allowMotionDuringDialogue)
            {
                playerController.UnlockControlsPublic();
            }
        }

        // Public configure method (used by RamayanaMotionWiring scaffolder)
        public void Configure(string newId, string newText, string newSpeaker, float radius)
        {
            var t = GetType();
            var fId = t.GetField("verseId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (fId != null) fId.SetValue(this, newId);
            var fText = t.GetField("text", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (fText != null) fText.SetValue(this, newText);
            var fSpeaker = t.GetField("speaker", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (fSpeaker != null) fSpeaker.SetValue(this, newSpeaker);
            var fRadius = t.GetField("triggerRadius", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (fRadius != null) fRadius.SetValue(this, radius);
        }
    }
}
