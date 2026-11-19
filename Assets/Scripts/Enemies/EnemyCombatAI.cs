using UnityEngine;
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
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
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

        private void OnDied()
        {
            enabled = false;
            if (controller != null) controller.enabled = false;
            Destroy(gameObject, 1.4f);
        }
    }
}