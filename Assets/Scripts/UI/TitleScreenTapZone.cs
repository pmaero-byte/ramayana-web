// Round 13 (playable-without-reading) — Title screen tap handler.
// Fires OnTap() UnityEvent when the ConceptArtTapZone button is tapped.
// In this minimal version, tapping the bottom 60% of the title screen
// just logs a confirmation.  When the game's actual story scenes are
// added, this handler will load them via SceneNavigator.
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Jambudweep.Ramayana.UI
{
    public sealed class TitleScreenTapZone : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private string targetScene = "MainMenu";

        [Header("Events")]
        [SerializeField] private UnityEvent onTap;

        // Round 13: a tap anywhere on the lower 60% of the title screen
        // fires this event.  We intentionally don't require any keyboard
        // input or text reading — the player just taps the art.
        public void OnTap()
        {
            Debug.Log("[TitleScreen] Tap registered → " + targetScene);
            onTap?.Invoke();
            if (!string.IsNullOrEmpty(targetScene))
            {
                SceneManager.LoadScene(targetScene);
            }
        }
    }
}