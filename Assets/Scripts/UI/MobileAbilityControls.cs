using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.UI
{
    /// <summary>
    /// Creates touch-friendly combat buttons at runtime for Android builds.
    /// Kept separate from the scene HUD so both arena and village scenes use the same controls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileAbilityControls : MonoBehaviour
    {
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Vector2 buttonSize = new Vector2(112f, 112f);
        [SerializeField] private float spacing = 14f;
        private Canvas controlsCanvas;

        private void Start()
        {
            if (combat == null) combat = GetComponent<PlayerCombat>();
            if (combat == null) return;
            EnsureEventSystem();
            BuildControls();
        }

        private void BuildControls()
        {
            GameObject canvasObject = new GameObject("Mobile Ability Controls",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            controlsCanvas = canvasObject.GetComponent<Canvas>();
            controlsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            controlsCanvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateButton("ATK", 0, () => combat.BasicAttack(), new Color(0.22f, 0.27f, 0.32f, 0.88f));
            CreateButton("BURST", 1, () => combat.CastChakraBurst(), new Color(0.12f, 0.48f, 0.78f, 0.92f));
            CreateButton("FIRE", 2, () => combat.CastFireStyle(), new Color(0.82f, 0.25f, 0.1f, 0.92f));
            CreateButton("LIGHT", 3, () => combat.CastLightningStyle(), new Color(0.48f, 0.35f, 0.9f, 0.92f));
            CreateButton("WATER", 4, () => combat.CastWaterStyle(), new Color(0.08f, 0.48f, 0.78f, 0.92f));
            CreateButton("WIND", 5, () => combat.CastWindStyle(), new Color(0.25f, 0.62f, 0.52f, 0.92f));
        }

        private void CreateButton(string label, int index, UnityEngine.Events.UnityAction action, Color color)
        {
            GameObject buttonObject = new GameObject("Ability_" + label,
                typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(controlsCanvas.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = new Vector2(28f + (index % 3) * (buttonSize.x + spacing),
                28f + (index / 3) * (buttonSize.y + spacing));
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4f, 4f);
            textRect.offsetMax = new Vector2(-4f, -4f);
            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 22;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void OnDestroy()
        {
            if (controlsCanvas != null) Destroy(controlsCanvas.gameObject);
        }
    }
}