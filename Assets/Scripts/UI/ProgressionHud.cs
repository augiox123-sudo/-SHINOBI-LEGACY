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
        private Text status;

        public void Configure(PlayerProgression playerProgression, PlayerInventory playerInventory, Text label)
        {
            progression = playerProgression;
            inventory = playerInventory;
            status = label;
            Refresh();
            if (progression != null) progression.ProgressChanged += OnProgressChanged;
            if (inventory != null) inventory.ItemAdded += OnItemAdded;
        }

        private void OnDestroy()
        {
            if (progression != null) progression.ProgressChanged -= OnProgressChanged;
            if (inventory != null) inventory.ItemAdded -= OnItemAdded;
        }

        private void OnProgressChanged(int level, int xp, int required) => Refresh();
        private void OnItemAdded(InventoryItem item) => Refresh();

        private void Refresh()
        {
            if (status == null || progression == null) return;
            int count = inventory != null ? inventory.Count : 0;
            status.text = "LEVEL " + progression.Level + "   XP " + progression.Experience + "/" +
                progression.ExperienceToNextLevel + "   ITEMS " + count;
        }
    }
}