using UnityEngine;

namespace ShinobiLegacy.Performance
{
    /// <summary>
    /// Conservative mobile defaults for the first Android test build.
    /// The target is stable 30 FPS on entry-level devices, not maximum visual quality.
    /// </summary>
    public sealed class MobilePerformanceBootstrap : MonoBehaviour
    {
        private const string FpsPreferenceKey = "SL_Graphics_FPS";
        private const string ShadowsPreferenceKey = "SL_Graphics_Shadows";

        private void Awake()
        {
            int targetFps = PlayerPrefs.GetInt(FpsPreferenceKey, 30) == 60 ? 60 : 30;
            bool shadowsEnabled = PlayerPrefs.GetInt(ShadowsPreferenceKey, 0) == 1;
            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0;

            if (Debug.isDebugBuild && GetComponent<MobilePerformanceOverlay>() == null)
                gameObject.AddComponent<MobilePerformanceOverlay>();

            if (!Application.isMobilePlatform) return;

            QualitySettings.antiAliasing = 0;
            QualitySettings.shadows = shadowsEnabled ? ShadowQuality.All : ShadowQuality.Disable;
            QualitySettings.shadowDistance = shadowsEnabled ? 35f : 0f;
            QualitySettings.lodBias = 0.75f;
            QualitySettings.maximumLODLevel = 1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}