using UnityEngine;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    /// <summary>One active regional hunt contract with persistent progress and automatic rewards.</summary>
    [DisallowMultipleComponent]
    public sealed class RegionalBountySystem : MonoBehaviour
    {
        private const string RegionKey = "SL_BOUNTY_REGION_V1";
        private const string KillsKey = "SL_BOUNTY_KILLS_V1";
        private const int RequiredKills = 10;

        private string activeRegion;
        private int kills;
        private PlayerWallet wallet;
        private PlayerProgression progression;

        public string ActiveRegion => activeRegion;
        public int Kills => kills;
        public int KillsRequired => RequiredKills;
        public bool HasContract => !string.IsNullOrEmpty(activeRegion);

        private void Awake()
        {
            wallet = GetComponent<PlayerWallet>();
            progression = GetComponent<PlayerProgression>();
            activeRegion = PlayerPrefs.GetString(RegionKey, "");
            kills = Mathf.Clamp(PlayerPrefs.GetInt(KillsKey, 0), 0, RequiredKills);
        }

        private void Start()
        {
            // Wait until every component has completed Awake before awarding a saved completion.
            if (kills >= RequiredKills && HasContract) CompleteContract();
        }

        public bool AcceptContract(string regionId)
        {
            if (string.IsNullOrEmpty(regionId) || HasContract) return false;
            WorldRegionProgression regions = GetComponent<WorldRegionProgression>();
            if (regions == null || !regions.IsUnlocked(regionId)) return false;

            activeRegion = regionId;
            kills = 0;
            Save();
            Debug.Log("SHINOBI LEGACY: Regional bounty accepted: " + regionId);
            return true;
        }

        public void RegisterKill(string regionId)
        {
            if (!HasContract || activeRegion != regionId || kills >= RequiredKills) return;
            kills++;
            Save();
            if (kills >= RequiredKills) CompleteContract();
        }

        private void CompleteContract()
        {
            int level = progression != null ? progression.Level : 1;
            int coinReward = 350 + level * 45;
            int experienceReward = 90 + level * 18;
            if (wallet != null) wallet.Add(coinReward);
            if (progression != null) progression.AddExperience(experienceReward);

            Debug.Log("SHINOBI LEGACY: Bounty completed! +" + coinReward +
                " coins, +" + experienceReward + " XP.");
            activeRegion = "";
            kills = 0;
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetString(RegionKey, activeRegion);
            PlayerPrefs.SetInt(KillsKey, kills);
            PlayerPrefs.Save();
        }
    }
}
