using UnityEngine;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatantHealth))]
    public sealed class EnemyCombatAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 1.65f;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float turnSpeed = 540f;

        private CharacterController controller;
        private CombatantHealth health;
        private float nextAttackTime;

        public void SetTarget(Transform value) => target = value;

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
            if (health == null || health.IsDead || target == null) return;
            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange) return;

            if (distance > attackRange)
            {
                Vector3 direction = offset.normalized;
                controller.Move((direction * moveSpeed + Vector3.down * 2f) * Time.deltaTime);
                Face(direction);
            }
            else
            {
                Face(offset.normalized);
                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + attackInterval;
                    CombatantHealth playerHealth = target.GetComponent<CombatantHealth>();
                    if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
                }
            }
        }

        private void Face(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion desired = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
        }

        private void OnDied()
        {
            enabled = false;
            controller.enabled = false;
            transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            Destroy(gameObject, 1.5f);
        }
    }
}
