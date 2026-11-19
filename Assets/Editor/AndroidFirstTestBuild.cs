using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ShinobiLegacy.Editor
{
    /// <summary>
    /// Reproducible scene setup and APK build entry points for the first Android test.
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
            Debug.Log("SHINOBI LEGACY: First-test scenes generated. Open Build Settings and confirm Android is the active platform before building.");
        }

        [MenuItem("Tools/Shinobi Legacy/Build Android First Test APK")]
        public static void BuildFirstTestApk()
        {
            if (!File.Exists(VillageScene) || !File.Exists(ArenaScene))
            {
                Debug.LogError("SHINOBI LEGACY: Test scenes are missing. Run Tools > Shinobi Legacy > Prepare Android First Test first.");
                return;
            }

            ConfigureAndroidPlayer();
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

            if (scenes.Length == 0 || scenes[0] != VillageScene)
            {
                Debug.LogError("SHINOBI LEGACY: SafeVillage must be the first enabled scene in Build Settings.");
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