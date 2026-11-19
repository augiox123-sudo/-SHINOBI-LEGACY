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
        [SerializeField] private float phaseTwoHealthThreshold = 0.55f;
        [SerializeField] private float specialCooldown = 9f;
        [SerializeField] private float specialRange = 7.5f;

        private CharacterController controller;
        private CombatantHealth health;
        private float nextSlamTime;
        private float windupRemaining;
        private bool windingUp;
        private bool phaseTwo;
        private bool specialWindup;
        private int bossProfile;
        private float nextSpecialTime;
        private Color baseColor;

        public void SetTarget(Transform value) => target = value;

        // 0 Ashen, 1 Verdant, 2 Thunder, 3 Drowned.
        public void ConfigureProfile(int profile)
        {
            bossProfile = Mathf.Clamp(profile, 0, 3);
        }

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

            float healthRatio = health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 1f;
            if (!phaseTwo && healthRatio <= phaseTwoHealthThreshold)
            {
                phaseTwo = true;
                moveSpeed *= 1.35f;
                slamDamage *= 1.3f;
                slamCooldown = Mathf.Max(1.8f, slamCooldown * 0.72f);
                if (bodyRenderer != null) bodyRenderer.material.color = ProfileColor();
                Debug.Log("SHINOBI LEGACY: Boss entered phase two: " + gameObject.name);
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange) return;

            Face(offset.normalized);

            if (windingUp)
            {
                windupRemaining -= Time.deltaTime;
                if (windupRemaining <= 0f)
                {
                    if (specialWindup) PerformSpecial();
                    else PerformSlam();
                }
                return;
            }

            if (phaseTwo && distance <= specialRange && Time.time >= nextSpecialTime)
            {
                windingUp = true;
                specialWindup = true;
                windupRemaining = windupDuration * 1.25f;
                nextSpecialTime = Time.time + specialCooldown;
                if (bodyRenderer != null) bodyRenderer.material.color = ProfileColor();
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
                specialWindup = false;
                windupRemaining = windupDuration;
                nextSlamTime = Time.time + slamCooldown;
                if (bodyRenderer != null) bodyRenderer.material.color = new Color(1f, 0.55f, 0.08f);
            }
        }

        private void PerformSlam()
        {
            windingUp = false;
            specialWindup = false;
            if (bodyRenderer != null) bodyRenderer.material.color = phaseTwo ? ProfileColor() : baseColor;
            if (target == null) return;

            Vector3 flatTarget = target.position;
            flatTarget.y = transform.position.y;
            if (Vector3.Distance(transform.position, flatTarget) <= slamRange + 0.8f)
            {
                CombatantHealth playerHealth = target.GetComponent<CombatantHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(slamDamage);
            }
        }

        private void PerformSpecial()
        {
            windingUp = false;
            specialWindup = false;
            if (bodyRenderer != null) bodyRenderer.material.color = ProfileColor();
            if (target == null) return;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > specialRange + 1f) return;

            CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
            if (bossProfile == 1)
            {
                // Verdant boss briefly regenerates instead of dealing burst damage.
                health.Restore(health.MaxHealth * 0.08f);
                return;
            }

            if (targetHealth == null) return;
            float damage = slamDamage * (bossProfile == 0 ? 1.55f : 1.2f);
            targetHealth.TakeDamage(damage);
            if (bossProfile == 2) targetHealth.ApplyStun(1.15f);
            else if (bossProfile == 3) targetHealth.ApplyStun(0.55f);
        }

        private Color ProfileColor()
        {
            switch (bossProfile)
            {
                case 1: return new Color(0.35f, 0.9f, 0.22f);
                case 2: return new Color(0.35f, 0.55f, 1f);
                case 3: return new Color(0.15f, 0.85f, 0.9f);
                default: return new Color(1f, 0.22f, 0.08f);
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
