using UnityEngine;
using System;
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
        private EquipmentManager equipment;

        public float Chakra => chakra;
        public float MaxChakra => chakraMaximum;
        public float BurstCooldownRemaining => Mathf.Max(0f, nextBurstTime - Time.time);
        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public float BurstCooldownDuration => burstCooldown;
        public float FireCooldownDuration => fireCooldown;
        public event Action ResourcesChanged;

        private void Awake()
        {
            chakra = chakraMaximum;
            equipment = GetComponent<EquipmentManager>();
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
            HitTargets(attackRange, attackRadius, GetScaledDamage(attackDamage));
        }

        public void CastChakraBurst()
        {
            if (Time.time < nextBurstTime || chakra < burstCost) return;
            chakra -= burstCost;
            nextBurstTime = Time.time + burstCooldown;
            HitTargets(burstRange, burstRadius, GetScaledDamage(burstDamage));
            ResourcesChanged?.Invoke();
        }

        public void CastFireStyle()
        {
            if (Time.time < nextFireTime || chakra < fireCost) return;
            chakra -= fireCost;
            nextFireTime = Time.time + fireCooldown;
            Vector3 origin = transform.position + Vector3.up * 1.1f + transform.forward * 0.6f;
            Vector3 direction = transform.forward;
            if (Physics.SphereCast(origin, 0.55f, direction, out RaycastHit hit, fireRange,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                CombatantHealth health = hit.collider.GetComponentInParent<CombatantHealth>();
                if (health != null && health.transform != transform)
                    health.TakeDamage(GetScaledDamage(fireDamage));
            }
            ResourcesChanged?.Invoke();
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
