using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class VillageVendor : MonoBehaviour
    {
        public string shopName = "Village Merchant";
        public string category = "General Goods";
        public Color markerColor = Color.yellow;
        public bool IsOpen { get; private set; }
        public void SetOpen(bool value) { IsOpen = value; }
        public int StockCount => 4;
        public InventoryItem GetStock(int index)
        {
            string slot = category.Contains("Weapon") ? "Weapon" :
                category.Contains("Armor") ? "Chest" :
                category.Contains("Tool") ? "Hands" :
                category.Contains("Scroll") ? "Accessory" :
                category.Contains("Rare") ? "Accessory" : "Hands";
            string[] names = category.Contains("Weapon")
                ? new[] { "Village Steel Tanto", "Tempered Kunai", "Twin Fang Blade", "Chakra Edge" }
                : category.Contains("Armor")
                ? new[] { "Reinforced Vest", "Forest Scout Jacket", "Iron Thread Coat", "Chakra Guard Robe" }
                : category.Contains("Scroll")
                ? new[] { "Wind Technique Scroll", "Flame Technique Scroll", "Water Technique Scroll", "Earth Technique Scroll" }
                : category.Contains("Food")
                ? new[] { "Ramen Meal", "Soldier Pill", "Herbal Tonic", "Travel Rations" }
                : category.Contains("Rare")
                ? new[] { "Refined Chakra Crystal", "Ancient Alloy", "Beast Core", "Sealed Relic" }
                : new[] { "Smoke Bomb Pack", "Wire Kit", "Field Supplies", "Kunai Bundle" };
            int power = 4 + index * 3;
            return new InventoryItem(names[index], index == 3 ? LootRarity.Rare : LootRarity.Common, power, slot);
        }
        public int GetPrice(int index) { return 60 + index * 45; }
    }
}