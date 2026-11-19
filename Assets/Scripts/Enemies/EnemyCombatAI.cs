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

        private CharacterController controller;
        private CombatantHealth health;
        private float nextAttackTime;
        private Renderer[] visuals;
        private MaterialPropertyBlock hitBlock;
        private Coroutine hitFlashRoutine;
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        public void SetTarget(Transform value) => target = value;

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
            health = GetComponent<CombatantHealth>();
            visuals = GetComponentsInChildren<Renderer>();
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
            if (health == null || health.IsDead || health.IsStunned || target == null) return;
            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange) return;

            if (distance > 0.05f)
            {
                Quaternion desired = Quaternion.LookRotation(offset.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
            }

            if (distance > attackRange)
            {
                Vector3 direction = offset.normalized;
                controller.Move((direction * moveSpeed + Vector3.down * 2f) * Time.deltaTime);
            }
            else if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
                if (targetHealth != null) targetHealth.TakeDamage(attackDamage);
            }
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
            SetHitFlash(Color.white);
            yield return new WaitForSeconds(0.045f);
            SetHitFlash(new Color(1f, 0.55f, 0.35f, 1f));
            yield return new WaitForSeconds(0.07f);
            SetHitFlash(Color.white);
            hitFlashRoutine = null;
        }

        private void SetHitFlash(Color color)
        {
            if (visuals == null) return;
            foreach (Renderer visual in visuals)
            {
                if (visual == null) continue;
                visual.GetPropertyBlock(hitBlock);
                hitBlock.SetColor(ColorProperty, color);
                visual.SetPropertyBlock(hitBlock);
            }
        }

        private void OnDied()
        {
            enabled = false;
            if (controller != null) controller.enabled = false;
            Destroy(gameObject, 1.4f);
        }
    }
}