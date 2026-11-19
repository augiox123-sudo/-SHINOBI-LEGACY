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
        private CombatAnimationBridge animationBridge;
        private float invulnerableUntil;
        private Coroutine burnRoutine;
        private CombatantHitReaction hitReaction;
        private float stunnedUntil;
        private float staggeredUntil;
        public bool IsStunned => Time.time < stunnedUntil;
        public bool IsStaggered => Time.time < staggeredUntil;

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
            animationBridge = GetComponent<CombatAnimationBridge>();
            if (animationBridge == null) animationBridge = gameObject.AddComponent<CombatAnimationBridge>();
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f || Time.time < invulnerableUntil) return;
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            if (animationBridge != null) animationBridge.TriggerHit();
            float incomingDamage = amount;
            if (equipment != null) incomingDamage = Mathf.Max(1f, amount - equipment.Defense);
            currentHealth = Mathf.Max(0f, currentHealth - incomingDamage);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) Die();
        }

        public void ReviveAtFullHealth(float invulnerabilitySeconds = 3f)
        {
            IsDead = false;
            if (animationBridge != null) animationBridge.SetDead(false);
            currentHealth = maxHealth;
            invulnerableUntil = Time.time + Mathf.Max(0f, invulnerabilitySeconds);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void GrantTemporaryInvulnerability(float duration)
        {
            if (IsDead) return;
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + Mathf.Max(0f, duration));
        }

        public void ApplyKnockbackFrom(Vector3 sourcePosition, float strength = 3.2f, float duration = 0.14f)
        {
            if (IsDead) return;
            if (hitReaction == null) hitReaction = GetComponent<CombatantHitReaction>();
            if (hitReaction == null) hitReaction = gameObject.AddComponent<CombatantHitReaction>();
            Vector3 direction = transform.position - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = -transform.forward;
            hitReaction.ApplyImpulse(direction, strength, duration);
            if (strength >= 3.8f)
            {
                if (animationBridge != null) animationBridge.TriggerHeavyHit();
            }
            else if (strength >= 2.7f)
            {
                if (animationBridge != null) animationBridge.TriggerMediumHit();
            }
            else
            {
                if (animationBridge != null) animationBridge.TriggerHit();
            }

            // Keep the existing interruption threshold independent from visual reaction tier.
            if (strength >= 2.3f)
            {
                float staggerDuration = Mathf.Clamp(0.12f + strength * 0.055f, 0.16f, 0.34f);
                staggeredUntil = Mathf.Max(staggeredUntil, Time.time + staggerDuration);
            }
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
            if (animationBridge != null) animationBridge.SetDead(true);
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject, 1.2f);
        }
    }
}
