using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.UI
{
    public sealed class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private CombatantHealth bossHealth;
        [SerializeField] private Image fill;
        [SerializeField] private GameObject root;

        public void Configure(CombatantHealth health, Image bar, GameObject container)
        {
            bossHealth = health;
            fill = bar;
            root = container;
            if (bossHealth != null) bossHealth.HealthChanged += UpdateBar;
            Refresh();
        }

        private void OnDestroy()
        {
            if (bossHealth != null) bossHealth.HealthChanged -= UpdateBar;
        }

        private void Update()
        {
            if (bossHealth != null && root != null)
                root.SetActive(!bossHealth.IsDead && bossHealth.CurrentHealth < bossHealth.MaxHealth);
        }

        private void UpdateBar(float current, float maximum)
        {
            if (fill != null) fill.fillAmount = maximum > 0f ? current / maximum : 0f;
        }

        private void Refresh()
        {
            if (fill != null && bossHealth != null)
                fill.fillAmount = bossHealth.MaxHealth > 0f ? bossHealth.CurrentHealth / bossHealth.MaxHealth : 0f;
        }
    }
}
