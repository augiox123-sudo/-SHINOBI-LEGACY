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
        private float nextLightningTime;
        private float nextWaterTime;
        private float nextWindTime;
        private float chakra;
        private readonly Collider[] hitBuffer = new Collider[24];
        private readonly HashSet<CombatantHealth> damagedThisCast = new HashSet<CombatantHealth>();
        private readonly Collider[] chainBuffer = new Collider[24];
        private EquipmentManager equipment;
        private CombatantHealth ownHealth;
        private AbilityVfxController abilityVfx;
        private EnemyTargetLockSystem targetLock;
        [SerializeField, Range(1, 5)] private int burstRank = 1;
        [SerializeField, Range(1, 5)] private int fireRank = 1;
        [SerializeField] private int burstUses;
        [SerializeField] private int fireUses;
        [SerializeField, Range(1, 5)] private int lightningRank = 1;
        [SerializeField, Range(1, 5)] private int waterRank = 1;
        [SerializeField, Range(1, 5)] private int windRank = 1;
        [SerializeField] private int lightningUses;
        [SerializeField] private int waterUses;
        [SerializeField] private int windUses;
        public int LightningRank => lightningRank;
        public int WaterRank => waterRank;
        public int WindRank => windRank;
        public int LightningUses => lightningUses;
        public int WaterUses => waterUses;
        public int WindUses => windUses;
        public int BurstRank => burstRank;
        public int FireRank => fireRank;
        public int BurstUses => burstUses;
        public int FireUses => fireUses;

        public float BasicAttackCooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);
        public float BasicAttackCooldownDuration => attackCooldown;
        public float Chakra => chakra;
        public float MaxChakra => chakraMaximum;
        public float BurstCooldownRemaining => Mathf.Max(0f, nextBurstTime - Time.time);
        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public float LightningCooldownRemaining => Mathf.Max(0f, nextLightningTime - Time.time);
        public float WaterCooldownRemaining => Mathf.Max(0f, nextWaterTime - Time.time);
        public float WindCooldownRemaining => Mathf.Max(0f, nextWindTime - Time.time);
        public float LightningCooldownDuration => 7f;
        public float WaterCooldownDuration => 5.5f;
        public float WindCooldownDuration => 4.5f;
        public bool HasChakraForBurst => chakra >= burstCost;
        public bool HasChakraForFire => chakra >= fireCost;
        public bool HasChakraForLightning => chakra >= 30f;
        public bool HasChakraForWater => chakra >= 25f;
        public bool HasChakraForWind => chakra >= 22f;
        public float BurstCooldownDuration => burstCooldown;
        public float FireCooldownDuration => fireCooldown;
        public event Action ResourcesChanged;
        public event Action AbilityProgressChanged;

        private void Awake()
        {
            chakra = chakraMaximum;
            equipment = GetComponent<EquipmentManager>();
            ownHealth = GetComponent<CombatantHealth>();
            abilityVfx = GetComponent<AbilityVfxController>();
            targetLock = GetComponent<EnemyTargetLockSystem>();
            if (abilityVfx == null) abilityVfx = gameObject.AddComponent<AbilityVfxController>();
            ResourcesChanged?.Invoke();
        }

        private void Update()
        {
            float previous = chakra;
            chakra = Mathf.Min(chakraMaximum, chakra + chakraRegeneration * Time.deltaTime);
            if (!Mathf.Approximately(previous, chakra) && Time.time >= nextResourceRegenEventTime)
            {
                nextResourceRegenEventTime = Time.time + ResourceRegenEventInterval;
                ResourcesChanged?.Invoke();
            }

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
            float damage = GetScaledDamage(attackDamage);
            if (!TryHitLockedTarget(damage))
                HitTargets(attackRange, attackRadius, damage);
        }

        private bool TryHitLockedTarget(float damage)
        {
            if (targetLock == null) targetLock = GetComponent<EnemyTargetLockSystem>();
            CombatantHealth target = targetLock != null ? targetLock.CurrentTarget : null;
            if (target == null || target.IsDead) return false;

            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            float reach = attackRange + attackRadius;
            if (offset.sqrMagnitude > reach * reach) return false;

            // Face the selected enemy before delivering the focused melee strike.
            if (offset.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);

            // A locked target takes the focused basic strike instead of the wide fallback sweep.
            ApplyDamageOnce(target, damage, false);
            return true;
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
                StunTargetsInFront(burstRange, burstRadius * 1.45f, 0.8f);
            }
            else if (modifier == "burst_chain")
            {
                HitTargets(burstRange, burstRadius, damage * 0.55f);
                ChainLightning(damage, 3, 4.2f);
            }
            else
            {
                HitTargets(burstRange, burstRadius, damage);
                if (modifier == "burst_guard" && ownHealth != null)
                    ownHealth.Restore(ownHealth.MaxHealth * 0.18f);
            }
            if (abilityVfx != null) abilityVfx.PlayChakra(transform.position, transform.forward, modifier, burstRadius);
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
            else if (modifier == "fire_burst" || modifier == "fire_meteor")
            {
                if (Physics.SphereCast(origin, 0.55f, direction, out RaycastHit hit, fireRange,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    DamageArea(hit.point, modifier == "fire_meteor" ? 4.5f : 3.2f,
                        damage * (modifier == "fire_meteor" ? 1.4f : 1.15f), true);
            }
            else FireRay(origin, direction, damage, 0.55f, false);
            if (abilityVfx != null) abilityVfx.PlayFire(origin, direction, fireRange, modifier);
            RecordAbilityUse(false);
            ResourcesChanged?.Invoke();
        }


        public void CastLightningStyle()
        {
            if (Time.time < nextLightningTime || chakra < 30f) return;
            chakra -= 30f;
            nextLightningTime = Time.time + 7f;
            damagedThisCast.Clear();
            Vector3 origin = transform.position + Vector3.up * 1.1f + transform.forward * 0.5f;
            float damage = GetScaledDamage(76f) * (1f + (lightningRank - 1) * 0.05f);
            string modifier = equipment != null ? equipment.GetAbilityModifier("lightning") : string.Empty;
            // Search the actual combat area for enemies instead of stopping at the first
            // collider; terrain and scenery must not consume the lightning cast.
            Vector3 searchCenter = origin + transform.forward * 5.5f;
            int targetCount = Physics.OverlapSphereNonAlloc(searchCenter,
                modifier == "lightning_storm" ? 3.8f : 3.1f, hitBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            CombatantHealth primaryTarget = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < targetCount; i++)
            {
                Collider candidateCollider = hitBuffer[i];
                if (candidateCollider != null)
                {
                    CombatantHealth candidate = candidateCollider.GetComponentInParent<CombatantHealth>();
                    if (candidate != null && candidate.transform != transform && !candidate.IsDead)
                    {
                        Vector3 toCandidate = candidate.transform.position - transform.position;
                        toCandidate.y = 0f;
                        float distance = toCandidate.sqrMagnitude;
                        // Keep acquisition in front of the shinobi; do not select enemies behind
                        // the player just because they happen to be closer to the search sphere.
                        if (distance > 0.01f &&
                            Vector3.Dot(transform.forward, toCandidate.normalized) >= 0.2f)
                        {
                            // A manually selected lock takes priority, provided it is inside
                            // this skill's actual acquisition area and facing arc.
                            if (targetLock != null && candidate == targetLock.CurrentTarget)
                            {
                                primaryTarget = candidate;
                                nearestDistance = -1f;
                            }
                            else if (nearestDistance >= 0f && distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                primaryTarget = candidate;
                            }
                        }
                    }
                }
                hitBuffer[i] = null;
            }

            if (primaryTarget != null)
            {
                float primaryDamage = damage * (modifier == "lightning_storm" ? 1.18f : 1f);
                ApplyDamageOnce(primaryTarget, primaryDamage, false);
                primaryTarget.ApplyStun(modifier == "lightning_storm" ? 1.65f : 1.1f);
                Vector3 impact = primaryTarget.transform.position + Vector3.up;
                if (abilityVfx != null) abilityVfx.PlayLightning(origin, impact);
                if (modifier == "lightning_chain")
                    ChainLightningFrom(primaryTarget, damage * 0.82f, 3, 5f);
                else if (modifier == "lightning_storm")
                {
                    DamageArea(primaryTarget.transform.position, 3.4f, damage * 0.62f, false);
                    StunTargetsAt(primaryTarget.transform.position, 3.4f, 0.55f);
                    if (abilityVfx != null) abilityVfx.PlayLightningStorm(primaryTarget.transform.position);
                }
            }
            RecordElementUse(0);
            ResourcesChanged?.Invoke();
        }

        public void CastWaterStyle()
        {
            if (Time.time < nextWaterTime || chakra < 25f) return;
            chakra -= 25f;
            nextWaterTime = Time.time + 5.5f;
            damagedThisCast.Clear();
            Vector3 origin = transform.position + Vector3.up * 0.7f + transform.forward * 2f;
            float damage = GetScaledDamage(43f) * (1f + (waterRank - 1) * 0.05f);
            string modifier = equipment != null ? equipment.GetAbilityModifier("water") : string.Empty;
            float radiusBonus = modifier == "water_torrent" ? 0.8f : 0f;
            float waterDamage = damage * (modifier == "water_torrent" ? 1.22f : 1f);
            for (int step = 0; step < 3; step++)
            {
                Vector3 center = origin + transform.forward * (step * 2.3f);
                float radius = 1.65f + step * 0.15f + radiusBonus;
                int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    Collider hit = hitBuffer[i];
                    if (hit != null)
                    {
                        CombatantHealth target = hit.GetComponentInParent<CombatantHealth>();
                        if (target != null && target.transform != transform && !target.IsDead)
                        {
                            ApplyDamageOnce(target, waterDamage, false);
                            target.ApplyStun(modifier == "water_prison" ? 1.15f : 0.22f);
                        }
                    }
                    hitBuffer[i] = null;
                }
            }
            if (modifier == "water_prison")
            {
                Vector3 prisonCenter = origin + transform.forward * 3.5f;
                StunTargetsInFront(6f, 2.5f, 1.15f);
                if (abilityVfx != null) abilityVfx.PlayWaterPrison(prisonCenter);
            }
            if (abilityVfx != null) abilityVfx.PlayWater(origin, transform.forward, 7f, modifier);
            RecordElementUse(1);
            ResourcesChanged?.Invoke();
        }

        public void CastWindStyle()
        {
            if (Time.time < nextWindTime || chakra < 22f) return;
            chakra -= 22f;
            nextWindTime = Time.time + 4.5f;
            damagedThisCast.Clear();
            Vector3 center = transform.position + Vector3.up + transform.forward * 3.1f;
            float damage = GetScaledDamage(58f) * (1f + (windRank - 1) * 0.05f);
            string modifier = equipment != null ? equipment.GetAbilityModifier("wind") : string.Empty;
            if (modifier == "wind_blade")
            {
                for (int step = 1; step <= 4; step++)
                    DamageArea(transform.position + Vector3.up + transform.forward * (step * 1.8f),
                        1.65f, damage * 0.78f, false);
            }
            else
            {
                float radius = modifier == "wind_vortex" ? 4.6f : 3.1f;
                int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    Collider hit = hitBuffer[i];
                    if (hit != null)
                    {
                        CombatantHealth target = hit.GetComponentInParent<CombatantHealth>();
                        if (target != null && target.transform != transform && !target.IsDead)
                        {
                            ApplyDamageOnce(target, damage * (modifier == "wind_vortex" ? 0.9f : 1f), false);
                            target.ApplyStun(modifier == "wind_vortex" ? 0.65f : 0.18f);
                        }
                    }
                    hitBuffer[i] = null;
                }
            }
            if (abilityVfx != null) abilityVfx.PlayWind(center, transform.forward, modifier);
            RecordElementUse(2);
            ResourcesChanged?.Invoke();
        }

        private void ChainLightningFrom(CombatantHealth firstTarget, float baseDamage, int maxJumps, float jumpRadius)
        {
            CombatantHealth previous = firstTarget;
            Vector3 searchCenter = firstTarget.transform.position + Vector3.up;
            for (int jump = 0; jump < maxJumps; jump++)
            {
                int count = Physics.OverlapSphereNonAlloc(searchCenter, jumpRadius, chainBuffer,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                CombatantHealth closest = null;
                float closestDistance = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    Collider hit = chainBuffer[i];
                    if (hit == null) continue;
                    CombatantHealth candidate = hit.GetComponentInParent<CombatantHealth>();
                    if (candidate == null || candidate.IsDead || candidate.transform == transform ||
                        damagedThisCast.Contains(candidate)) continue;
                    float distance = (candidate.transform.position - searchCenter).sqrMagnitude;
                    if (distance < closestDistance) { closestDistance = distance; closest = candidate; }
                    chainBuffer[i] = null;
                }
                if (closest == null) break;
                if (abilityVfx != null)
                    abilityVfx.PlayLightning(previous.transform.position + Vector3.up, closest.transform.position + Vector3.up);
                ApplyDamageOnce(closest, baseDamage * Mathf.Pow(0.72f, jump), false);
                closest.ApplyStun(0.45f);
                previous = closest;
                searchCenter = closest.transform.position + Vector3.up;
            }
            for (int i = 0; i < chainBuffer.Length; i++) chainBuffer[i] = null;
        }

        private void RecordElementUse(int element)
        {
            if (element == 0) { lightningUses++; if (lightningRank < 5 && lightningUses >= lightningRank * 20) lightningRank++; }
            else if (element == 1) { waterUses++; if (waterRank < 5 && waterUses >= waterRank * 20) waterRank++; }
            else { windUses++; if (windRank < 5 && windUses >= windRank * 20) windRank++; }
            AbilityProgressChanged?.Invoke();
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

        private void ChainLightning(float baseDamage, int maxJumps, float jumpRadius)
        {
            CombatantHealth previous = null;
            Vector3 searchCenter = transform.position + Vector3.up + transform.forward * burstRange * 0.55f;
            for (int jump = 0; jump < maxJumps; jump++)
            {
                int count = Physics.OverlapSphereNonAlloc(searchCenter, jumpRadius, chainBuffer,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                CombatantHealth closest = null;
                float closestDistance = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    Collider hit = chainBuffer[i];
                    if (hit == null) continue;
                    CombatantHealth candidate = hit.GetComponentInParent<CombatantHealth>();
                    if (candidate == null || candidate.IsDead || candidate.transform == transform ||
                        damagedThisCast.Contains(candidate)) continue;
                    float distance = (candidate.transform.position - searchCenter).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = candidate;
                    }
                }
                if (closest == null) break;
                if (abilityVfx != null) abilityVfx.PlayLightning(previous != null ? previous.transform.position + Vector3.up : transform.position + Vector3.up, closest.transform.position + Vector3.up);
                ApplyDamageOnce(closest, baseDamage * Mathf.Pow(0.72f, jump + 1), false);
                closest.ApplyStun(0.35f);
                previous = closest;
                searchCenter = previous.transform.position + Vector3.up;
            }
            for (int i = 0; i < chainBuffer.Length; i++) chainBuffer[i] = null;
        }

        private void StunTargetsAt(Vector3 center, float radius, float duration)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit != null)
                {
                    CombatantHealth target = hit.GetComponentInParent<CombatantHealth>();
                    if (target != null && target.transform != transform && !target.IsDead)
                        target.ApplyStun(duration);
                }
                hitBuffer[i] = null;
            }
        }

        private void StunTargetsInFront(float range, float radius, float duration)
        {
            Vector3 center = transform.position + Vector3.up + transform.forward * range * 0.55f;
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit == null) continue;
                CombatantHealth target = hit.GetComponentInParent<CombatantHealth>();
                if (target != null && target.transform != transform && !target.IsDead)
                    target.ApplyStun(duration);
                hitBuffer[i] = null;
            }
        }

        private void ApplyDamageOnce(CombatantHealth target, float damage, bool applyBurn)
        {
            if (target == null || target.IsDead || !damagedThisCast.Add(target)) return;
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            bool critical = equipment != null && UnityEngine.Random.value < equipment.CriticalChance;
            if (critical) damage *= equipment.CriticalDamageMultiplier;
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

        public void RestoreAbilityProgress(int savedBurstRank, int savedFireRank, int savedBurstUses, int savedFireUses,
            int savedLightningRank = 1, int savedWaterRank = 1, int savedWindRank = 1,
            int savedLightningUses = 0, int savedWaterUses = 0, int savedWindUses = 0)
        {
            burstRank = Mathf.Clamp(savedBurstRank, 1, 5);
            fireRank = Mathf.Clamp(savedFireRank, 1, 5);
            burstUses = Mathf.Max(0, savedBurstUses);
            fireUses = Mathf.Max(0, savedFireUses);
            lightningRank = Mathf.Clamp(savedLightningRank, 1, 5);
            waterRank = Mathf.Clamp(savedWaterRank, 1, 5);
            windRank = Mathf.Clamp(savedWindRank, 1, 5);
            lightningUses = Mathf.Max(0, savedLightningUses);
            waterUses = Mathf.Max(0, savedWaterUses);
            windUses = Mathf.Max(0, savedWindUses);
        }

        private float GetScaledDamage(float baseDamage)
        {
            if (equipment == null) equipment = GetComponent<EquipmentManager>();
            if (equipment == null) return baseDamage;
            return baseDamage * equipment.DamageMultiplier * equipment.LegendarySynergyDamageMultiplier;
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
                    ApplyDamageOnce(health, damage, false);
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
