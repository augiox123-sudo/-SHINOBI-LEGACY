using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ShinobiLegacy.UI
{
    /// <summary>
    /// Runtime graphics menu for Android and desktop. Lets the player select a 30/60 FPS
    /// target and disable/restore real-time shadows to reduce GPU load.
    /// </summary>
    public sealed class GraphicsSettingsMenu : MonoBehaviour
    {
        private const string FpsKey = "SL_Graphics_FPS";
        private const string ShadowsKey = "SL_Graphics_Shadows";

        private Canvas canvas;
        private GameObject panel;
        private Text fpsButtonText;
        private Text shadowsButtonText;
        private int targetFps = 30;
        private bool shadowsEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<GraphicsSettingsMenu>() != null) return;
            GameObject root = new GameObject("Graphics Settings");
            DontDestroyOnLoad(root);
            root.AddComponent<GraphicsSettingsMenu>();
        }

        private void Awake()
        {
            if (FindObjectsOfType<GraphicsSettingsMenu>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            targetFps = PlayerPrefs.GetInt(FpsKey, 30) == 60 ? 60 : 30;
            shadowsEnabled = PlayerPrefs.GetInt(ShadowsKey, 0) == 1;
            ApplySettings();
            BuildMenu();
        }

        private void ApplySettings()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
            QualitySettings.shadows = shadowsEnabled ? ShadowQuality.All : ShadowQuality.Disable;
            QualitySettings.shadowDistance = shadowsEnabled ? 35f : 0f;
            PlayerPrefs.SetInt(FpsKey, targetFps);
            PlayerPrefs.SetInt(ShadowsKey, shadowsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void BuildMenu()
        {
            GameObject canvasObject = new GameObject("Graphics Settings Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("Graphics Settings EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Button openButton = CreateButton("GRAPHICS", canvasObject.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-105f, -55f),
                new Vector2(180f, 64f), new Color(0.08f, 0.16f, 0.24f, 0.92f), out _);
            openButton.onClick.AddListener(TogglePanel);

            panel = new GameObject("Graphics Settings Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-24f, -132f);
            panelRect.sizeDelta = new Vector2(390f, 250f);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.055f, 0.085f, 0.96f);

            CreateButton("FRAME RATE", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -18f), new Vector2(340f, 52f), new Color(0.12f, 0.24f, 0.34f, 1f), out Text heading);
            heading.text = "FRAME RATE";

            Button fpsButton = CreateButton("", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -82f), new Vector2(340f, 52f), new Color(0.12f, 0.34f, 0.42f, 1f), out fpsButtonText);
            fpsButton.onClick.AddListener(ToggleFps);

            Button shadowsButton = CreateButton("", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -146f), new Vector2(340f, 52f), new Color(0.25f, 0.22f, 0.16f, 1f), out shadowsButtonText);
            shadowsButton.onClick.AddListener(ToggleShadows);

            Button closeButton = CreateButton("CLOSE", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -210f), new Vector2(340f, 34f), new Color(0.18f, 0.2f, 0.24f, 1f), out _);
            closeButton.onClick.AddListener(TogglePanel);

            panel.SetActive(false);
            RefreshLabels();
        }

        private Button CreateButton(string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size, Color color, out Text labelText)
        {
            GameObject buttonObject = new GameObject(string.IsNullOrEmpty(label) ? "Setting Button" : label,
                typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 2f);
            textRect.offsetMax = new Vector2(-8f, -2f);
            labelText = textObject.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 22;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;
            labelText.raycastTarget = false;
            return button;
        }

        private void TogglePanel()
        {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }

        private void ToggleFps()
        {
            targetFps = targetFps == 30 ? 60 : 30;
            ApplySettings();
            RefreshLabels();
        }

        private void ToggleShadows()
        {
            shadowsEnabled = !shadowsEnabled;
            ApplySettings();
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (fpsButtonText != null) fpsButtonText.text = targetFps + " FPS  (TAP TO CHANGE)";
            if (shadowsButtonText != null)
                shadowsButtonText.text = "SHADOWS: " + (shadowsEnabled ? "ON" : "OFF") + "  (TAP TO CHANGE)";
        }
    }
}
