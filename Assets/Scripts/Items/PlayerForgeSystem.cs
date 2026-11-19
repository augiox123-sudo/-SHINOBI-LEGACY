using UnityEngine;
using UnityEngine.UI;

namespace ShinobiLegacy.Items
{
    /// <summary>Mobile forge: upgrades carried and equipped gear using coins and dungeon materials.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerForgeSystem : MonoBehaviour
    {
        private const string MaterialKey = "SHINOBI_FORGE_MATERIALS_V1";
        private PlayerInventory inventory;
        private EquipmentManager equipment;
        private PlayerWallet wallet;
        private int materials;
        private GameObject panel;
        private RectTransform rows;
        private Text header;

        public int Materials => materials;

        private void Start()
        {
            inventory = GetComponent<PlayerInventory>();
            equipment = GetComponent<EquipmentManager>();
            wallet = GetComponent<PlayerWallet>();
            materials = Mathf.Max(0, PlayerPrefs.GetInt(MaterialKey, 0));
            if (inventory == null || equipment == null || wallet == null) return;
            BuildUI();
        }

        public void AddMaterials(int amount)
        {
            if (amount <= 0) return;
            materials += amount;
            PlayerPrefs.SetInt(MaterialKey, materials);
            PlayerPrefs.Save();
            Refresh();
        }

        public int GetCoinCost(InventoryItem item)
        {
            return item == null ? int.MaxValue : 100 + item.upgradeLevel * 85 + (int)item.rarity * 35;
        }

        public int GetMaterialCost(InventoryItem item)
        {
            return item == null ? int.MaxValue : 2 + item.upgradeLevel * 2 + (int)item.rarity;
        }

        public bool UpgradeInventoryItem(int index)
        {
            if (inventory == null || index < 0 || index >= inventory.Count) return false;
            InventoryItem item = inventory.Items[index];
            if (!CanUpgrade(item)) return false;
            int coins = GetCoinCost(item);
            int mats = GetMaterialCost(item);
            if (!wallet.Spend(coins)) return false;
            materials -= mats;
            bool success = inventory.UpgradeAt(index, 2 + item.upgradeLevel,
                0.003f, 0.35f, 0.001f, 0.004f);
            if (!success)
            {
                materials += mats;
                wallet.Add(coins);
                return false;
            }
            SaveMaterials();
            Refresh();
            return true;
        }

        public bool UpgradeEquippedItem(string slot)
        {
            if (equipment == null) return false;
            InventoryItem item = equipment.GetEquipped(slot);
            if (!CanUpgrade(item)) return false;
            int coins = GetCoinCost(item);
            int mats = GetMaterialCost(item);
            if (!wallet.Spend(coins)) return false;
            materials -= mats;
            bool success = equipment.UpgradeEquipped(slot, 2 + item.upgradeLevel,
                0.003f, 0.35f, 0.001f, 0.004f);
            if (!success)
            {
                materials += mats;
                wallet.Add(coins);
                return false;
            }
            SaveMaterials();
            Refresh();
            return true;
        }

        private bool CanUpgrade(InventoryItem item)
        {
            return item != null && item.upgradeLevel < 10 &&
                wallet != null && wallet.Coins >= GetCoinCost(item) &&
                materials >= GetMaterialCost(item);
        }

        private void SaveMaterials()
        {
            PlayerPrefs.SetInt(MaterialKey, materials);
            PlayerPrefs.Save();
        }

        private void BuildUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Forge Canvas",
                    typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            CreateButton(canvas.transform, "FORGE", new Vector2(30f, -285f),
                new Vector2(190f, 68f), TogglePanel);
            panel = new GameObject("Forge Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(820f, 760f);
            panel.GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.065f, 0.97f);
            header = CreateText(panel.transform, "", new Vector2(0f, -30f),
                new Vector2(740f, 70f), 25, TextAnchor.MiddleCenter);
            CreateButton(panel.transform, "CLOSE", new Vector2(-25f, -25f),
                new Vector2(130f, 54f), TogglePanel, true);

            GameObject scroll = new GameObject("Forge Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(panel.transform, false);
            RectTransform sr = scroll.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero;
            sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(24f, 24f);
            sr.offsetMax = new Vector2(-24f, -130f);
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            RectTransform vr = viewport.GetComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = new Vector2(6f, 6f);
            vr.offsetMax = new Vector2(-6f, -6f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            GameObject content = new GameObject("Rows", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            rows = content.GetComponent<RectTransform>();
            rows.anchorMin = new Vector2(0f, 1f);
            rows.anchorMax = new Vector2(1f, 1f);
            rows.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.viewport = vr;
            scrollRect.content = rows;
            scrollRect.horizontal = false;
            panel.SetActive(false);
        }

        private void TogglePanel()
        {
            if (panel == null) return;
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) Refresh();
        }

        private void Refresh()
        {
            if (rows == null) return;
            header.text = "VILLAGE FORGE   |   MATERIALS: " + materials +
                "   |   COINS: " + wallet.Coins + "\nEnhance equipment up to +10";
            for (int i = rows.childCount - 1; i >= 0; i--)
                Destroy(rows.GetChild(i).gameObject);

            string[] slots = { "Weapon", "Chest", "Hands", "Accessory" };
            for (int i = 0; i < slots.Length; i++)
            {
                string slot = slots[i];
                InventoryItem item = equipment.GetEquipped(slot);
                if (item != null)
                {
                    int level = item.upgradeLevel;
                    string label = slot + "  |  " + item.itemName + "  +" + level +
                        "  |  POWER " + item.power;
                    AddRow(label, GetCoinCost(item), GetMaterialCost(item), CanUpgrade(item),
                        () => UpgradeEquippedItem(slot));
                }
            }

            for (int i = 0; i < inventory.Count; i++)
            {
                int index = i;
                InventoryItem item = inventory.Items[i];
                if (item == null) continue;
                string label = item.itemName + "  +" + item.upgradeLevel +
                    "  |  " + item.slot + "  |  POWER " + item.power;
                AddRow(label, GetCoinCost(item), GetMaterialCost(item), CanUpgrade(item),
                    () => UpgradeInventoryItem(index));
            }
            if (rows.childCount == 0)
                CreateText(rows, "No equipment available. Collect gear from enemies and dungeons.",
                    Vector2.zero, new Vector2(700f, 70f), 20, TextAnchor.MiddleCenter);
        }

        private void AddRow(string label, int coinCost, int materialCost, bool canUpgrade,
            UnityEngine.Events.UnityAction action)
        {
            GameObject row = new GameObject("Forge Item", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(rows, false);
            row.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.17f, 0.96f);
            LayoutElement element = row.AddComponent<LayoutElement>();
            element.minHeight = 94f;
            element.preferredHeight = 94f;
            HorizontalLayoutGroup group = row.GetComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(10, 10, 6, 6);
            group.spacing = 8f;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandWidth = false;
            Text text = CreateText(row.transform, label + "\nCost: " + coinCost + " coins / " +
                materialCost + " materials", Vector2.zero, new Vector2(0f, 80f), 16, TextAnchor.MiddleLeft);
            LayoutElement textLayout = text.gameObject.AddComponent<LayoutElement>();
            textLayout.flexibleWidth = 1f;
            textLayout.minWidth = 420f;
            Button button = CreateButton(row.transform, "UPGRADE", Vector2.zero,
                new Vector2(150f, 62f), action);
            button.interactable = canUpgrade;
        }

        private static Text CreateText(Transform parent, string value, Vector2 position,
            Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position,
            Vector2 size, UnityEngine.Events.UnityAction action, bool topRight = false)
        {
            GameObject obj = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = rect.anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = obj.GetComponent<Image>();
            image.color = new Color(0.14f, 0.34f, 0.48f, 0.98f);
            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            Text text = CreateText(obj.transform, label, Vector2.zero, size, 18, TextAnchor.MiddleCenter);
            RectTransform tr = text.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return button;
        }

        private void OnDestroy()
        {
            if (panel != null) Destroy(panel);
        }
    }
}