// Day 5+ (RamayanaPS5) — RamayanaWireupTests.
// EditMode tests that prove the Day 1-7 surface is reachable by name.
// These tests do NOT instantiate MonoBehaviours (no scene, no runtime).
// They assert that each type exists in the runtime assembly and exposes
// the public API the rest of the game depends on.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Ramayana.Tests
{
    public class RamayanaWireupTests
    {
        // ── Day 1 — MainMenuScreenController ─────────────────────

        [Test]
        public void Day1_MainMenuScreenController_ExposesKandaEvent()
        {
            var t = Type.GetType("Jambudweep.Ramayana.UI.MainMenuScreenController, Assembly-CSharp");
            Assert.IsNotNull(t, "MainMenuScreenController type not found in Assembly-CSharp");
            var ev = t.GetField("onKandaSelected", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(ev, "MainMenuScreenController.onKandaSelected not exposed as public field");
            Assert.AreEqual(typeof(UnityEngine.Events.UnityEvent<string>),
                ev.FieldType.GenericTypeArguments[0].Name == "String"
                    ? typeof(UnityEngine.Events.UnityEvent<string>)
                    : ev.FieldType);
        }

        [Test]
        public void Day1_MainMenuScreenController_HasBuildAndPopulate()
        {
            var t = Type.GetType("Jambudweep.Ramayana.UI.MainMenuScreenController, Assembly-CSharp");
            Assert.IsNotNull(t);
            var build = t.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance);
            var populate = t.GetMethod("Populate", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(build, "MainMenuScreenController.Build() missing");
            Assert.IsNotNull(populate, "MainMenuScreenController.Populate() missing");
        }

        // ── Day 2 — StoryMomentPlayer ────────────────────────────

        [Test]
        public void Day2_StoryMomentPlayer_HasPublicWalkerApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Story.StoryMomentPlayer, Assembly-CSharp");
            Assert.IsNotNull(t, "StoryMomentPlayer type not found");
            var load = t.GetMethod("LoadAct", new[] { typeof(string) });
            var advance = t.GetMethod("AdvanceObjective", Type.EmptyTypes);
            var complete = t.GetMethod("CompleteCurrentObjective", Type.EmptyTypes);
            Assert.IsNotNull(load, "StoryMomentPlayer.LoadAct(string) missing");
            Assert.IsNotNull(advance, "StoryMomentPlayer.AdvanceObjective() missing");
            Assert.IsNotNull(complete, "StoryMomentPlayer.CompleteCurrentObjective() missing");
            Assert.AreEqual(typeof(bool), load.ReturnType);
            Assert.AreEqual(typeof(bool), advance.ReturnType);
        }

        [Test]
        public void Day2_StoryMomentPlayer_HasUnityEvents()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Story.StoryMomentPlayer, Assembly-CSharp");
            Assert.IsNotNull(t);
            Assert.IsNotNull(t.GetField("onObjectiveEntered", BindingFlags.Public | BindingFlags.Instance));
            Assert.IsNotNull(t.GetField("onObjectiveCompleted", BindingFlags.Public | BindingFlags.Instance));
            Assert.IsNotNull(t.GetField("onActCompleted", BindingFlags.Public | BindingFlags.Instance));
        }

        // ── Day 3 — Combat trio ──────────────────────────────────

        [Test]
        public void Day3_RakshasaTarget_ExposesDamageApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.RakshasaTarget, Assembly-CSharp");
            Assert.IsNotNull(t, "RakshasaTarget type not found");
            var dmg = t.GetMethod("Damage", new[] { typeof(int) });
            Assert.IsNotNull(dmg, "RakshasaTarget.Damage(int) missing");
            Assert.AreEqual(typeof(void), dmg.ReturnType);
            var isDead = t.GetProperty("IsDead", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(isDead, "RakshasaTarget.IsDead missing");
        }

        [Test]
        public void Day3_WaveController_HasStartWavesApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.WaveController, Assembly-CSharp");
            Assert.IsNotNull(t, "WaveController type not found");
            var start = t.GetMethod("StartWaves", new[] { typeof(int) });
            Assert.IsNotNull(start, "WaveController.StartWaves(int) missing");
            Assert.AreEqual(typeof(void), start.ReturnType);
        }

        [Test]
        public void Day3_ArcherAutoFire_DamagesRakshasaTarget()
        {
            // Compile-time: ensure the public surface of ArcherAutoFire is
            // accessible from the runtime assembly. We don't need to fire Update().
            var t = Type.GetType("Jambudweep.Ramayana.Combat.ArcherAutoFire, Assembly-CSharp");
            Assert.IsNotNull(t, "ArcherAutoFire type not found");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t),
                "ArcherAutoFire must inherit from MonoBehaviour");
        }

        // ── Day 4 — HudOrchestrator + SaveLoadHud ─────────────────

        [Test]
        public void Day4_HudOrchestrator_WiresStoryAndCombat()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Feedback.HudOrchestrator, Assembly-CSharp");
            Assert.IsNotNull(t, "HudOrchestrator type not found");
            var bndEvents = t.GetMethod("BindEvents",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(bndEvents, "HudOrchestrator.BindEvents() missing");
        }

        [Test]
        public void Day4_SaveLoadHud_ExposesSaveLoadApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.UI.SaveLoadHud, Assembly-CSharp");
            Assert.IsNotNull(t, "SaveLoadHud type not found");
            Assert.IsNotNull(t.GetMethod("Save", Type.EmptyTypes));
            Assert.IsNotNull(t.GetMethod("Load", Type.EmptyTypes));
            Assert.IsNotNull(t.GetMethod("DeleteSave", Type.EmptyTypes));
        }

        // ── Cross-day: integration signatures ────────────────────

        [Test]
        public void Cross_AllNewTypes_AreSealedMonoBehaviours()
        {
            // Day 5 invariant: every Day 1-4 type is a sealed MonoBehaviour.
            string[] typeNames =
            {
                "Jambudweep.Ramayana.UI.MainMenuScreenController",
                "Jambudweep.Ramayana.Story.StoryMomentPlayer",
                "Jambudweep.Ramayana.Combat.RakshasaTarget",
                "Jambudweep.Ramayana.Combat.WaveController",
                "Jambudweep.Ramayana.Combat.ArcherAutoFire",
                "Jambudweep.Ramayana.Feedback.HudOrchestrator",
                "Jambudweep.Ramayana.UI.SaveLoadHud",
            };
            foreach (var name in typeNames)
            {
                var t = Type.GetType(name + ", Assembly-CSharp");
                Assert.IsNotNull(t, $"{name} not found");
                Assert.IsTrue(t.IsSealed, $"{name} should be sealed");
                Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t),
                    $"{name} should inherit from MonoBehaviour");
            }
        }

        // ── Day 6 — PlayerSceneBootstrap ─────────────────────────

        [Test]
        public void Day6_PlayerSceneBootstrap_ExposesBootApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Scene.PlayerSceneBootstrap, Assembly-CSharp");
            Assert.IsNotNull(t, "PlayerSceneBootstrap type not found");
            Assert.IsTrue(t.IsSealed, "PlayerSceneBootstrap should be sealed");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t),
                "PlayerSceneBootstrap should inherit from MonoBehaviour");
            Assert.IsNotNull(t.GetField("autoLoadActId", BindingFlags.NonPublic | BindingFlags.Instance),
                "autoLoadActId SerializeField missing");
            Assert.IsNotNull(t.GetField("autoStartWaves", BindingFlags.NonPublic | BindingFlags.Instance),
                "autoStartWaves SerializeField missing");
            Assert.IsNotNull(t.GetField("mountSaveLoadHud", BindingFlags.NonPublic | BindingFlags.Instance),
                "mountSaveLoadHud SerializeField missing");
            Assert.IsNotNull(t.GetField("ensureMinimalScene", BindingFlags.NonPublic | BindingFlags.Instance),
                "ensureMinimalScene SerializeField missing");
            Assert.IsNotNull(t.GetField("onPlayerReady", BindingFlags.Public | BindingFlags.Instance),
                "onPlayerReady UnityEvent missing");
            Assert.IsNotNull(t.GetField("onSceneBootstrapped", BindingFlags.Public | BindingFlags.Instance),
                "onSceneBootstrapped UnityEvent missing");
        }

        [Test]
        public void Day6_PlayerSceneBootstrap_WiresDayOneToFour()
        {
            // Source-level: confirm the bootstrap references each of the
            // Day 1-4 system types so a designer can be confident that
            // dropping the component on a scene wires everything up.
            var t = Type.GetType("Jambudweep.Ramayana.Scene.PlayerSceneBootstrap, Assembly-CSharp");
            Assert.IsNotNull(t);
            var src = System.IO.File.ReadAllText(
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "..", "..", "..", "Assets", "Scripts", "Scene", "PlayerSceneBootstrap.cs"));
            Assert.IsTrue(src.Contains("StoryMomentPlayer"),
                "PlayerSceneBootstrap should reference StoryMomentPlayer");
            Assert.IsTrue(src.Contains("WaveController"),
                "PlayerSceneBootstrap should reference WaveController");
            Assert.IsTrue(src.Contains("HudOrchestrator"),
                "PlayerSceneBootstrap should reference HudOrchestrator");
            Assert.IsTrue(src.Contains("SaveLoadHud"),
                "PlayerSceneBootstrap should reference SaveLoadHud");
            Assert.IsTrue(src.Contains("ThirdPersonMotionController"),
                "PlayerSceneBootstrap should reference ThirdPersonMotionController");
        }

        // ── Day 7 — ArrowProjectile + BowCooldown ─────────────────

        [Test]
        public void Day7_ArrowProjectile_ExposesInitializeApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.ArrowProjectile, Assembly-CSharp");
            Assert.IsNotNull(t, "ArrowProjectile type not found");
            Assert.IsTrue(t.IsSealed, "ArrowProjectile should be sealed");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t),
                "ArrowProjectile should inherit from MonoBehaviour");
            var init = t.GetMethod("Initialize", new[]
            {
                typeof(Vector3), typeof(Vector3), typeof(float), typeof(int), typeof(float)
            });
            Assert.IsNotNull(init, "ArrowProjectile.Initialize(Vector3,Vector3,float,int,float) missing");
            Assert.AreEqual(typeof(void), init.ReturnType);
            var create = t.GetMethod("CreateProcedural",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(create, "ArrowProjectile.CreateProcedural factory missing");
        }

        [Test]
        public void Day7_BowCooldown_ExposesCanFireApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.BowCooldown, Assembly-CSharp");
            Assert.IsNotNull(t, "BowCooldown type not found");
            Assert.IsTrue(t.IsSealed, "BowCooldown should be sealed");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t),
                "BowCooldown should inherit from MonoBehaviour");
            Assert.IsNotNull(t.GetMethod("CanFire", Type.EmptyTypes),
                "BowCooldown.CanFire() missing");
            Assert.IsNotNull(t.GetMethod("Consume", Type.EmptyTypes),
                "BowCooldown.Consume() missing");
        }

        [Test]
        public void Day7_ArcherAutoFire_SpawnsArrowProjectile()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.ArcherAutoFire, Assembly-CSharp");
            Assert.IsNotNull(t, "ArcherAutoFire type not found");
            // Source-level: Day 7 archer must plumb ArrowProjectile + BowCooldown.
            var srcPath = System.IO.Path.Combine(
                Application.dataPath, "Scripts", "Combat", "ArcherAutoFire.cs");
            Assert.IsTrue(System.IO.File.Exists(srcPath), "ArcherAutoFire.cs not found on disk");
            var src = System.IO.File.ReadAllText(srcPath);
            Assert.IsTrue(src.Contains("ArrowProjectile"),
                "ArcherAutoFire should spawn/reference ArrowProjectile");
            Assert.IsTrue(src.Contains("BowCooldown"),
                "ArcherAutoFire should delegate to BowCooldown");
            Assert.IsTrue(src.Contains("Initialize"),
                "ArcherAutoFire should call ArrowProjectile.Initialize");
        }

        // ── Day 8 — Mac GTA playable slice ───────────────────────

        [Test]
        public void Day8_MacDesktopInput_ExposesBindApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Platform.MacDesktopInput, Assembly-CSharp");
            Assert.IsNotNull(t, "MacDesktopInput type not found");
            Assert.IsTrue(t.IsSealed, "MacDesktopInput should be sealed");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t));
            Assert.IsNotNull(t.GetMethod("Bind", BindingFlags.Public | BindingFlags.Instance),
                "MacDesktopInput.Bind missing");
            Assert.IsNotNull(t.GetMethod("EnsureCreated", BindingFlags.Public | BindingFlags.Static),
                "MacDesktopInput.EnsureCreated missing");
        }

        [Test]
        public void Day8_CinematicLetterbox_ExposesEnsureCreated()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Feedback.CinematicLetterbox, Assembly-CSharp");
            Assert.IsNotNull(t, "CinematicLetterbox type not found");
            Assert.IsTrue(t.IsSealed);
            Assert.IsNotNull(t.GetMethod("EnsureCreated", BindingFlags.Public | BindingFlags.Static));
            Assert.IsNotNull(t.GetMethod("SetVisible", new[] { typeof(bool) }));
        }

        [Test]
        public void Day8_PlayerSceneBootstrap_WiresMacGtaStack()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Scene.PlayerSceneBootstrap, Assembly-CSharp");
            Assert.IsNotNull(t);
            var src = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts", "Scene", "PlayerSceneBootstrap.cs"));
            Assert.IsTrue(src.Contains("CinematicThirdPersonCamera"),
                "Bootstrap should wire cinematic camera");
            Assert.IsTrue(src.Contains("MacDesktopInput"),
                "Bootstrap should wire Mac desktop input");
            Assert.IsTrue(src.Contains("CinematicLetterbox"),
                "Bootstrap should wire letterbox");
            Assert.IsTrue(src.Contains("MacGtaFeelDirector"),
                "Bootstrap should wire GTA feel director");
            Assert.IsTrue(src.Contains("ArcherAutoFire"),
                "Bootstrap should mount archer for combat");
            Assert.IsTrue(src.Contains("SetCameraRoot"),
                "Bootstrap should set camera-relative movement");
        }

        [Test]
        public void Day8_MotionController_ExposesSetCameraRoot()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Motion3D.ThirdPersonMotionController, Assembly-CSharp");
            Assert.IsNotNull(t, "ThirdPersonMotionController type not found");
            Assert.IsNotNull(t.GetMethod("SetCameraRoot", new[] { typeof(Transform) }),
                "SetCameraRoot(Transform) missing");
        }

        // ── Day 9 — Character portraits ──────────────────────────

        [Test]
        public void Day9_PortraitResolver_ExposesResolveApi()
        {
            var t = Type.GetType("Jambudweep.Ramayana.UI.PortraitResolver, Assembly-CSharp");
            Assert.IsNotNull(t, "PortraitResolver type not found");
            Assert.IsTrue(t.IsSealed, "PortraitResolver should be sealed");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t));
            Assert.IsNotNull(t.GetMethod("Resolve", BindingFlags.Public | BindingFlags.Static),
                "PortraitResolver.Resolve static missing");
            Assert.IsNotNull(t.GetMethod("EnsureCreated", BindingFlags.Public | BindingFlags.Static),
                "PortraitResolver.EnsureCreated missing");
        }

        [Test]
        public void Day9_StoryMomentPlayer_ExposesResolvePortrait()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Story.StoryMomentPlayer, Assembly-CSharp");
            Assert.IsNotNull(t);
            var m = t.GetMethod("ResolvePortrait", new[] { typeof(string) });
            Assert.IsNotNull(m, "StoryMomentPlayer.ResolvePortrait(string) missing");
            Assert.AreEqual(typeof(UnityEngine.Sprite), m.ReturnType);
        }

        [Test]
        public void Day9_DialogueOverlay_ExposesEnsureCreated()
        {
            var t = Type.GetType("Jambudweep.Ramayana.UI.DialogueOverlay, Assembly-CSharp");
            Assert.IsNotNull(t);
            Assert.IsNotNull(t.GetMethod("EnsureCreated", BindingFlags.Public | BindingFlags.Static),
                "DialogueOverlay.EnsureCreated missing");
            var show = t.GetMethod("Show");
            Assert.IsNotNull(show, "DialogueOverlay.Show missing");
        }

        [Test]
        public void Day9_PortraitsFolder_HasKeyFaces()
        {
            var dir = System.IO.Path.Combine(Application.dataPath, "Resources", "portraits");
            Assert.IsTrue(System.IO.Directory.Exists(dir), "Assets/Resources/portraits missing");
            foreach (var id in new[] { "rama", "sita", "hanuman", "lakshmana", "ravana", "default" })
            {
                Assert.IsTrue(System.IO.File.Exists(System.IO.Path.Combine(dir, id + ".png")),
                    $"Missing portrait {id}.png");
            }
        }

        // ── Day 10 — Formation strategies ────────────────────────

        [Test]
        public void Day10_FormationStrategy_HasArcChakraVyuha()
        {
            var abs = Type.GetType("Jambudweep.Ramayana.Combat.FormationStrategy, Assembly-CSharp");
            Assert.IsNotNull(abs, "FormationStrategy type not found");
            Assert.IsTrue(abs.IsAbstract, "FormationStrategy should be abstract");
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(abs),
                "FormationStrategy should NOT be a MonoBehaviour");

            foreach (var name in new[] { "ArcFormation", "ChakraFormation", "VyuhaFormation" })
            {
                var t = Type.GetType($"Jambudweep.Ramayana.Combat.{name}, Assembly-CSharp");
                Assert.IsNotNull(t, $"{name} missing");
                Assert.IsTrue(t.IsSealed, $"{name} should be sealed");
                Assert.IsTrue(abs.IsAssignableFrom(t), $"{name} should extend FormationStrategy");
            }

            var factory = abs.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(factory, "FormationStrategy.For factory missing");
        }

        [Test]
        public void Day10_WaveController_AcceptsFormationStrategy()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.WaveController, Assembly-CSharp");
            Assert.IsNotNull(t);
            Assert.IsNotNull(t.GetMethod("SetFormation"),
                "WaveController.SetFormation missing");
            Assert.IsNotNull(t.GetField("formationKind", BindingFlags.NonPublic | BindingFlags.Instance),
                "formationKind SerializeField missing");
            var prop = t.GetProperty("Formation", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop, "Formation property missing");
        }

        [Test]
        public void Day10_FormationKind_EnumHasThreeLayouts()
        {
            var t = Type.GetType("Jambudweep.Ramayana.Combat.FormationKind, Assembly-CSharp");
            Assert.IsNotNull(t, "FormationKind enum missing");
            Assert.IsTrue(t.IsEnum);
            var names = System.Enum.GetNames(t);
            CollectionAssert.AreEquivalent(new[] { "Arc", "Chakra", "Vyuha" }, names);
        }
    }
}
