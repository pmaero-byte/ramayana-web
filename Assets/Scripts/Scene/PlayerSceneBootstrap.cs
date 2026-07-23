// Day 6 + Day 8 (RamayanaPS5) — PlayerSceneBootstrap.
// Single-component scene root that wires Day 1-7 systems + Mac GTA feel
// at scene load. Drop this MonoBehaviour on any GameObject in a kanda
// scene (e.g. YuddhaKanda.unity) and a Mac laptop player gets:
//   - third-person capsule + CharacterController
//   - cinematic follow camera + mouse orbit (MacDesktopInput)
//   - ArcherAutoFire with kinematic ArrowProjectile
//   - StoryMomentPlayer + WaveController + HUD + Save/Load
//   - GTA-style letterbox + Lanka dusk ambient (MacGtaFeelDirector)
//
// Pattern matches RamayanaHudBootstrap / GameBootstrap:
//   - Idempotent EnsureX() calls construct missing singletons
//   - All events are bound in Awake()
//   - Start() kicks off gameplay after one frame

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Jambudweep.Ramayana.Story;
using Jambudweep.Ramayana.Verse;
using Jambudweep.Ramayana.Combat;
using Jambudweep.Ramayana.Feedback;
using Jambudweep.Ramayana.UI;
using Jambudweep.Ramayana.Platform;
using Jambudweep.Ramayana.Motion3D;

namespace Jambudweep.Ramayana.Scene
{
    public sealed class PlayerSceneBootstrap : MonoBehaviour
    {
        [Header("Boot options")]
        [Tooltip("If non-empty, StoryMomentPlayer.LoadAct(actId) is called in Start().")]
        [SerializeField] private string autoLoadActId = "yuddhakanda-war";

        [Tooltip("If >0, WaveController.StartWaves(totalWaves) is called in Start().")]
        [SerializeField, Min(0)] private int autoStartWaves = 3;

        [Tooltip("If true, the SaveLoadHud is mounted so players can save/load.")]
        [SerializeField] private bool mountSaveLoadHud = true;

        [Tooltip("If true, a default lighting + ground plane is created when missing.")]
        [SerializeField] private bool ensureMinimalScene = true;

        [Header("Mac GTA feel (Day 8)")]
        [SerializeField] private bool mountCinematicCamera = true;
        [SerializeField] private bool mountArcherAutoFire = true;
        [SerializeField] private bool mountLetterbox = true;
        [SerializeField] private bool mountMacDesktopInput = true;
        [SerializeField] private bool mountGtaFeelDirector = true;

        [Header("Events")]
        public UnityEvent onPlayerReady = new UnityEvent();
        public UnityEvent onSceneBootstrapped = new UnityEvent();

        // Resolved at Awake.
        private StoryMomentPlayer _momentPlayer;
        private WaveController _waveController;
        private HudOrchestrator _hudOrchestrator;
        private SaveLoadHud _saveLoadHud;
        private ThirdPersonMotionController _motionController;
        private CinematicThirdPersonCamera _cinematicCamera;
        private ArcherAutoFire _archer;
        private MacDesktopInput _macInput;
        private CinematicLetterbox _letterbox;
        private MacGtaFeelDirector _feelDirector;

        void Awake()
        {
            EnsureScene();
            EnsureComponents();
            BindEvents();
            onSceneBootstrapped?.Invoke();
        }

        void Start()
        {
            // Day 36 — consume cross-scene kanda selection if any.
            string pendingKanda = KandaLaunchBridge.ConsumePending();
            if (!string.IsNullOrEmpty(pendingKanda))
            {
                autoLoadActId = pendingKanda;
            }

            // Canonical moment+audio+combat path: VerseOrchestrator.
            // StoryMomentPlayer is intentionally NOT called here to avoid
            // double-driving the same cue loop.
            if (!string.IsNullOrEmpty(autoLoadActId))
            {
                if (VerseOrchestrator.Instance != null)
                {
                    string kandaId = autoLoadActId;
                    var map = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
                    {
                        { "bala-birth", "bala-kanda" },
                        { "ayodhya-dharma", "ayodhya-kanda" },
                        { "panchavati-golden-deer", "aranya-kanda" },
                        { "kishkindha-alliance", "kishkindha-kanda" },
                        { "sundarakanda-leap", "sundara-kanda" },
                        { "yuddhakanda-war", "yuddha-kanda" },
                        { "return-ayodhya", "uttara-kanda" },
                        { "uttara-earth-return", "return-kanda" }
                    };
                    if (map.TryGetValue(kandaId, out var resolved))
                        kandaId = resolved;
                    StartCoroutine(VerseOrchestrator.Instance.LoadAndPlay(kandaId));
                    VerseCombatTrigger.Instance?.ResetExhausted();
                }
                else
                {
                    Debug.LogError("[PlayerSceneBootstrap] VerseOrchestrator missing — cannot start moment loop.");
                }
            }
            if (autoStartWaves > 0 && _waveController != null)
            {
                _waveController.StartWaves(autoStartWaves);
            }
            _feelDirector?.Apply();
            onPlayerReady?.Invoke();
        }

        // ── Construction helpers ──────────────────────────────────

        private void EnsureScene()
        {
            if (!ensureMinimalScene) return;
            if (GameObject.Find("[SceneRoot]/Ground") == null &&
                GameObject.Find("Ground") == null)
            {
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.localScale = new Vector3(28f, 1f, 28f);
                ground.transform.SetParent(transform, false);
                ground.GetComponent<Renderer>().material.color = new Color(0.20f, 0.12f, 0.06f, 1f);
            }
            if (FindFirstObjectByType<Light>() == null)
            {
                var lightGO = new GameObject("SunLight");
                lightGO.transform.SetParent(transform, false);
                lightGO.transform.rotation = Quaternion.Euler(42f, 35f, 0f);
                var l = lightGO.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.35f;
                l.color = new Color(1f, 0.90f, 0.75f, 1f);
                l.shadows = LightShadows.Soft;
            }
        }

        private void EnsureComponents()
        {
            _momentPlayer = FindFirstObjectByType<StoryMomentPlayer>();
            _waveController = FindFirstObjectByType<WaveController>();
            _motionController = FindFirstObjectByType<ThirdPersonMotionController>();
            _hudOrchestrator = FindFirstObjectByType<HudOrchestrator>();
            _saveLoadHud = FindFirstObjectByType<SaveLoadHud>();
            _cinematicCamera = FindFirstObjectByType<CinematicThirdPersonCamera>();
            _archer = FindFirstObjectByType<ArcherAutoFire>();

            // Lazy-create the player capsule if motion is missing.
            if (_motionController == null)
            {
                var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerGO.name = "Player";
                playerGO.transform.SetParent(transform, false);
                playerGO.transform.position = new Vector3(0f, 1f, 0f);
                playerGO.GetComponent<Renderer>().material.color = new Color(0.95f, 0.78f, 0.42f, 1f);
                // RequireComponent adds CharacterController; strip default CapsuleCollider conflict.
                var col = playerGO.GetComponent<CapsuleCollider>();
                if (col != null) Destroy(col);
                _motionController = playerGO.AddComponent<ThirdPersonMotionController>();
            }

            if (_momentPlayer == null)
                _momentPlayer = gameObject.AddComponent<StoryMomentPlayer>();
            if (_waveController == null)
                _waveController = gameObject.AddComponent<WaveController>();

            EnsureGtaCameraAndCombat();

            HudOrchestrator.EnsureCreated();
            _hudOrchestrator = HudOrchestrator.Instance;
            if (mountSaveLoadHud && _saveLoadHud == null)
            {
                var hudGO = new GameObject("SaveLoadHud");
                _saveLoadHud = hudGO.AddComponent<SaveLoadHud>();
            }

            if (mountLetterbox)
                _letterbox = CinematicLetterbox.EnsureCreated();
            if (mountGtaFeelDirector)
                _feelDirector = MacGtaFeelDirector.EnsureCreated();
            if (mountMacDesktopInput)
            {
                _macInput = MacDesktopInput.EnsureCreated();
                _macInput.Bind(_cinematicCamera, _motionController);
            }

            // Day 9 — cinematic dialogue panel + portrait resolver for GTA feel.
            DialogueOverlay.EnsureCreated();
            PortraitResolver.EnsureCreated();
        }

        private void EnsureGtaCameraAndCombat()
        {
            if (!mountCinematicCamera && !mountArcherAutoFire) return;

            // Main camera + cinematic follow.
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camGO.AddComponent<AudioListener>();
            }

            if (mountCinematicCamera)
            {
                _cinematicCamera = cam.GetComponent<CinematicThirdPersonCamera>();
                if (_cinematicCamera == null)
                    _cinematicCamera = cam.gameObject.AddComponent<CinematicThirdPersonCamera>();
                if (_motionController != null)
                {
                    _cinematicCamera.SetTarget(_motionController.transform);
                    _motionController.SetCameraRoot(cam.transform);
                }
                // Seed a useful GTA-style over-shoulder start pose.
                cam.transform.position = new Vector3(0f, 3.2f, -7.5f);
                cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            }

            if (mountArcherAutoFire && _motionController != null)
            {
                _archer = _motionController.GetComponent<ArcherAutoFire>();
                if (_archer == null)
                    _archer = _motionController.gameObject.AddComponent<ArcherAutoFire>();
            }
        }

        private void BindEvents()
        {
            if (_momentPlayer != null && _hudOrchestrator != null)
            {
                _hudOrchestrator.onDayChanged.AddListener(_ => { /* future hook */ });
            }
            if (_waveController != null && _momentPlayer != null)
            {
                _waveController.onAllWavesCompleted.AddListener(() =>
                {
                    _momentPlayer.CompleteCurrentObjective();
                });
            }
        }
    }
}
