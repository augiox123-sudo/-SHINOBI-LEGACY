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
            int bestPower = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].power > bestPower)
                {
                    bestPower = items[i].power;
                    bestIndex = i;
                }
            }
            return EquipAt(bestIndex, equipment);
        }

        public bool EquipAt(int index, EquipmentManager equipment)
        {
            if (equipment == null || index < 0 || index >= items.Count) return false;
            InventoryItem item = items[index];
            if (!equipment.Equip(item)) return false;
            items.RemoveAt(index);
            ItemEquipped?.Invoke(item);
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= items.Count) return false;
            items.RemoveAt(index);
            ItemAdded?.Invoke(null);
            return true;
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