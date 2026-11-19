using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinobiLegacy.Editor
{
    /// <summary>
    /// Reproducible scene setup, validation and APK build entry points for the first Android test.
    /// Requires Unity 2022.3 LTS with Android Build Support, SDK, NDK and OpenJDK installed.
    /// </summary>
    public static class AndroidFirstTestBuild
    {
        private const string VillageScene = "Assets/Scenes/SafeVillage.unity";
        private const string ArenaScene = "Assets/Scenes/PrototypeArena.unity";
        private const string OutputPath = "Builds/Android/SHINOBI_LEGACY_FirstTest.apk";

        [MenuItem("Tools/Shinobi Legacy/Prepare Android First Test")]
        public static void PrepareFirstTest()
        {
            PrototypeSceneGenerator.CreateSceneForBuild();
            SafeVillageSceneGenerator.CreateVillageForBuild();
            ConfigureAndroidPlayer();
            Debug.Log("SHINOBI LEGACY: First-test scenes generated. Run Validate Android First Test before building.");
        }

        [MenuItem("Tools/Shinobi Legacy/Validate Android First Test")]
        public static void ValidateAndroidFirstTest()
        {
            ConfigureAndroidPlayer();
            if (ValidateFirstTestProject())
                Debug.Log("SHINOBI LEGACY: Android first-test validation passed. The required scenes, player controls, camera and combat objects were found.");
        }

        [MenuItem("Tools/Shinobi Legacy/Build Android First Test APK")]
        public static void BuildFirstTestApk()
        {
            ConfigureAndroidPlayer();
            if (!ValidateFirstTestProject())
            {
                Debug.LogError("SHINOBI LEGACY: APK build cancelled because project validation failed. Fix the errors above and run validation again.");
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                if (!switched || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    Debug.LogError("SHINOBI LEGACY: Switch the active build platform to Android, wait for Unity to finish reimporting, then run the APK build again.");
                    return;
                }
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length < 2 || scenes[0] != VillageScene || scenes[1] != ArenaScene)
            {
                Debug.LogError("SHINOBI LEGACY: Build Settings must contain SafeVillage first and PrototypeArena second. Run Prepare Android First Test again.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("SHINOBI LEGACY: Android APK ready: " + Path.GetFullPath(OutputPath) +
                    " | Size: " + (report.summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB");
            else
                Debug.LogError("SHINOBI LEGACY: Android build failed. Result: " + report.summary.result +
                    ". Check the Unity Console and Editor.log for the first compiler or Android toolchain error.");
        }

        private static bool ValidateFirstTestProject()
        {
            List<string> errors = new List<string>();

            if (!File.Exists(VillageScene))
                errors.Add("Missing scene: " + VillageScene + ". Run Prepare Android First Test.");
            if (!File.Exists(ArenaScene))
                errors.Add("Missing scene: " + ArenaScene + ". Run Prepare Android First Test.");

            if (File.Exists(VillageScene))
                ValidateScene(VillageScene, false, errors);
            if (File.Exists(ArenaScene))
                ValidateScene(ArenaScene, true, errors);

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length < 2 ||
                !buildScenes[0].enabled || buildScenes[0].path != VillageScene ||
                !buildScenes[1].enabled || buildScenes[1].path != ArenaScene)
            {
                errors.Add("Build Settings must have SafeVillage enabled as scene 1 and PrototypeArena enabled.");
            }

            if (PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) != "com.shinobilegacy.game")
                errors.Add("Android application identifier must be com.shinobilegacy.game.");

            if (errors.Count == 0)
                return true;

            Debug.LogError("SHINOBI LEGACY: Android first-test validation found " + errors.Count + " issue(s):\n - " +
                string.Join("\n - ", errors));
            return false;
        }

        private static void ValidateScene(string scenePath, bool isArena, List<string> errors)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = false;

            try
            {
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    openedHere = true;
                }

                if (!scene.IsValid() || !scene.isLoaded)
                {
                    errors.Add("Could not open scene for validation: " + scenePath);
                    return;
                }

                List<GameObject> objects = new List<GameObject>();
                foreach (GameObject root in scene.GetRootGameObjects())
                    objects.AddRange(root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject));

                GameObject player = objects.FirstOrDefault(go => go.GetComponent("PlayerMotor") != null);
                if (player == null)
                {
                    errors.Add(scenePath + ": no player with PlayerMotor was found.");
                }
                else
                {
                    RequireComponent(player, "CharacterController", scenePath, errors);
                    RequireComponent(player, "CombatantHealth", scenePath, errors);
                    RequireComponent(player, "PlayerCombat", scenePath, errors);
                    RequireComponent(player, "MobileAbilityControls", scenePath, errors);
                    RequireComponent(player, "MobilePerformanceBootstrap", scenePath, errors);
                }

                bool hasMainCamera = objects.Any(go =>
                {
                    Camera camera = go.GetComponent<Camera>();
                    return camera != null && go.CompareTag("MainCamera");
                });
                if (!hasMainCamera)
                    errors.Add(scenePath + ": tagged MainCamera with Camera component is missing.");

                if (!objects.Any(go => go.GetComponent("IsometricCameraFollow") != null))
                    errors.Add(scenePath + ": IsometricCameraFollow is missing.");

                if (!objects.Any(go => go.GetComponent("VirtualJoystick") != null))
                    errors.Add(scenePath + ": VirtualJoystick is missing.");

                if (!objects.Any(go => go.GetComponent("EventSystem") != null))
                    errors.Add(scenePath + ": EventSystem is missing.");

                if (!objects.Any(go => go.GetComponent<Canvas>() != null))
                    errors.Add(scenePath + ": UI Canvas is missing.");

                if (isArena)
                {
                    if (!objects.Any(go => go.GetComponent("BossCombatAI") != null))
                        errors.Add(scenePath + ": BossCombatAI is missing from the combat test arena.");
                    if (!objects.Any(go => go.GetComponent("EnemyCombatAI") != null))
                        errors.Add(scenePath + ": no EnemyCombatAI training enemy was found.");
                }
                else
                {
                    if (!objects.Any(go => go.GetComponent("VillageQuestSystem") != null))
                        errors.Add(scenePath + ": VillageQuestSystem is missing.");
                    if (!objects.Any(go => go.GetComponent("FastTravelMapUI") != null))
                        errors.Add(scenePath + ": FastTravelMapUI is missing.");
                }
            }
            catch (System.Exception exception)
            {
                errors.Add(scenePath + ": validation threw " + exception.GetType().Name + " - " + exception.Message);
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void RequireComponent(GameObject target, string componentName, string scenePath, List<string> errors)
        {
            if (target.GetComponent(componentName) == null)
                errors.Add(scenePath + ": player is missing " + componentName + ".");
        }

        private static void ConfigureAndroidPlayer()
        {
            PlayerSettings.companyName = "Shinobi Legacy";
            PlayerSettings.productName = "SHINOBI LEGACY";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.shinobilegacy.game");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
        }
    }
}