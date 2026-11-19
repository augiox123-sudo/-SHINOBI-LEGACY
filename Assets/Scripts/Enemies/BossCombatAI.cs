using UnityEngine;
using ShinobiLegacy.Combat;

namespace ShinobiLegacy.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatantHealth))]
    public sealed class BossCombatAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float detectionRange = 18f;
        [SerializeField] private float slamRange = 3.8f;
        [SerializeField] private float slamDamage = 28f;
        [SerializeField] private float slamCooldown = 4.2f;
        [SerializeField] private float windupDuration = 0.85f;
        [SerializeField] private float turnSpeed = 360f;
        [SerializeField] private Renderer bodyRenderer;

        private CharacterController controller;
        private CombatantHealth health;
        private float nextSlamTime;
        private float windupRemaining;
        private bool windingUp;
        private Color baseColor;

        public void SetTarget(Transform value) => target = value;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<CombatantHealth>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            if (bodyRenderer != null) baseColor = bodyRenderer.material.color;
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void Update()
        {
            if (health == null || health.IsDead || target == null) return;
            if (health.IsStunned)
            {
                windingUp = false;
                windupRemaining = 0f;
                if (bodyRenderer != null) bodyRenderer.material.color = baseColor;
                return;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange) return;

            Face(offset.normalized);

            if (windingUp)
            {
                windupRemaining -= Time.deltaTime;
                if (windupRemaining <= 0f) PerformSlam();
                return;
            }

            if (distance > slamRange)
            {
                Vector3 direction = offset.normalized;
                controller.Move((direction * moveSpeed + Vector3.down * 2f) * Time.deltaTime);
            }
            else if (Time.time >= nextSlamTime)
            {
                windingUp = true;
                windupRemaining = windupDuration;
                nextSlamTime = Time.time + slamCooldown;
                if (bodyRenderer != null) bodyRenderer.material.color = new Color(1f, 0.55f, 0.08f);
            }
        }

        private void PerformSlam()
        {
            windingUp = false;
            if (bodyRenderer != null) bodyRenderer.material.color = baseColor;
            if (target == null) return;

            Vector3 flatTarget = target.position;
            flatTarget.y = transform.position.y;
            if (Vector3.Distance(transform.position, flatTarget) <= slamRange + 0.8f)
            {
                CombatantHealth playerHealth = target.GetComponent<CombatantHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(slamDamage);
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
            windingUp = false;
            if (bodyRenderer != null) bodyRenderer.material.color = baseColor;
            enabled = false;
            controller.enabled = false;
            Destroy(gameObject, 2f);
        }
    }
}
