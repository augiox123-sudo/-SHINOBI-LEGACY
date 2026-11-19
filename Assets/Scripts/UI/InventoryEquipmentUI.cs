using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.UI
{
    /// <summary>
    /// Runtime inventory and equipment panel for touch devices.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryEquipmentUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private PlayerWallet wallet;
        private GameObject panel;
        private Text summaryText;
        private Text listText;
        private RectTransform listRoot;
        private bool isOpen;
        private float refreshTimer;

        private void Start()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            if (wallet == null) wallet = GetComponent<PlayerWallet>();
            if (inventory == null || equipment == null || wallet == null) return;
            BuildUI();
            Refresh();
        }

        private void BuildUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Inventory UI Canvas",
                    typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            CreateButton(canvas.transform, "INVENTORY", new Vector2(30f, -205f),
                new Vector2(190f, 68f), TogglePanel);

            panel = new GameObject("Inventory Equipment Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900f, 760f);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.07f, 0.96f);

            CreateText(panel.transform, "INVENTORY  /  EQUIPMENT", new Vector2(0f, -24f),
                new Vector2(820f, 54f), 30, TextAnchor.MiddleCenter);
            summaryText = CreateText(panel.transform, "", new Vector2(0f, -88f),
                new Vector2(820f, 112f), 20, TextAnchor.MiddleLeft);

            CreateButton(panel.transform, "CLOSE", new Vector2(-28f, -28f),
                new Vector2(130f, 54f), TogglePanel, true);

            GameObject scrollObject = new GameObject("Item Scroll View",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel.transform, false);
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(28f, 28f);
            scrollRect.offsetMax = new Vector2(-28f, -220f);
            scrollObject.GetComponent<Image>().color = new Color(0.06f, 0.085f, 0.12f, 0.95f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 8f);
            viewportRect.offsetMax = new Vector2(-8f, -8f);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewport.transform, false);
            listRoot = contentObject.GetComponent<RectTransform>();
            listRoot.anchorMin = new Vector2(0f, 1f);
            listRoot.anchorMax = new Vector2(1f, 1f);
            listRoot.pivot = new Vector2(0.5f, 1f);
            listRoot.sizeDelta = new Vector2(0f, 0f);
            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = listRoot;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            panel.SetActive(false);
        }

        private void Update()
        {
            if (!isOpen) return;
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= 0.5f)
            {
                refreshTimer = 0f;
                Refresh();
            }
        }

        private void TogglePanel()
        {
            isOpen = !isOpen;
            if (panel != null) panel.SetActive(isOpen);
            if (isOpen) Refresh();
        }

        private void Refresh()
        {
            if (summaryText == null || inventory == null) return;
            summaryText.text = "COINS: " + wallet.Coins + "    BAG: " + inventory.Count + "/60\n" +
                "WEAPON: " + ItemSummary(equipment.GetEquipped("Weapon")) + "\n" +
                "CHEST: " + ItemSummary(equipment.GetEquipped("Chest")) + "    HANDS: " +
                ItemSummary(equipment.GetEquipped("Hands")) + "\n" +
                "ACCESSORY: " + ItemSummary(equipment.GetEquipped("Accessory"));
            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);

            for (int i = 0; i < inventory.Count; i++)
            {
                InventoryItem item = inventory.Items[i];
                if (item == null) continue;
                int index = i;
                string details = item.itemName + "  [" + item.rarity.ToString().ToUpper() + "]  " +
                    item.slot + "  POWER " + item.power + "\n" + StatSummary(item);
                CreateItemRow(item, details, index);
            }
            if (inventory.Count == 0)
                CreateText(listRoot, "Your inventory is empty. Defeat enemies to collect equipment.",
                    Vector2.zero, new Vector2(760f, 70f), 20, TextAnchor.MiddleCenter);
        }

        private void CreateItemRow(InventoryItem item, string details, int index)
        {
            GameObject row = new GameObject("Item_" + index, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(listRoot, false);
            row.GetComponent<Image>().color = RarityColor(item.rarity);
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.minHeight = 104f;
            rowLayout.preferredHeight = 104f;
            HorizontalLayoutGroup horizontal = row.GetComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(10, 10, 6, 6);
            horizontal.spacing = 8f;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = true;

            Text label = CreateText(row.transform, details, Vector2.zero,
                new Vector2(0f, 84f), 17, TextAnchor.MiddleLeft);
            LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1f;
            labelLayout.minWidth = 400f;

            CreateButton(row.transform, "EQUIP", Vector2.zero, new Vector2(116f, 64f),
                () => EquipItem(index));
            CreateButton(row.transform, "SELL", Vector2.zero, new Vector2(116f, 64f),
                () => SellItem(index));
        }

        private void EquipItem(int index)
        {
            if (inventory.EquipAt(index, equipment)) Refresh();
        }

        private void SellItem(int index)
        {
            if (index < 0 || index >= inventory.Count) return;
            InventoryItem item = inventory.Items[index];
            if (item == null) return;
            int sellPrice = Mathf.Max(5, item.power * 3 + ((int)item.rarity * 18));
            if (!inventory.RemoveAt(index)) return;
            wallet.Add(sellPrice);
            Refresh();
        }

        private static string ItemSummary(InventoryItem item)
        {
            return item == null ? "EMPTY" : item.itemName + " (PWR " + item.power + ")";
        }

        private static string StatSummary(InventoryItem item)
        {
            string stats = "DMG +" + (item.bonusDamagePercent * 100f).ToString("0.0") +
                "%  DEF +" + item.bonusDefense.ToString("0.0") +
                "  CRIT +" + (item.bonusCriticalChance * 100f).ToString("0.0") +
                "%  CRIT DMG +" + (item.bonusCriticalDamage * 100f).ToString("0.0") + "%";
            if (!string.IsNullOrEmpty(item.abilityModifier)) stats += "  SKILL: " + item.abilityModifier;
            return stats;
        }

        private static Color RarityColor(LootRarity rarity)
        {
            switch (rarity)
            {
                case LootRarity.Uncommon: return new Color(0.12f, 0.28f, 0.16f, 0.95f);
                case LootRarity.Rare: return new Color(0.1f, 0.2f, 0.36f, 0.95f);
                case LootRarity.Epic: return new Color(0.28f, 0.13f, 0.38f, 0.95f);
                case LootRarity.Legendary: return new Color(0.42f, 0.24f, 0.08f, 0.98f);
                default: return new Color(0.16f, 0.18f, 0.21f, 0.95f);
            }
        }

        private static Text CreateText(Transform parent, string value, Vector2 position,
            Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateButton(Transform parent, string label, Vector2 position,
            Vector2 size, UnityEngine.Events.UnityAction action, bool topRight = false)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (topRight)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.32f, 0.5f, 0.96f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            Text text = CreateText(buttonObject.transform, label, Vector2.zero, size, 20, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (panel != null) Destroy(panel);
        }
    }
}