using System;
using UnityEngine;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Player;

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
            public int burstRank = 1;
            public int fireRank = 1;
            public int burstUses;
            public int fireUses;
            public int lightningRank = 1;
            public int waterRank = 1;
            public int windRank = 1;
            public int lightningUses;
            public int waterUses;
            public int windUses;
            public int playerLevel = 1;
            public int playerExperience;
        }

        private PlayerWallet wallet;
        private PlayerInventory inventory;
        private EquipmentManager equipment;
        private PlayerCombat playerCombat;
        private PlayerProgression progression;
        private bool ready;
        [SerializeField] private float autosaveInterval = 30f;
        private float autosaveTimer;

        private void Start()
        {
            wallet = GetComponent<PlayerWallet>();
            inventory = GetComponent<PlayerInventory>();
            equipment = GetComponent<EquipmentManager>();
            playerCombat = GetComponent<PlayerCombat>();
            progression = GetComponent<PlayerProgression>();

            Load();
            if (wallet != null) wallet.CoinsChanged += Save;
            if (inventory != null) inventory.ItemAdded += OnInventoryChanged;
            if (inventory != null) inventory.ItemEquipped += OnItemEquipped;
            if (equipment != null) equipment.EquipmentChanged += Save;
            if (playerCombat != null) playerCombat.AbilityProgressChanged += Save;
            if (progression != null) progression.ProgressChanged += OnProgressChanged;
            ready = true;
        }

        private void OnProgressChanged(int level, int experience, int required)
        {
            if (ready) Save();
        }

        private void OnInventoryChanged(InventoryItem item)
        {
            if (ready) Save();
        }

        private void OnItemEquipped(InventoryItem item)
        {
            if (ready) Save();
        }

        private void Update()
        {
            if (!ready) return;
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= Mathf.Max(5f, autosaveInterval))
            {
                autosaveTimer = 0f;
                Save();
            }
        }

        public void Save()
        {
            if (!ready) return;
            SaveData data = new SaveData
            {
                coins = wallet != null ? wallet.Coins : 0,
                inventory = inventory != null ? Copy(inventory.Items) : new InventoryItem[0],
                equipped = equipment != null ? equipment.GetEquippedItems() : new InventoryItem[0],
                burstRank = playerCombat != null ? playerCombat.BurstRank : 1,
                fireRank = playerCombat != null ? playerCombat.FireRank : 1,
                burstUses = playerCombat != null ? playerCombat.BurstUses : 0,
                fireUses = playerCombat != null ? playerCombat.FireUses : 0,
                lightningRank = playerCombat != null ? playerCombat.LightningRank : 1,
                waterRank = playerCombat != null ? playerCombat.WaterRank : 1,
                windRank = playerCombat != null ? playerCombat.WindRank : 1,
                playerLevel = progression != null ? progression.Level : 1,
                playerExperience = progression != null ? progression.Experience : 0
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
                if (playerCombat != null) playerCombat.RestoreAbilityProgress(data.burstRank, data.fireRank, data.burstUses, data.fireUses,
                    data.lightningRank, data.waterRank, data.windRank, data.lightningUses, data.waterUses, data.windUses);
                if (progression != null) progression.RestoreProgress(data.playerLevel, data.playerExperience);
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
                    result[i] = new InventoryItem(item.itemName, item.rarity, item.power, item.slot) { abilityModifier = item.abilityModifier };
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
            if (playerCombat != null) playerCombat.AbilityProgressChanged -= Save;
            if (progression != null) progression.ProgressChanged -= OnProgressChanged;
        }
    }
}
