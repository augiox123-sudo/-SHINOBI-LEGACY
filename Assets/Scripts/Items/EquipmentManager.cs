using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class EquipmentManager : MonoBehaviour
    {
        private readonly Dictionary<string, InventoryItem> equipped = new Dictionary<string, InventoryItem>();
        public event Action EquipmentChanged;
        public int TotalPower
        {
            get
            {
                int total = 0;
                foreach (InventoryItem item in equipped.Values) if (item != null) total += item.power;
                return total;
            }
        }

        public InventoryItem GetEquipped(string slot)
        {
            InventoryItem item;
            return equipped.TryGetValue(slot, out item) ? item : null;
        }

        public bool Equip(InventoryItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.slot)) return false;
            if (item.slot != "Weapon" && item.slot != "Chest" && item.slot != "Accessory" && item.slot != "Hands")
                return false;
            equipped[item.slot] = item;
            EquipmentChanged?.Invoke();
            return true;
        }

        public float DamageMultiplier
        {
            get
            {
                InventoryItem weapon = GetEquipped("Weapon");
                return 1f + (weapon != null ? weapon.power * 0.0125f : 0f);
            }
        }

        public float Defense
        {
            get
            {
                float value = 0f;
                foreach (InventoryItem item in equipped.Values)
                    if (item != null && item.slot != "Weapon") value += item.power * 0.08f;
                return value;
            }
        }
    }
}