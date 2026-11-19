using System;
using System.Collections.Generic;
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
        [SerializeField] private Vector2 buttonSize = new Vector2(148f, 148f);
        [SerializeField] private float spacing = 14f;
        private Canvas controlsCanvas;
        private EnemyTargetLockSystem targetLock;
        private PlayerDodge playerDodge;
        private readonly List<CooldownWidget> cooldownWidgets = new List<CooldownWidget>(6);
        private float nextCooldownRefresh;

        private sealed class CooldownWidget
        {
            public Image overlay;
            public Text label;
            public Func<float> remaining;
            public Func<float> duration;
            public Func<bool> hasResource;
            public string title;
        }

        private void Start()
        {
            if (combat == null) combat = GetComponent<PlayerCombat>();
            if (combat == null) return;
            targetLock = GetComponent<EnemyTargetLockSystem>();
            playerDodge = GetComponent<PlayerDodge>();
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

            CreateButton("ATK", 0, () => combat.BasicAttack(), new Color(0.22f, 0.27f, 0.32f, 0.88f),
                () => combat.BasicAttackCooldownRemaining, () => combat.BasicAttackCooldownDuration, null);
            CreateButton("BURST", 1, () => combat.CastChakraBurst(), new Color(0.12f, 0.48f, 0.78f, 0.92f),
                () => combat.BurstCooldownRemaining, () => combat.BurstCooldownDuration, () => combat.HasChakraForBurst);
            CreateButton("FIRE", 2, () => combat.CastFireStyle(), new Color(0.82f, 0.25f, 0.1f, 0.92f),
                () => combat.FireCooldownRemaining, () => combat.FireCooldownDuration, () => combat.HasChakraForFire);
            CreateButton("LIGHT", 3, () => combat.CastLightningStyle(), new Color(0.48f, 0.35f, 0.9f, 0.92f),
                () => combat.LightningCooldownRemaining, () => combat.LightningCooldownDuration, () => combat.HasChakraForLightning);
            CreateButton("WATER", 4, () => combat.CastWaterStyle(), new Color(0.08f, 0.48f, 0.78f, 0.92f),
                () => combat.WaterCooldownRemaining, () => combat.WaterCooldownDuration, () => combat.HasChakraForWater);
            CreateButton("WIND", 5, () => combat.CastWindStyle(), new Color(0.25f, 0.62f, 0.52f, 0.92f),
                () => combat.WindCooldownRemaining, () => combat.WindCooldownDuration, () => combat.HasChakraForWind);
            if (targetLock != null)
                CreateButton("TARGET", 6, () => targetLock.CycleTarget(), new Color(0.72f, 0.42f, 0.12f, 0.94f));
            if (playerDodge != null)
                CreateButton("DODGE", 7, () => playerDodge.TryDodge(), new Color(0.18f, 0.58f, 0.38f, 0.94f),
                    () => playerDodge.CooldownRemaining, () => playerDodge.CooldownDuration, null);
        }

        private void CreateButton(string label, int index, UnityEngine.Events.UnityAction action, Color color,
            Func<float> remaining = null, Func<float> duration = null, Func<bool> hasResource = null)
        {
            GameObject buttonObject = new GameObject("Ability_" + label,
                typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(controlsCanvas.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = new Vector2(-28f - (index % 3) * (buttonSize.x + spacing),
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
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;

            if (remaining != null && duration != null)
            {
                GameObject overlayObject = new GameObject("Cooldown Overlay",
                    typeof(RectTransform), typeof(Image));
                overlayObject.transform.SetParent(buttonObject.transform, false);
                RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                Image overlay = overlayObject.GetComponent<Image>();
                overlay.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                overlayObject.transform.SetAsFirstSibling();
                overlay.type = Image.Type.Filled;
                overlay.fillMethod = Image.FillMethod.Radial360;
                overlay.fillOrigin = (int)Image.Origin360.Top;
                overlay.fillClockwise = false;
                overlay.color = new Color(0.015f, 0.02f, 0.035f, 0.78f);
                overlay.raycastTarget = false;
                cooldownWidgets.Add(new CooldownWidget
                {
                    overlay = overlay,
                    label = text,
                    remaining = remaining,
                    duration = duration,
                    hasResource = hasResource,
                    title = label
                });
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < nextCooldownRefresh) return;
            nextCooldownRefresh = Time.unscaledTime + 0.1f;
            for (int i = 0; i < cooldownWidgets.Count; i++)
            {
                CooldownWidget widget = cooldownWidgets[i];
                if (widget.overlay == null || widget.label == null) continue;
                float remaining = Mathf.Max(0f, widget.remaining());
                float duration = Mathf.Max(0.01f, widget.duration());
                widget.overlay.fillAmount = Mathf.Clamp01(remaining / duration);
                bool resourceReady = widget.hasResource == null || widget.hasResource();
                widget.label.text = remaining > 0.05f
                    ? Mathf.CeilToInt(remaining).ToString()
                    : widget.title;
                widget.label.color = resourceReady || remaining > 0.05f
                    ? Color.white
                    : new Color(1f, 0.72f, 0.58f, 1f);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void OnDestroy()
        {
            cooldownWidgets.Clear();
            if (controlsCanvas != null) Destroy(controlsCanvas.gameObject);
        }
    }
}