using UnityEngine;
using UnityEngine.UI;

namespace ShinobiLegacy.UI
{
    public sealed class CombatHud : MonoBehaviour
    {
        [SerializeField] private ShinobiLegacy.Combat.CombatantHealth health;
        [SerializeField] private ShinobiLegacy.Combat.PlayerCombat combat;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image chakraFill;
        [SerializeField] private Text statusText;

        public void Configure(
            ShinobiLegacy.Combat.CombatantHealth playerHealth,
            ShinobiLegacy.Combat.PlayerCombat playerCombat,
            Image healthBar,
            Image chakraBar,
            Text status)
        {
            health = playerHealth;
            combat = playerCombat;
            healthFill = healthBar;
            chakraFill = chakraBar;
            statusText = status;
            if (health != null) health.HealthChanged += OnHealthChanged;
            if (combat != null) combat.ResourcesChanged += Refresh;
            Refresh();
        }

        private void OnEnable()
        {
            if (health != null) health.HealthChanged += OnHealthChanged;
            if (combat != null) combat.ResourcesChanged += Refresh;
        }

        private void OnDisable()
        {
            if (health != null) health.HealthChanged -= OnHealthChanged;
            if (combat != null) combat.ResourcesChanged -= Refresh;
        }

        private void Update()
        {
            Refresh();
            if (combat == null) return;
            if (statusText != null)
            {
                string burst = combat.BurstCooldownRemaining > 0f
                    ? combat.BurstCooldownRemaining.ToString("0.0") + "s"
                    : "READY";
                string fire = combat.FireCooldownRemaining > 0f
                    ? combat.FireCooldownRemaining.ToString("0.0") + "s"
                    : "READY";
                EquipmentManager gear = combat.GetComponent<EquipmentManager>();
                string burstMod = gear != null ? gear.GetAbilityModifier("burst") : string.Empty;
                string fireMod = gear != null ? gear.GetAbilityModifier("fire") : string.Empty;
                string lightningMod = gear != null ? gear.GetAbilityModifier("lightning") : string.Empty;
                string waterMod = gear != null ? gear.GetAbilityModifier("water") : string.Empty;
                string windMod = gear != null ? gear.GetAbilityModifier("wind") : string.Empty;
                statusText.text = "BURST R" + combat.BurstRank + ": " + burst +
                    (string.IsNullOrEmpty(burstMod) ? "" : " [" + ModifierLabel(burstMod) + "]") +
                    "    FIRE R" + combat.FireRank + ": " + fire +
                    (string.IsNullOrEmpty(fireMod) ? "" : " [" + ModifierLabel(fireMod) + "]") +
                    "    LIGHTNING R" + combat.LightningRank +
                    (string.IsNullOrEmpty(lightningMod) ? "" : " [" + ModifierLabel(lightningMod) + "]") +
                    "    WATER R" + combat.WaterRank +
                    (string.IsNullOrEmpty(waterMod) ? "" : " [" + ModifierLabel(waterMod) + "]") +
                    "    WIND R" + combat.WindRank +
                    (string.IsNullOrEmpty(windMod) ? "" : " [" + ModifierLabel(windMod) + "]") +
                    (gear != null && gear.LegendarySynergyActive ? "    LEGENDARY SYNERGY +10%" : "");
            }
        }

        private static string ModifierLabel(string id)
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
                default: return id;
            }
        }

        private void OnHealthChanged(float current, float maximum)
        {
            if (healthFill != null)
                healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
        }

        private void Refresh()
        {
            if (healthFill != null && health != null)
                healthFill.fillAmount = health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 0f;
            if (chakraFill != null && combat != null)
                chakraFill.fillAmount = combat.MaxChakra > 0f ? combat.Chakra / combat.MaxChakra : 0f;
        }
    }
}
