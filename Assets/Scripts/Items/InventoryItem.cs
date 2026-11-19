using System;
using UnityEngine;

namespace ShinobiLegacy.Items
{
    public enum LootRarity { Common, Uncommon, Rare, Epic, Legendary }

    [Serializable]
    public sealed class InventoryItem
    {
        public string itemName;
        public LootRarity rarity;
        public int power;
        public string slot;
        // Optional legendary skill modifier id, e.g. fire_wave or burst_storm.
        public string abilityModifier;

        public InventoryItem(string name, LootRarity itemRarity, int itemPower, string itemSlot)
        {
            itemName = name;
            rarity = itemRarity;
            power = itemPower;
            slot = itemSlot;
            abilityModifier = string.Empty;
        }
    }
}