using UnityEngine;
using System;
using System.Collections.Generic;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Combat
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [Header("Basic attack")]
        [SerializeField] private float attackDamage = 24f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackRadius = 1.15f;
        [SerializeField] private float attackCooldown = 0.55f;

        [Header("Chakra burst")]
        [SerializeField] private float burstDamage = 65f;
        [SerializeField] private float burstRange = 5.5f;
        [SerializeField] private float burstRadius = 1.8f;
        [SerializeField] private float burstCost = 35f;
        [SerializeField] private float burstCooldown = 4.5f;

        [Header("Fire style")]
        [SerializeField] private float fireDamage = 48f;
        [SerializeField] private float fireRange = 9f;
        [SerializeField] private float fireCost = 25f;
        [SerializeField] private float fireCooldown = 3f;

        [Header("Chakra")]
        [SerializeField] private float chakraMaximum = 100f;
        [SerializeField] private float chakraRegeneration = 7f;

        private float nextAttackTime;
        private float nextBurstTime;
        private float nextFireTime;
        private float chakra;
        private readonly Collider[] hitBuffer = new Collider[24];
        private readonly HashSet<CombatantHealth> damagedThisCast = new HashSet<CombatantHealth>();
        private EquipmentManager equipment;
        private CombatantHealth ownHealth;
        [SerializeField, Range(1, 5)] private int burstRank = 1;
        [SerializeField, Range(1, 5)] private int fireRank = 1;
        [SerializeField] private int burstUses;
        [SerializeField] private int fireUses;
        public int BurstRank => burstRank;
        public int FireRank => fireRank;
        public int BurstUses => burstUses;
        public int FireUses => fireUses;

        public float Chakra => chakra;
        public float MaxChakra => chakraMaximum;
        public float BurstCooldownRemaining => Mathf.Max(0f, nextBurstTime - Time.time);
        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public float BurstCooldownDuration => burstCooldown;
        public float FireCooldownDuration => fireCooldown;
        public event Action ResourcesChanged;
        public event Action AbilityProgressChanged;

        private void Awake()
        {
            chakra = chakraMaximum;
            equipment = GetComponent<EquipmentManager>();
            ownHealth = GetComponent<CombatantHealth>();
            ResourcesChanged?.Invoke();
        }

        private void Update()
        {
            float previous = chakra;
            chakra = Mathf.Min(chakraMaximum, chakra + chakraRegeneration * Time.deltaTime);
            if (!Mathf.Approximately(previous, chakra)) ResourcesChanged?.Invoke();

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.Space)) BasicAttack();
            if (Input.GetKeyDown(KeyCode.Q)) CastChakraBurst();
            if (Input.GetKeyDown(KeyCode.E)) CastFireStyle();
#endif
        }

        public void BasicAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackCooldown;
            damagedThisCast.Clear();
            HitTargets(attackRange, attackRadius, GetScaledDamage(attackDamage));
        }

        public void CastChakraBurst()
        {
            if (Time.time < nextBurstTime || chakra < burstCost) return;
            chakra -= burstCost;
            nextBurstTime = Time.time + burstCooldown;
            damagedThisCast.Clear();
            float damage = GetScaledDamage(burstDamage) * (1f + (burstRank - 1) * 0.05f);
            string modifier = equipment != null ? equipment.GetAbilityModifier("burst") : string.Empty;
            if (modifier == "burst_storm")
            {
                HitTargets(burstRange, burstRadius * 1.45f, damage * 0.72f);
                HitTargets(burstRange * 0.82f, burstRadius * 1.2f, damage * 0.42f);
            }
            else
            {
                HitTargets(burstRange, burstRadius, damage);
                if (modifier == "burst_guard" && ownHealth != null)
                    ownHealth.Restore(ownHealth.MaxHealth * 0.18f);
            }
            RecordAbilityUse(true);
            ResourcesChanged?.Invoke();
        }

        public void CastFireStyle()
        {
            if (Time.time < nextFireTime || chakra < fireCost) return;
            chakra -= fireCost;
            nextFireTime = Time.time + fireCooldown;
            damagedThisCast.Clear();
            Vector3 origin = transform.position + Vector3.up * 1.1f + transform.forward * 0.6f;
            Vector3 direction = transform.forward;
            float damage = GetScaledDamage(fireDamage) * (1f + (fireRank - 1) * 0.05f);
            string modifier = equipment != null ? equipment.GetAbilityModifier("fire") : string.Empty;
            if (modifier == "fire_wave")
            {
                for (int step = 1; step <= 3; step++)
                    DamageArea(origin + direction * (step * fireRange / 3f), 2.1f, damage * 0.62f, true);
            }
            else if (modifier == "fire_dragon")
            {
                FireRay(origin - transform.right * 0.8f, direction, damage * 0.82f, 0.8f, true);
                FireRay(origin + transform.right * 0.8f, direction, damage * 0.82f, 0.8f, true);
            }
            else if (modifier == "fire_burst")
            {
                if (Physics.SphereCast(origin, 0.55f, direction, out RaycastHit hit, fireRange,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    DamageArea(hit.point, 3.2f, damage * 1.15f, true);
            }
            else FireRay(origin, direction, damage, 0.55f, false);
            RecordAbilityUse(false);
            ResourcesChanged?.Invoke();
        }


        private void FireRay(Vector3 origin, Vector3 direction, float damage, float radius, bool applyBurn)
        {
            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, fireRange,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                CombatantHealth health = hit.collider.GetComponentInParent<CombatantHealth>();
                if (health != null && health.transform != transform) ApplyDamageOnce(health, damage, applyBurn);
            }
        }

        private void DamageArea(Vector3 center, float radius, float damage, bool applyBurn)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit == null) continue;
                CombatantHealth health = hit.GetComponentInParent<CombatantHealth>();
                if (health != null && !health.IsDead && health.transform != transform) ApplyDamageOnce(health, damage, applyBurn);
                hitBuffer[i] = null;
            }
        }

        private void ApplyDamageOnce(CombatantHealth target, float damage, bool applyBurn)
        {
            if (target == null || target.IsDead || !damagedThisCast.Add(target)) return;
            target.TakeDamage(damage);
            if (applyBurn && !target.IsDead) target.ApplyBurn(Mathf.Max(2f, damage * 0.08f), 3f, 1f);
        }

        private void RecordAbilityUse(bool burst)
        {
            if (burst)
            {
                burstUses++;
                if (burstRank < 5 && burstUses >= burstRank * 20) burstRank++;
            }
            else
            {
                fireUses++;
                if (fireRank < 5 && fireUses >= fireRank * 20) fireRank++;
            }
            AbilityProgressChanged?.Invoke();
        }

        public void RestoreAbilityProgress(int savedBurstRank, int savedFireRank, int savedBurstUses, int savedFireUses)
        {
            burstRank = Mathf.Clamp(savedBurstRank, 1, 5);
            fireRank = Mathf.Clamp(savedFireRank, 1, 5);
            burstUses = Mathf.Max(0, savedBurstUses);
            fireUses = Mathf.Max(0, savedFireUses);
        }

        private float GetScaledDamage(float baseDamage)
        {
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            if (equipment == null) return baseDamage;
            return baseDamage * equipment.DamageMultiplier;
        }

        private void HitTargets(float range, float radius, float damage)
        {
            Vector3 center = transform.position + Vector3.up * 1f + transform.forward * range * 0.55f;
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit == null) continue;
                CombatantHealth health = hit.GetComponentInParent<CombatantHealth>();
                if (health != null && !health.IsDead && health.transform != transform)
                    health.TakeDamage(damage);
                hitBuffer[i] = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up + transform.forward * attackRange * 0.55f, attackRadius);
        }
#endif
    }
}
