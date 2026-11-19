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
        private PlayerProgression target;
        private CombatantHealth health;
        private bool rewarded;

        public void Configure(PlayerProgression player, int xp, float chance, bool boss)
        {
            target = player;
            experienceReward = xp;
            dropChance = Mathf.Clamp01(chance);
            guaranteedRareChance = boss;
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
            if (rarity == LootRarity.Legendary)
            {
                string[] legendaryEffects = { "fire_wave", "fire_dragon", "fire_burst", "fire_meteor", "burst_storm", "burst_guard", "burst_chain" };
                item.abilityModifier = legendaryEffects[Random.Range(0, legendaryEffects.Length)];
                item.itemName += " [" + GetModifierName(item.abilityModifier) + "]";
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
                default: return "Awakened Technique";
            }
        }

        private LootRarity RollRarity()
        {
            float roll = Random.value;
            if (guaranteedRareChance && roll < 0.08f) return LootRarity.Legendary;
            if (roll < 0.015f) return LootRarity.Legendary;
            if (roll < 0.07f) return LootRarity.Epic;
            if (roll < 0.22f) return LootRarity.Rare;
            if (roll < 0.52f) return LootRarity.Uncommon;
            return LootRarity.Common;
        }
    }
}