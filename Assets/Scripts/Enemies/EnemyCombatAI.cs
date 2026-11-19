using UnityEngine;
using System.Collections;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatantHealth))]
    public sealed class EnemyCombatAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 1.65f;
        [SerializeField] private float attackDamage = 9f;
        [SerializeField] private float attackCooldown = 1.6f;
        [SerializeField] private float turnSpeed = 540f;
        [SerializeField] private float attackWindup = 0.28f;
        [SerializeField] private float skirmisherPreferredRange = 3.4f;
        [SerializeField] private float dodgeInterval = 2.8f;

        // 0 Hunter, 1 Skirmisher, 2 Brute.
        private int behavior;
        private float attackWindupRemaining;
        private bool attackPending;
        private float nextDodgeTime;
        private float dodgeRemaining;
        private float dodgeDirection = 1f;
        private float orbitDirection = 1f;

        private CharacterController controller;
        private CombatAnimationBridge animationBridge;
        private CombatantHealth health;
        private float nextAttackTime;
        private Renderer[] visuals;
        private Color[] originalBaseColors;
        private Color[] originalLegacyColors;
        private bool[] supportsBaseColor;
        private bool[] supportsLegacyColor;
        private MaterialPropertyBlock hitBlock;
        private Coroutine hitFlashRoutine;
        private readonly RaycastHit[] meleeHitBuffer = new RaycastHit[16];
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        public void SetTarget(Transform value) => target = value;

        public void ConfigureBehavior(int value)
        {
            behavior = Mathf.Clamp(value, 0, 2);
            if (behavior == 1)
            {
                moveSpeed *= 1.08f;
                attackDamage *= 0.82f;
                attackCooldown *= 0.92f;
                attackRange = Mathf.Max(attackRange, 2.05f);
                skirmisherPreferredRange = 3.4f;
            }
            else if (behavior == 2)
            {
                moveSpeed *= 0.78f;
                attackDamage *= 1.55f;
                attackCooldown *= 1.28f;
                attackRange = Mathf.Max(attackRange, 2f);
                attackWindup = 0.52f;
            }
        }

        public void Configure(float speed, float damage, float cooldown, float range)
        {
            moveSpeed = Mathf.Max(0.5f, speed);
            attackDamage = Mathf.Max(1f, damage);
            attackCooldown = Mathf.Max(0.4f, cooldown);
            detectionRange = Mathf.Max(attackRange, range);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animationBridge = GetComponent<CombatAnimationBridge>();
            if (animationBridge == null) animationBridge = gameObject.AddComponent<CombatAnimationBridge>();
            health = GetComponent<CombatantHealth>();
            visuals = GetComponentsInChildren<Renderer>();
            originalBaseColors = new Color[visuals.Length];
            originalLegacyColors = new Color[visuals.Length];
            supportsBaseColor = new bool[visuals.Length];
            supportsLegacyColor = new bool[visuals.Length];
            for (int i = 0; i < visuals.Length; i++)
            {
                Material material = visuals[i] != null ? visuals[i].sharedMaterial : null;
                supportsBaseColor[i] = material != null && material.HasProperty(BaseColorProperty);
                supportsLegacyColor[i] = material != null && material.HasProperty(ColorProperty);
                originalBaseColors[i] = supportsBaseColor[i] ? material.GetColor(BaseColorProperty) : Color.white;
                originalLegacyColors[i] = supportsLegacyColor[i] ? material.GetColor(ColorProperty) : Color.white;
            }
            hitBlock = new MaterialPropertyBlock();
            health.Died += OnDied;
            health.HealthChanged += OnHealthChanged;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
                health.HealthChanged -= OnHealthChanged;
            }
        }

        private void Update()
        {
            if (health == null || health.IsDead) return;
            if (animationBridge != null) animationBridge.SetStaggered(health.IsStaggered);
            if (health.IsStunned || health.IsStaggered)
            {
                attackPending = false;
                if (animationBridge != null)
                {
                    animationBridge.CancelWindup();
                    animationBridge.SetMoveSpeed(0f);
                }
                return;
            }
            if (target == null)
            {
                attackPending = false;
                if (animationBridge != null)
                {
                    animationBridge.CancelWindup();
                    animationBridge.SetMoveSpeed(0f);
                }
                return;
            }
            CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
            if (targetHealth == null || targetHealth.IsDead)
            {
                target = null;
                attackPending = false;
                if (animationBridge != null)
                {
                    animationBridge.CancelWindup();
                    animationBridge.SetMoveSpeed(0f);
                }
                return;
            }
            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange)
            {
                attackPending = false;
                if (animationBridge != null)
                {
                    animationBridge.CancelWindup();
                    animationBridge.SetMoveSpeed(0f);
                }
                return;
            }

            // Lock the attack direction once the windup starts. The player can evade
            // by moving outside the committed strike arc before the hit resolves.
            if (!attackPending && distance > 0.05f)
            {
                Quaternion desired = Quaternion.LookRotation(offset.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
            }

            if (attackPending)
            {
                attackWindupRemaining -= Time.deltaTime;
                if (animationBridge != null) animationBridge.SetMoveSpeed(0f);
                if (attackWindupRemaining <= 0f)
                {
                    attackPending = false;
                    if (target != null)
                    {
                        CombatantHealth liveTarget = target.GetComponent<CombatantHealth>();
                        if (liveTarget != null && !liveTarget.IsDead && IsMeleeStrikeValid(liveTarget))
                        {
                            if (animationBridge != null) animationBridge.PlayAttack();
                            liveTarget.TakeDamage(attackDamage);
                            if (!liveTarget.IsDead) liveTarget.ApplyKnockbackFrom(transform.position, 1.8f, 0.10f);
                        }
                    }
                }
                return;
            }

            Vector3 moveDirection = Vector3.zero;
            bool shouldAttack = false;
            if (behavior == 1)
            {
                float preferred = skirmisherPreferredRange;
                if (distance > preferred + 0.65f) moveDirection = offset.normalized;
                else if (distance < preferred - 0.65f) moveDirection = -offset.normalized;
                else moveDirection = Vector3.Cross(Vector3.up, offset.normalized) * orbitDirection;

                if (Time.time >= nextDodgeTime && distance < 5.2f)
                {
                    dodgeRemaining = 0.32f;
                    dodgeDirection = Random.value > 0.5f ? 1f : -1f;
                    orbitDirection = -orbitDirection;
                    nextDodgeTime = Time.time + dodgeInterval + Random.Range(0f, 0.8f);
                }
                if (dodgeRemaining > 0f)
                {
                    dodgeRemaining -= Time.deltaTime;
                    moveDirection = Vector3.Cross(Vector3.up, offset.normalized) * dodgeDirection * 2.1f;
                }
                shouldAttack = distance <= attackRange;
            }
            else
            {
                shouldAttack = distance <= attackRange;
                if (!shouldAttack) moveDirection = offset.normalized;
            }

            float moveMagnitude = moveDirection.magnitude;
            if (animationBridge != null) animationBridge.SetMoveSpeed(moveMagnitude > 0.05f ? moveSpeed : 0f);
            if (moveMagnitude > 0.05f)
                controller.Move((moveDirection.normalized * moveSpeed + Vector3.down * 2f) * Time.deltaTime);

            if (shouldAttack && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                attackPending = true;
                attackWindupRemaining = attackWindup;
                if (animationBridge != null) animationBridge.PlayWindup(behavior == 2);
            }
        }

        private bool IsMeleeStrikeValid(CombatantHealth liveTarget)
        {
            Collider targetCollider = liveTarget.GetComponent<Collider>();
            if (targetCollider == null) targetCollider = liveTarget.GetComponentInChildren<Collider>();
            Vector3 strikeOrigin = transform.position + Vector3.up * 1.0f + transform.forward * 0.35f;
            Vector3 targetPoint = targetCollider != null
                ? targetCollider.ClosestPoint(strikeOrigin)
                : liveTarget.transform.position + Vector3.up * 0.85f;

            Vector3 toTarget = targetPoint - strikeOrigin;
            float verticalGap = Mathf.Abs(toTarget.y);
            Vector3 horizontal = new Vector3(toTarget.x, 0f, toTarget.z);
            float reach = attackRange + 0.18f;
            if (verticalGap > 1.5f || horizontal.magnitude > reach) return false;

            Vector3 direction = horizontal.sqrMagnitude > 0.0001f
                ? horizontal.normalized
                : transform.forward;
            if (Vector3.Dot(transform.forward, direction) < 0.35f) return false;

            // Ignore this enemy's own colliders, but let walls and other solid
            // objects block the strike. The closest valid hit must belong to target.
            float rayDistance = toTarget.magnitude;
            if (rayDistance > 0.05f)
            {
                int hitCount = Physics.RaycastNonAlloc(
                    strikeOrigin,
                    toTarget.normalized,
                    meleeHitBuffer,
                    rayDistance + 0.08f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                float nearestDistance = float.MaxValue;
                Transform nearestTransform = null;
                for (int i = 0; i < hitCount; i++)
                {
                    Collider hitCollider = meleeHitBuffer[i].collider;
                    if (hitCollider == null || hitCollider.transform == transform ||
                        hitCollider.transform.IsChildOf(transform)) continue;

                    if (meleeHitBuffer[i].distance < nearestDistance)
                    {
                        nearestDistance = meleeHitBuffer[i].distance;
                        nearestTransform = hitCollider.transform;
                    }
                }

                if (nearestTransform != null &&
                    nearestTransform != liveTarget.transform &&
                    !nearestTransform.IsChildOf(liveTarget.transform)) return false;
            }

            return true;
        }

        private void OnHealthChanged(float current, float maximum)
        {
            if (health == null || health.IsDead || visuals == null) return;
            if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = StartCoroutine(HitFlash());
        }

        private IEnumerator HitFlash()
        {
            SetHitFlash(new Color(1f, 0.22f, 0.16f, 1f));
            yield return new WaitForSeconds(0.09f);
            RestoreVisualColors();
            yield return new WaitForSeconds(0.045f);
            SetHitFlash(new Color(1f, 0.55f, 0.35f, 1f));
            yield return new WaitForSeconds(0.07f);
            RestoreVisualColors();
            hitFlashRoutine = null;
        }

        private void RestoreVisualColors()
        {
            if (visuals == null || originalBaseColors == null || originalLegacyColors == null) return;
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] == null) continue;
                visuals[i].GetPropertyBlock(hitBlock);
                if (supportsBaseColor[i]) hitBlock.SetColor(BaseColorProperty, originalBaseColors[i]);
                if (supportsLegacyColor[i]) hitBlock.SetColor(ColorProperty, originalLegacyColors[i]);
                visuals[i].SetPropertyBlock(hitBlock);
            }
        }
        private void SetHitFlash(Color color)
        {
            if (visuals == null) return;
            for (int i = 0; i < visuals.Length; i++)
            {
                Renderer visual = visuals[i];
                if (visual == null) continue;
                visual.GetPropertyBlock(hitBlock);
                if (supportsBaseColor[i]) hitBlock.SetColor(BaseColorProperty, color);
                if (supportsLegacyColor[i]) hitBlock.SetColor(ColorProperty, color);
                visual.SetPropertyBlock(hitBlock);
            }
        }

        private void OnDied()
        {
            enabled = false;
            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
                hitFlashRoutine = null;
            }
            RestoreVisualColors();
            if (controller != null) controller.enabled = false;
            Destroy(gameObject, 1.4f);
        }
    }
}