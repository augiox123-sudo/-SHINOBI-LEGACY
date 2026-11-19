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

        public bool Add(InventoryItem item)
        {
            if (item == null || items.Count >= capacity) return false;
            items.Add(item);
            ItemAdded?.Invoke(item);
            return true;
        }
    }
}