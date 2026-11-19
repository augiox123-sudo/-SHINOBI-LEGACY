using System;
using UnityEngine;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    /// <summary>
    /// Tracks account-wide regional access. Regions unlock automatically when the
    /// character reaches their recommended level; unlocks persist on this device.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldRegionProgression : MonoBehaviour
    {
        [Serializable]
        public sealed class Region
        {
            public string id;
            public string displayName;
            public string description;
            [Min(1)] public int requiredLevel;
            public Region(string id, string name, string description, int level)
            {
                this.id = id;
                displayName = name;
                this.description = description;
                requiredLevel = level;
            }
        }

        private static readonly Region[] RegionCatalog =
        {
            new Region("hidden_village", "Hidden Leaf Enclave", "Safe hub, merchants, academy and crafting.", 1),
            new Region("ashen_ruins", "Ashen Ruins", "Scorched stone corridors and the Ashen Warden.", 5),
            new Region("verdant_forest", "Verdant Forest", "Overgrown trails and the Rootbound Colossus.", 10),
            new Region("thunder_shrine", "Thunder Shrine", "Storm-charged highlands and Raijin's Broken Avatar.", 18),
            new Region("drowned_temple", "Drowned Temple", "Flooded halls and the Abyssal Guardian.", 28)
        };

        private PlayerProgression progression;
        private int lastLevelChecked;
        public event Action RegionsChanged;

        public static Region[] Regions => RegionCatalog;

        private void Awake()
        {
            progression = GetComponent<PlayerProgression>();
            UnlockEligibleRegions();
        }

        private void Update()
        {
            int level = progression != null ? progression.Level : 1;
            if (level == lastLevelChecked) return;
            UnlockEligibleRegions();
        }

        public bool IsUnlocked(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return true;
            return PlayerPrefs.GetInt("SL_REGION_UNLOCKED_" + regionId, 0) == 1;
        }

        public bool TryUnlock(string regionId)
        {
            Region region = Array.Find(RegionCatalog, item => item.id == regionId);
            if (region == null) return false;
            int level = progression != null ? progression.Level : 1;
            if (level < region.requiredLevel) return false;
            string key = "SL_REGION_UNLOCKED_" + region.id;
            if (PlayerPrefs.GetInt(key, 0) == 1) return true;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            RegionsChanged?.Invoke();
            Debug.Log("SHINOBI LEGACY: Region unlocked: " + region.displayName);
            return true;
        }

        private void UnlockEligibleRegions()
        {
            lastLevelChecked = progression != null ? progression.Level : 1;
            foreach (Region region in RegionCatalog)
                if (lastLevelChecked >= region.requiredLevel) TryUnlock(region.id);
        }
    }
}
