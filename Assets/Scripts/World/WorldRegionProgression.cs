using System;
using UnityEngine;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    /// <summary>
    /// Persistent character-level gates for the game's four expedition regions.
    /// Unlocks are local to the current save/device and are evaluated on level-up.
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
            public string dungeonName;
            public string bossName;

            public Region(string id, string name, string description, int level, string dungeon, string boss)
            {
                this.id = id;
                displayName = name;
                this.description = description;
                requiredLevel = level;
                dungeonName = dungeon;
                bossName = boss;
            }
        }

        private static readonly Region[] RegionCatalog =
        {
            new Region("hidden_village", "Hidden Leaf Enclave", "Safe hub, merchants, academy and crafting.", 1, "Village Hub", "—"),
            new Region("ashen_ruins", "Ashen Ruins", "Scorched stone corridors and dangerous fire-touched enemies.", 5, "Ashen Ruins", "The Ashen Warden"),
            new Region("verdant_forest", "Verdant Forest", "Overgrown trails, ancient roots and relentless beasts.", 10, "Verdant Forest", "The Rootbound Colossus"),
            new Region("thunder_shrine", "Thunder Shrine", "Storm-charged highlands and lightning guardians.", 18, "Thunder Shrine", "Raijin's Broken Avatar"),
            new Region("drowned_temple", "Drowned Temple", "Flooded halls, drowned warriors and deep-water curses.", 28, "Drowned Temple", "The Abyssal Guardian")
        };

        private PlayerProgression progression;
        private int lastLevelChecked = -1;
        private string latestUnlockMessage = "";
        private float unlockMessageUntil;
        public event Action RegionsChanged;

        public static Region[] Regions => RegionCatalog;
        public string LatestUnlockMessage => Time.unscaledTime < unlockMessageUntil ? latestUnlockMessage : "";

        private void Awake()
        {
            progression = GetComponent<PlayerProgression>();
            UnlockEligibleRegions();
        }

        private void Update()
        {
            int level = progression != null ? progression.Level : 1;
            if (level != lastLevelChecked) UnlockEligibleRegions();
        }

        public bool IsUnlocked(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return true;
            return PlayerPrefs.GetInt("SL_REGION_UNLOCKED_" + regionId, 0) == 1;
        }

        public int GetCurrentLevel()
        {
            return progression != null ? Mathf.Max(1, progression.Level) : 1;
        }

        public int GetLevelsUntilUnlock(string regionId)
        {
            Region region = Array.Find(RegionCatalog, item => item.id == regionId);
            return region == null || IsUnlocked(regionId) ? 0 : Mathf.Max(0, region.requiredLevel - GetCurrentLevel());
        }

        public bool TryUnlock(string regionId)
        {
            Region region = Array.Find(RegionCatalog, item => item.id == regionId);
            if (region == null) return false;
            if (GetCurrentLevel() < region.requiredLevel) return false;

            string key = "SL_REGION_UNLOCKED_" + region.id;
            if (PlayerPrefs.GetInt(key, 0) == 1) return true;

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            latestUnlockMessage = "NEW REGION UNLOCKED: " + region.displayName;
            unlockMessageUntil = Time.unscaledTime + 6f;
            RegionsChanged?.Invoke();
            Debug.Log("SHINOBI LEGACY: " + latestUnlockMessage);
            return true;
        }

        private void UnlockEligibleRegions()
        {
            lastLevelChecked = GetCurrentLevel();
            foreach (Region region in RegionCatalog)
                TryUnlock(region.id);
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(LatestUnlockMessage)) return;
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.width / 48, 18, 30),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 1f, 0.78f) }
            };
            GUI.Box(new Rect(Screen.width * 0.2f, 86f, Screen.width * 0.6f, 54f), LatestUnlockMessage, style);
        }
    }
}
