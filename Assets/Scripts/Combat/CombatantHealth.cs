using UnityEngine;
using System;

namespace ShinobiLegacy.Combat
{
    public sealed class CombatantHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        private float currentHealth;

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

        private void Awake() => currentHealth = maxHealth;

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
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
