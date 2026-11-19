using UnityEngine;
using UnityEngine.Profiling;

namespace ShinobiLegacy.Performance
{
    /// <summary>
    /// Lightweight development-only diagnostic overlay for physical Android performance checks.
    /// Samples frame rate twice per second and keeps a rolling 10-second window.
    /// </summary>
    public sealed class MobilePerformanceOverlay : MonoBehaviour
    {
        private const int SampleCount = 20;
        private readonly float[] fpsSamples = new float[SampleCount];
        private int sampleIndex;
        private int sampleTotal;
        private int framesInInterval;
        private float intervalElapsed;
        private float sampleFps;
        private float averageFps;
        private float minimumFps;
        private bool visible = true;
        private GUIStyle diagnosticStyle;
        private string diagnosticText = "Collecting performance data...";

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            if (delta <= 0f) return;

            framesInInterval++;
            intervalElapsed += delta;
            if (intervalElapsed < 0.5f) return;

            sampleFps = framesInInterval / intervalElapsed;
            fpsSamples[sampleIndex] = sampleFps;
            sampleIndex = (sampleIndex + 1) % SampleCount;
            sampleTotal = Mathf.Min(sampleTotal + 1, SampleCount);

            float sum = 0f;
            minimumFps = float.MaxValue;
            for (int i = 0; i < sampleTotal; i++)
            {
                float value = fpsSamples[i];
                sum += value;
                minimumFps = Mathf.Min(minimumFps, value);
            }
            averageFps = sampleTotal > 0 ? sum / sampleTotal : 0f;

            framesInInterval = 0;
            intervalElapsed = 0f;
            diagnosticText =
                "FPS now: " + sampleFps.ToString("0") +
                "   10s avg: " + averageFps.ToString("0") +
                "   min: " + (sampleTotal > 0 ? minimumFps.ToString("0") : "--") +
                "\nFrame: " + (1000f / Mathf.Max(1f, sampleFps)).ToString("0.0") + " ms" +
                "   Unity memory: " + (Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f)).ToString("0") + " MB" +
                "\nDevice: " + SystemInfo.deviceModel +
                "\nGPU: " + SystemInfo.graphicsDeviceName +
                "   " + Screen.width + "x" + Screen.height;
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            if (diagnosticStyle == null)
            {
                diagnosticStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    wordWrap = true
                };
                diagnosticStyle.normal.textColor = Color.white;
            }

            const float panelWidth = 390f;
            const float panelHeight = 118f;
            float scale = Mathf.Clamp(Screen.width / 1600f, 0.65f, 1f);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            float x = 12f;
            float y = 12f;
            if (visible)
            {
                GUI.Box(new Rect(x, y, panelWidth, panelHeight), "SHINOBI LEGACY - PERFORMANCE");
                GUI.Label(new Rect(x + 10f, y + 25f, panelWidth - 20f, panelHeight - 32f), diagnosticText, diagnosticStyle);
                if (GUI.Button(new Rect(x + panelWidth - 30f, y + 3f, 24f, 20f), "–"))
                    visible = false;
            }
            else if (GUI.Button(new Rect(x, y, 116f, 28f), "SHOW FPS"))
            {
                visible = true;
            }

            GUI.matrix = previousMatrix;
        }
    }
}