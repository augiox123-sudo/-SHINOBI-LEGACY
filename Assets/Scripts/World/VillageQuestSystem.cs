using UnityEngine;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    /// <summary>Four persistent regional story contracts issued by the village mission officer.</summary>
    [DisallowMultipleComponent]
    public sealed class VillageQuestSystem : MonoBehaviour
    {
        private const string QuestIndexKey = "SL_STORY_QUEST_INDEX_V1";
        private const string ActiveKey = "SL_STORY_QUEST_ACTIVE_V1";
        private const string ProgressKey = "SL_STORY_QUEST_PROGRESS_V1";

        private static readonly string[] QuestRegions =
        {
            "ashen_ruins", "verdant_forest", "thunder_shrine", "drowned_temple"
        };
        private static readonly string[] QuestNames =
        {
            "Ashes on the Wind", "Roots Beneath the Canopy", "Silence of the Storm", "The Drowned Oath"
        };
        private static readonly string[] QuestDescriptions =
        {
            "Defeat 6 enemies in the Ashen Ruins patrols.",
            "Defeat 6 enemies in the Verdant Forest patrols.",
            "Defeat 8 enemies in the Thunder Shrine patrols.",
            "Defeat the elite champion in the Drowned Temple."
        };
        private static readonly int[] RequiredKills = { 6, 6, 8, 1 };

        private int questIndex;
        private int progress;
        private bool active;
        private bool boardVisible;
        private PlayerWallet wallet;
        private PlayerProgression progression;
        private WorldRegionProgression regions;
        private PlayerInventory inventory;

        public bool BoardVisible => boardVisible;
        public int QuestIndex => questIndex;
        public int Progress => progress;
        public bool IsActive => active;

        private void Awake()
        {
            wallet = GetComponent<PlayerWallet>();
            progression = GetComponent<PlayerProgression>();
            regions = GetComponent<WorldRegionProgression>();
            inventory = GetComponent<PlayerInventory>();
            questIndex = Mathf.Clamp(PlayerPrefs.GetInt(QuestIndexKey, 0), 0, QuestRegions.Length);
            active = PlayerPrefs.GetInt(ActiveKey, 0) == 1;
            progress = Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey, 0), 0, questIndex < RequiredKills.Length ? RequiredKills[questIndex] : 0);
        }

        public void SetBoardVisible(bool visible) => boardVisible = visible;

        public void AcceptCurrentQuest()
        {
            if (active || questIndex >= QuestRegions.Length) return;
            if (regions == null || !regions.IsUnlocked(QuestRegions[questIndex])) return;
            active = true;
            progress = 0;
            Save();
        }

        public void RegisterEnemyDefeated(string regionId, bool elite)
        {
            if (!active || questIndex >= QuestRegions.Length || regionId != QuestRegions[questIndex]) return;
            if (questIndex == 3 && !elite) return;
            progress = Mathf.Min(RequiredKills[questIndex], progress + 1);
            Save();
            if (progress >= RequiredKills[questIndex]) CompleteCurrentQuest();
        }

        private void CompleteCurrentQuest()
        {
            int level = progression != null ? progression.Level : 1;
            int coins = 500 + questIndex * 250 + level * 35;
            int xp = 140 + questIndex * 90 + level * 22;
            if (wallet != null) wallet.Add(coins);
            if (progression != null) progression.AddExperience(xp);
            GrantStoryRelic(questIndex, level);

            Debug.Log("SHINOBI LEGACY: Story quest complete: " + QuestNames[questIndex] +
                " | +" + coins + " coins | +" + xp + " XP | story relic granted");
            questIndex++;
            progress = 0;
            active = false;
            Save();
        }

        private void GrantStoryRelic(int completedQuest, int level)
        {
            string[] names =
            {
                "Ashen Warden's Ember Seal",
                "Rootbound Heartwood Charm",
                "Raijin's Storm Conduit",
                "Abyssal Oath — Tidebound Relic"
            };
            string[] modifiers = { "fire_meteor", "wind_vortex", "lightning_storm", "water_prison" };
            string[] slots = { "Accessory", "Chest", "Weapon", "Accessory" };
            int power = 18 + completedQuest * 12 + level * 2;
            InventoryItem relic = new InventoryItem(names[completedQuest], LootRarity.Legendary, power, slots[completedQuest])
            {
                abilityModifier = modifiers[completedQuest],
                bonusDamagePercent = 0.035f + completedQuest * 0.005f,
                bonusDefense = 1.5f + completedQuest,
                bonusCriticalChance = 0.018f,
                bonusCriticalDamage = 0.06f
            };

            if (inventory != null && inventory.Add(relic)) return;

            // Full inventory fallback: drop the guaranteed relic beside the player.
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "QuestReward_" + names[completedQuest].Replace(" ", "_");
            orb.transform.position = transform.position + Vector3.up * 0.65f;
            orb.transform.localScale = Vector3.one * 0.8f;
            Collider pickupCollider = orb.GetComponent<Collider>();
            pickupCollider.isTrigger = true;
            Rigidbody body = orb.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            orb.AddComponent<LootPickup>().Configure(relic);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(QuestIndexKey, questIndex);
            PlayerPrefs.SetInt(ActiveKey, active ? 1 : 0);
            PlayerPrefs.SetInt(ProgressKey, progress);
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            if (!boardVisible) return;
            float width = Mathf.Min(540f, Screen.width - 32f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.18f, width, 300f);
            GUI.Box(panel, "VILLAGE MISSION OFFICE");

            GUILayout.BeginArea(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, panel.height - 48f));
            if (questIndex >= QuestRegions.Length)
            {
                GUILayout.Label("All current regional story contracts are complete.");
            }
            else
            {
                GUILayout.Label("CONTRACT " + (questIndex + 1) + " / " + QuestRegions.Length + "  |  " + QuestNames[questIndex]);
                GUILayout.Label(QuestDescriptions[questIndex]);
                int requiredLevel = WorldRegionProgression.Regions[questIndex + 1].requiredLevel;
                bool unlocked = regions != null && regions.IsUnlocked(QuestRegions[questIndex]);
                if (active)
                    GUILayout.Label("PROGRESS: " + progress + " / " + RequiredKills[questIndex]);
                else if (!unlocked)
                    GUILayout.Label("LOCKED: Reach level " + requiredLevel + " to unlock this region.");
                else if (GUILayout.Button("ACCEPT STORY CONTRACT", GUILayout.Height(48f)))
                    AcceptCurrentQuest();
            }
            if (GUILayout.Button("CLOSE", GUILayout.Height(40f))) boardVisible = false;
            GUILayout.EndArea();
        }
    }
}
