using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Player;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.UI
{
    public sealed class ProgressionHud : MonoBehaviour
    {
        private PlayerProgression progression;
        private PlayerInventory inventory;
        private EquipmentManager equipment;
        private Text status;

        public void Configure(PlayerProgression playerProgression, PlayerInventory playerInventory, Text label, EquipmentManager equipmentManager)
        {
            progression = playerProgression;
            inventory = playerInventory;
            status = label;
            equipment = equipmentManager;
            Refresh();
            if (progression != null) progression.ProgressChanged += OnChanged;
            if (inventory != null)
            {
                inventory.ItemAdded += OnItemChanged;
                inventory.ItemEquipped += OnItemChanged;
            }
            if (equipment != null) equipment.EquipmentChanged += OnEquipmentChanged;
        }

        private void OnDestroy()
        {
            if (progression != null) progression.ProgressChanged -= OnChanged;
            if (inventory != null)
            {
                inventory.ItemAdded -= OnItemChanged;
                inventory.ItemEquipped -= OnItemChanged;
            }
            if (equipment != null) equipment.EquipmentChanged -= OnEquipmentChanged;
        }

        private void OnChanged(int level, int xp, int required) => Refresh();
        private void OnItemChanged(InventoryItem item) => Refresh();
        private void OnEquipmentChanged() => Refresh();

        private void Refresh()
        {
            if (status == null || progression == null) return;
            int count = inventory != null ? inventory.Count : 0;
            int power = equipment != null ? equipment.TotalPower : 0;
            float crit = equipment != null ? equipment.CriticalChance * 100f : 0f;
            float bonusDamage = equipment != null ? equipment.BonusDamagePercent * 100f : 0f;
            float defense = equipment != null ? equipment.Defense : 0f;
            status.text = "LEVEL " + progression.Level + "   XP " + progression.Experience + "/" +
                progression.ExperienceToNextLevel + "   ITEMS " + count + "   GEAR POWER " + power +
                "   BONUS DMG +" + bonusDamage.ToString("0.0") + "%" +
                "   CRIT " + crit.ToString("0.0") + "%" +
                "   DEF " + defense.ToString("0");
        }
    }
}