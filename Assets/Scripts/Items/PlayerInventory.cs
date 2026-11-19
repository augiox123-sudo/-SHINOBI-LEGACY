using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int capacity = 60;
        private readonly List<InventoryItem> items = new List<InventoryItem>();
        public IReadOnlyList<InventoryItem> Items => items;
        public int Count => items.Count;
        public event Action<InventoryItem> ItemAdded;
        public event Action<InventoryItem> ItemEquipped;

        public bool EquipBest(EquipmentManager equipment)
        {
            if (equipment == null || items.Count == 0) return false;

            int bestIndex = -1;
            int bestUpgrade = 0;
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem candidate = items[i];
                if (candidate == null || string.IsNullOrEmpty(candidate.slot)) continue;

                InventoryItem current = equipment.GetEquipped(candidate.slot);
                int upgrade = candidate.power - (current != null ? current.power : 0);
                if (upgrade > bestUpgrade)
                {
                    bestUpgrade = upgrade;
                    bestIndex = i;
                }
            }

            // Never replace a stronger item with a weaker one.
            return bestIndex >= 0 && EquipAt(bestIndex, equipment);
        }

        public bool EquipAt(int index, EquipmentManager equipment)
        {
            if (equipment == null || index < 0 || index >= items.Count) return false;
            InventoryItem item = items[index];
            InventoryItem previous = equipment.GetEquipped(item != null ? item.slot : string.Empty);
            if (!equipment.Equip(item)) return false;
            items.RemoveAt(index);
            if (previous != null) items.Add(previous);
            ItemEquipped?.Invoke(item);
            return true;
        }

        public bool UpgradeAt(int index, int powerIncrease, float damageIncrease, float defenseIncrease, float critIncrease, float critDamageIncrease)
        {
            if (index < 0 || index >= items.Count) return false;
            InventoryItem item = items[index];
            if (item == null || item.upgradeLevel >= 10) return false;
            item.power += Mathf.Max(1, powerIncrease);
            item.bonusDamagePercent += Mathf.Max(0f, damageIncrease);
            item.bonusDefense += Mathf.Max(0f, defenseIncrease);
            item.bonusCriticalChance += Mathf.Max(0f, critIncrease);
            item.bonusCriticalDamage += Mathf.Max(0f, critDamageIncrease);
            item.upgradeLevel++;
            ItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= items.Count) return false;
            items.RemoveAt(index);
            ItemAdded?.Invoke(null);
            return true;
        }

        public void RestoreItems(IEnumerable<InventoryItem> restoredItems)
        {
            items.Clear();
            if (restoredItems == null) return;
            foreach (InventoryItem item in restoredItems)
            {
                if (item != null && items.Count < capacity) items.Add(item);
            }
            ItemAdded?.Invoke(null);
        }

        public bool Add(InventoryItem item)
        {
            if (item == null || items.Count >= capacity) return false;
            items.Add(item);
            ItemAdded?.Invoke(item);
            return true;
        }
    }
}