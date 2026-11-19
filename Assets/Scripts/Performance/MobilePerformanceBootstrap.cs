using UnityEngine;

namespace ShinobiLegacy.Performance
{
    /// <summary>
    /// Conservative mobile defaults for the first Android test build.
    /// The target is stable 30 FPS on entry-level devices, not maximum visual quality.
    /// </summary>
    public sealed class MobilePerformanceBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;

            if (!Application.isMobilePlatform) return;

            QualitySettings.antiAliasing = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.lodBias = 0.75f;
            QualitySettings.maximumLODLevel = 1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}