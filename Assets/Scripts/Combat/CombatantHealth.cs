using UnityEngine;
using System;
using System.Collections;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Combat
{
    public sealed class CombatantHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        private float currentHealth;
        private EquipmentManager equipment;
        private float invulnerableUntil;
        private Coroutine burnRoutine;
        private float stunnedUntil;
        public bool IsStunned => Time.time < stunnedUntil;

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
            if (IsDead || amount <= 0f || Time.time < invulnerableUntil) return;
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            float incomingDamage = amount;
            if (equipment != null) incomingDamage = Mathf.Max(1f, amount - equipment.Defense);
            currentHealth = Mathf.Max(0f, currentHealth - incomingDamage);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) Die();
        }

        public void ReviveAtFullHealth(float invulnerabilitySeconds = 3f)
        {
            IsDead = false;
            currentHealth = maxHealth;
            invulnerableUntil = Time.time + Mathf.Max(0f, invulnerabilitySeconds);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ApplyStun(float duration)
        {
            if (!IsDead) stunnedUntil = Mathf.Max(stunnedUntil, Time.time + Mathf.Max(0f, duration));
        }

        public void ApplyBurn(float damagePerTick, float duration, float tickInterval = 1f)
        {
            if (IsDead || damagePerTick <= 0f || duration <= 0f) return;
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = StartCoroutine(BurnOverTime(damagePerTick, duration, Mathf.Max(0.25f, tickInterval)));
        }

        private IEnumerator BurnOverTime(float damagePerTick, float duration, float tickInterval)
        {
            float elapsed = 0f;
            while (elapsed < duration && !IsDead)
            {
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
                if (!IsDead) TakeDamage(damagePerTick);
            }
            burnRoutine = null;
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
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject, 1.2f);
        }
    }
}
