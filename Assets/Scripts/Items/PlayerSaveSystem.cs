using System;
using UnityEngine;

namespace ShinobiLegacy.Items
{
    /// <summary>
    /// Lightweight local save for the village prototype. Stores currency, inventory and
    /// equipped items in PlayerPrefs JSON. This is a single-device prototype save, not
    /// an authoritative or cloud-backed account save.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerSaveSystem : MonoBehaviour
    {
        private const string SaveKey = "SHINOBI_LEGACY_PLAYER_SAVE_V1";

        [Serializable]
        private sealed class SaveData
        {
            public int coins;
            public InventoryItem[] inventory;
            public InventoryItem[] equipped;
        }

        private PlayerWallet wallet;
        private PlayerInventory inventory;
        private EquipmentManager equipment;
        private bool ready;

        private void Start()
        {
            wallet = GetComponent<PlayerWallet>();
            inventory = GetComponent<PlayerInventory>();
            equipment = GetComponent<EquipmentManager>();

            Load();
            if (wallet != null) wallet.CoinsChanged += Save;
            if (inventory != null) inventory.ItemAdded += OnInventoryChanged;
            if (inventory != null) inventory.ItemEquipped += OnItemEquipped;
            if (equipment != null) equipment.EquipmentChanged += Save;
            ready = true;
        }

        private void OnInventoryChanged(InventoryItem item)
        {
            if (ready) Save();
        }

        private void OnItemEquipped(InventoryItem item)
        {
            if (ready) Save();
        }

        public void Save()
        {
            if (!ready) return;
            SaveData data = new SaveData
            {
                coins = wallet != null ? wallet.Coins : 0,
                inventory = inventory != null ? Copy(inventory.Items) : new InventoryItem[0],
                equipped = equipment != null ? equipment.GetEquippedItems() : new InventoryItem[0]
            };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
                if (data == null) return;
                if (wallet != null) wallet.SetCoins(data.coins);
                if (inventory != null) inventory.RestoreItems(data.inventory);
                if (equipment != null) equipment.RestoreEquipment(data.equipped);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("SHINOBI LEGACY: Save data could not be loaded. " + exception.Message);
            }
        }

        private static InventoryItem[] Copy(System.Collections.Generic.IReadOnlyList<InventoryItem> source)
        {
            InventoryItem[] result = new InventoryItem[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                InventoryItem item = source[i];
                if (item != null)
                    result[i] = new InventoryItem(item.itemName, item.rarity, item.power, item.slot);
            }
            return result;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnDestroy()
        {
            if (wallet != null) wallet.CoinsChanged -= Save;
            if (inventory != null) inventory.ItemAdded -= OnInventoryChanged;
            if (inventory != null) inventory.ItemEquipped -= OnItemEquipped;
            if (equipment != null) equipment.EquipmentChanged -= Save;
        }
    }
}
