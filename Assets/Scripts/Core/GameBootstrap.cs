// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Game Bootstrap (Entry Point)
// C# port of `GameBootstrap.cs` from Cīvaka Cintāmaṇi (PS5 reference)
// Reference: civaka-cintamani-ps5/unity/Assets/Scripts/Core/GameBootstrap.cs
// ════════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.SceneManagement;
using Jambudweep.Ramayana.Core;
using Jambudweep.Ramayana.Story;
using Jambudweep.Ramayana.Data;

namespace Jambudweep.Ramayana
{
    /// <summary>
    /// Entry point. On Awake: registers platform shims, attempts resume from save.
    /// On Start: kicks off TitleScreen or direct-to-game if user has a recent save.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Episode")]
        public Data.EpisodeData episode;

        [Header("Wiring (auto-resolved if null)")]
        public StoryEngine storyEngine;
        public Audio.RagaAudioEngine audio;

        [Header("Scene order")]
        [Tooltip("Scene to load on first frame when no save exists.")]
        [SerializeField] private string titleSceneName = "TitleScreen";

        [Tooltip("Scene to load when a recent save exists.")]
        [SerializeField] private string resumeSceneName = "MainMenu";

        void Awake()
        {
            // Two-flag init guard (pop-grade-game-polish §10)
            if (s_initialized) return;
            s_initialized = true;

            // Configure Tempest 3D Audio (PS5)
            Platform.TempestAudio.ConfigureTempest();

            // Resolve scene refs
            if (storyEngine == null) storyEngine = FindObjectOfType<StoryEngine>();
            if (audio == null) audio = FindObjectOfType<Audio.RagaAudioEngine>();
        }

        void Start()
        {
            // Defer to title screen or resume flow after 100ms (Cīvaka pattern)
            Invoke(nameof(OnFirstFrame), 0.1f);
        }

        private void OnFirstFrame()
        {
            var mostRecent = SaveSystem.GetMostRecentSave();
            if (mostRecent != null && !string.IsNullOrEmpty(mostRecent.selectedCharacterId))
            {
                Debug.Log($"[Bootstrap] Resuming save: act={mostRecent.currentActId}, char={mostRecent.selectedCharacterId}");
                SceneManager.LoadScene(resumeSceneName);
            }
            else
            {
                Debug.Log("[Bootstrap] No save found → TitleScreen scene");
                SceneManager.LoadScene(titleSceneName);
            }
        }

        private static bool s_initialized;
    }
}
