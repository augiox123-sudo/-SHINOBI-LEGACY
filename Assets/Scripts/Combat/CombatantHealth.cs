using UnityEngine;
using System;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Combat
{
    public sealed class CombatantHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        private float currentHealth;
        private EquipmentManager equipment;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }
        public event Action<float, float> HealthChanged;
        public event Action Died;

        public void SetMaxHealth(float value)
        {
            maxHealth = Mathf.Max(1f, value);
            currentHealth = maxHealth;
        }

        private void Awake()
        {
            currentHealth = maxHealth;
            equipment = GetComponent<EquipmentManager>();
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;
            float incomingDamage = amount;
            if (equipment != null) incomingDamage = Mathf.Max(1f, amount - equipment.Defense);
            currentHealth = Mathf.Max(0f, currentHealth - incomingDamage);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) Die();
        }

        public void Restore(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject, 1.2f);
        }
    }
}
