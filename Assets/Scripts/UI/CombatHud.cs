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
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.15f;
        [SerializeField, Min(0.1f)] private float statusRefreshInterval = 0.5f;
        private float nextRefreshTime;
        private float nextStatusRefreshTime;
        private EquipmentManager cachedEquipment;
        private bool subscribed;

        public void Configure(
            ShinobiLegacy.Combat.CombatantHealth playerHealth,
            ShinobiLegacy.Combat.PlayerCombat playerCombat,
            Image healthBar,
            Image chakraBar,
            Text status)
        {
            Unsubscribe();
            health = playerHealth;
            combat = playerCombat;
            healthFill = healthBar;
            chakraFill = chakraBar;
            statusText = status;
            cachedEquipment = combat != null ? combat.GetComponent<EquipmentManager>() : null;
            Subscribe();
            Refresh();
            RefreshStatus();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed) return;
            if (health != null) health.HealthChanged += OnHealthChanged;
            if (combat != null) combat.ResourcesChanged += Refresh;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (health != null) health.HealthChanged -= OnHealthChanged;
            if (combat != null) combat.ResourcesChanged -= Refresh;
            subscribed = false;
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefreshTime)
            {
                nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
                Refresh();
            }

            if (Time.unscaledTime >= nextStatusRefreshTime)
            {
                nextStatusRefreshTime = Time.unscaledTime + Mathf.Max(0.1f, statusRefreshInterval);
                RefreshStatus();
            }
        }

        private void RefreshStatus()
        {
            if (combat == null || statusText == null) return;
            if (cachedEquipment == null) cachedEquipment = combat.GetComponent<EquipmentManager>();

            string burst = CooldownLabel(combat.BurstCooldownRemaining);
            string fire = CooldownLabel(combat.FireCooldownRemaining);
            string lightning = CooldownLabel(combat.LightningCooldownRemaining);
            string water = CooldownLabel(combat.WaterCooldownRemaining);
            string wind = CooldownLabel(combat.WindCooldownRemaining);
            string burstMod = cachedEquipment != null ? cachedEquipment.GetAbilityModifier("burst") : string.Empty;
            string fireMod = cachedEquipment != null ? cachedEquipment.GetAbilityModifier("fire") : string.Empty;
            string lightningMod = cachedEquipment != null ? cachedEquipment.GetAbilityModifier("lightning") : string.Empty;
            string waterMod = cachedEquipment != null ? cachedEquipment.GetAbilityModifier("water") : string.Empty;
            string windMod = cachedEquipment != null ? cachedEquipment.GetAbilityModifier("wind") : string.Empty;

            statusText.text = "BURST R" + combat.BurstRank + ": " + burst +
                (string.IsNullOrEmpty(burstMod) ? "" : " [" + ModifierLabel(burstMod) + "]") +
                "    FIRE R" + combat.FireRank + ": " + fire +
                (string.IsNullOrEmpty(fireMod) ? "" : " [" + ModifierLabel(fireMod) + "]") +
                "    LIGHT R" + combat.LightningRank + ": " + lightning +
                (string.IsNullOrEmpty(lightningMod) ? "" : " [" + ModifierLabel(lightningMod) + "]") +
                "\nWATER R" + combat.WaterRank + ": " + water +
                (string.IsNullOrEmpty(waterMod) ? "" : " [" + ModifierLabel(waterMod) + "]") +
                "    WIND R" + combat.WindRank + ": " + wind +
                (string.IsNullOrEmpty(windMod) ? "" : " [" + ModifierLabel(windMod) + "]") +
                (cachedEquipment != null && cachedEquipment.LegendarySynergyActive ? "    |    LEGENDARY SYNERGY +10%" : "");
        }

        private static string CooldownLabel(float remaining)
        {
            return remaining > 0f ? remaining.ToString("0.0") + "s" : "READY";
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