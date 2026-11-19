using UnityEngine;
using ShinobiLegacy.Player;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.Items
{
    [RequireComponent(typeof(CombatantHealth))]
    public sealed class EnemyRewardDrop : MonoBehaviour
    {
        [SerializeField] private int experienceReward = 35;
        [SerializeField] private float dropChance = 0.75f;
        [SerializeField] private bool guaranteedRareChance;
        [SerializeField, Range(0f, 1f)] private float legendaryChanceOverride = -1f;
        private PlayerProgression target;
        private CombatantHealth health;
        private bool rewarded;
        private int bossLootProfile = -1;

        public void Configure(PlayerProgression player, int xp, float chance, bool boss)
        {
            target = player;
            experienceReward = xp;
            dropChance = Mathf.Clamp01(chance);
            guaranteedRareChance = boss;
        }

        public void SetLegendaryChance(float chance)
        {
            legendaryChanceOverride = Mathf.Clamp01(chance);
        }

        public void SetBossLootProfile(int profile)
        {
            bossLootProfile = Mathf.Clamp(profile, 0, 3);
        }

        private void Awake()
        {
            health = GetComponent<CombatantHealth>();
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void OnDied()
        {
            if (rewarded) return;
            rewarded = true;
            if (target != null) target.AddExperience(experienceReward);
            if (Random.value > dropChance) return;

            LootRarity rarity = RollRarity();
            int power = Random.Range(2, 8) + (int)rarity * 7;
            string[] names = { "Ashen Tanto", "Storm Thread Vest", "Warden's Seal", "Moonsteel Kunai", "Cinderweave Wraps" };
            string[] slots = { "Weapon", "Chest", "Accessory", "Weapon", "Hands" };
            int index = Random.Range(0, names.Length);
            InventoryItem item = new InventoryItem(names[index], rarity, power, slots[index]);
            RollSecondaryStats(item);
            if (rarity == LootRarity.Legendary)
            {
                string[] legendaryEffects = {
                    "fire_wave", "fire_dragon", "fire_burst", "fire_meteor",
                    "burst_storm", "burst_guard", "burst_chain",
                    "lightning_chain", "lightning_storm",
                    "water_prison", "water_torrent",
                    "wind_vortex", "wind_blade"
                };
                if (bossLootProfile >= 0)
                    ApplyBossLegendary(item, bossLootProfile);
                else
                {
                    item.abilityModifier = legendaryEffects[Random.Range(0, legendaryEffects.Length)];
                    item.itemName += " [" + GetModifierName(item.abilityModifier) + "]";
                }
            }

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.position = transform.position + Vector3.up * 0.45f;
            orb.transform.localScale = Vector3.one * 0.55f;
            Collider collider = orb.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = orb.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            LootPickup pickup = orb.AddComponent<LootPickup>();
            pickup.Configure(item);
        }

        private static void RollSecondaryStats(InventoryItem item)
        {
            float tier = (int)item.rarity + 1;
            item.bonusDamagePercent = Random.Range(0f, 0.008f * tier);
            item.bonusDefense = Random.Range(0f, 0.75f * tier);
            item.bonusCriticalChance = Random.Range(0f, 0.006f * tier);
            item.bonusCriticalDamage = Random.Range(0f, 0.025f * tier);
            if (item.rarity == LootRarity.Legendary)
            {
                item.bonusDamagePercent += 0.025f;
                item.bonusCriticalChance += 0.015f;
            }
        }

        private static string GetModifierName(string id)
        {
            switch (id)
            {
                case "fire_wave": return "Flame Wave";
                case "fire_dragon": return "Twin Dragons";
                case "fire_burst": return "Inferno Impact";
                case "fire_meteor": return "Meteor Flame";
                case "burst_storm": return "Chakra Tempest";
                case "burst_guard": return "Guardian Pulse";
                case "burst_chain": return "Chain Lightning";
                case "lightning_chain": return "Lightning Chain";
                case "lightning_storm": return "Thunderstorm";
                case "water_prison": return "Water Prison";
                case "water_torrent": return "Water Torrent";
                case "wind_vortex": return "Wind Vortex";
                case "wind_blade": return "Wind Blade";
                default: return "Awakened Technique";
            }
        }

        private static void ApplyBossLegendary(InventoryItem item, int profile)
        {
            string[] names =
            {
                "Ashen Warden's Cinderfang",
                "Rootbound Colossus Heart",
                "Raijin's Tempest Core",
                "Abyssal Guardian's Deepwater Seal"
            };
            string[] modifiers = { "fire_meteor", "wind_vortex", "lightning_storm", "water_prison" };
            string[] slots = { "Weapon", "Chest", "Accessory", "Accessory" };
            item.itemName = names[profile];
            item.abilityModifier = modifiers[profile];
            item.slot = slots[profile];
            item.bonusDamagePercent += 0.035f;
            item.bonusCriticalChance += 0.012f;
            item.bonusCriticalDamage += 0.08f;
            item.bonusDefense += 2f + profile;
        }

        private LootRarity RollRarity()
        {
            float roll = Random.value;
            float legendaryChance = legendaryChanceOverride >= 0f ? legendaryChanceOverride :
                (guaranteedRareChance ? 0.08f : 0.015f);
            // Keep the normal rarity-band widths when a boss-specific legendary rate is configured.
            float epicThreshold = legendaryChance + 0.055f;
            float rareThreshold = legendaryChance + 0.205f;
            float uncommonThreshold = legendaryChance + 0.505f;
            if (roll < legendaryChance) return LootRarity.Legendary;
            if (roll < epicThreshold) return LootRarity.Epic;
            if (roll < rareThreshold) return LootRarity.Rare;
            if (roll < uncommonThreshold) return LootRarity.Uncommon;
            return LootRarity.Common;
        }
    }
}