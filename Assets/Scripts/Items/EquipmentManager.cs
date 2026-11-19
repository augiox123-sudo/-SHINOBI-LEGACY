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

        public InventoryItem[] GetEquippedItems()
        {
            List<InventoryItem> snapshot = new List<InventoryItem>();
            foreach (InventoryItem item in equipped.Values)
                if (item != null) snapshot.Add(item);
            return snapshot.ToArray();
        }

        public void RestoreEquipment(IEnumerable<InventoryItem> restoredItems)
        {
            equipped.Clear();
            if (restoredItems != null)
                foreach (InventoryItem item in restoredItems)
                    if (item != null && !string.IsNullOrEmpty(item.slot))
                        Equip(item);
            EquipmentChanged?.Invoke();
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

        public string GetAbilityModifier(string abilityId)
        {
            foreach (InventoryItem item in equipped.Values)
                if (item != null && item.rarity == LootRarity.Legendary &&
                    !string.IsNullOrEmpty(item.abilityModifier) &&
                    item.abilityModifier.StartsWith(abilityId + "_"))
                    return item.abilityModifier;
            return string.Empty;
        }

        public bool LegendarySynergyActive
        {
            get
            {
                int legendaryAffixes = 0;
                foreach (InventoryItem item in equipped.Values)
                    if (item != null && item.rarity == LootRarity.Legendary &&
                        !string.IsNullOrEmpty(item.abilityModifier)) legendaryAffixes++;
                return legendaryAffixes >= 2;
            }
        }

        public float LegendarySynergyDamageMultiplier => LegendarySynergyActive ? 1.10f : 1f;

        public bool HasAbilityModifier(string modifierId)
        {
            foreach (InventoryItem item in equipped.Values)
                if (item != null && item.rarity == LootRarity.Legendary &&
                    item.abilityModifier == modifierId) return true;
            return false;
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