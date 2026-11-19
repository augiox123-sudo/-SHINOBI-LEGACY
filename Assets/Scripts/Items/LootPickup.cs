using UnityEngine;

namespace ShinobiLegacy.Items
{
    [RequireComponent(typeof(Collider))]
    public sealed class LootPickup : MonoBehaviour
    {
        private InventoryItem item;
        private bool collected;

        public void Configure(InventoryItem lootItem)
        {
            item = lootItem;
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = RarityColor(lootItem.rarity);
            gameObject.name = "Loot_" + lootItem.itemName;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null || !inventory.Add(item)) return;
            collected = true;
            Destroy(gameObject);
        }

        private static Color RarityColor(LootRarity rarity)
        {
            switch (rarity)
            {
                case LootRarity.Uncommon: return new Color(0.2f, 0.85f, 0.35f);
                case LootRarity.Rare: return new Color(0.2f, 0.55f, 1f);
                case LootRarity.Epic: return new Color(0.72f, 0.3f, 1f);
                case LootRarity.Legendary: return new Color(1f, 0.58f, 0.12f);
                default: return new Color(0.78f, 0.78f, 0.78f);
            }
        }
    }
}