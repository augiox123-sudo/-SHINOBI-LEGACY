using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class VillageShopUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerWallet wallet;
        private VillageVendor nearbyVendor;
        private bool showShop;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (wallet == null) wallet = GetComponent<PlayerWallet>();
        }

        private void Update()
        {
            if (nearbyVendor != null && Input.GetKeyDown(KeyCode.E)) showShop = !showShop;
            if (nearbyVendor == null) showShop = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            VillageVendor vendor = other.GetComponent<VillageVendor>();
            if (vendor != null) nearbyVendor = vendor;
        }

        private void OnTriggerExit(Collider other)
        {
            VillageVendor vendor = other.GetComponent<VillageVendor>();
            if (vendor != null && vendor == nearbyVendor) { nearbyVendor = null; showShop = false; }
        }

        private void OnGUI()
        {
            if (nearbyVendor == null) return;
            float width = Mathf.Min(520f, Screen.width - 30f);
            float left = (Screen.width - width) * 0.5f;
            GUI.Box(new Rect(left, Screen.height - 105f, width, 75f),
                nearbyVendor.shopName + "\nPress E or tap OPEN SHOP");
            if (GUI.Button(new Rect(left + width - 145f, Screen.height - 78f, 125f, 38f), showShop ? "CLOSE" : "OPEN SHOP"))
                showShop = !showShop;
            if (!showShop) return;

            float panelHeight = Mathf.Min(540f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect(left, (Screen.height - panelHeight) * 0.5f, width, panelHeight), GUI.skin.window);
            GUILayout.Label(nearbyVendor.shopName, GUI.skin.title);
            GUILayout.Label("Coins: " + (wallet != null ? wallet.Coins : 0));
            GUILayout.Label("BUY");
            for (int i = 0; i < nearbyVendor.StockCount; i++)
            {
                InventoryItem item = nearbyVendor.GetStock(i);
                int price = nearbyVendor.GetPrice(i);
                GUILayout.BeginHorizontal();
                GUILayout.Label(item.itemName + "  |  " + item.rarity + "  |  Power " + item.power + "  |  " + price + " coins");
                if (GUILayout.Button("BUY", GUILayout.Width(65f))) Buy(item, price);
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.Label("SELL INVENTORY");
            if (inventory != null)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    InventoryItem item = inventory.Items[i];
                    if (item == null) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(item.itemName + "  |  " + item.rarity + "  |  Power " + item.power);
                    if (GUILayout.Button("SELL " + SellPrice(item), GUILayout.Width(90f))) Sell(i);
                    GUILayout.EndHorizontal();
                }
            }
            if (GUILayout.Button("CLOSE SHOP")) showShop = false;
            GUILayout.EndArea();
        }

        private void Buy(InventoryItem item, int price)
        {
            if (wallet == null || inventory == null || inventory.Count >= 60) return;
            if (!wallet.Spend(price)) return;
            if (!inventory.Add(new InventoryItem(item.itemName, item.rarity, item.power, item.slot))) wallet.Add(price);
        }

        private int SellPrice(InventoryItem item) { return Mathf.Max(5, item.power * 8); }

        private void Sell(int index)
        {
            if (inventory == null || wallet == null || index < 0 || index >= inventory.Count) return;
            InventoryItem item = inventory.Items[index];
            int value = SellPrice(item);
            if (inventory.RemoveAt(index)) wallet.Add(value);
        }
    }
}