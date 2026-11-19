using UnityEngine;

namespace ShinobiLegacy.Combat
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [Header("Basic attack")]
        [SerializeField] private float attackDamage = 24f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackRadius = 1.15f;
        [SerializeField] private float attackCooldown = 0.55f;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Chakra technique")]
        [SerializeField] private float techniqueDamage = 65f;
        [SerializeField] private float techniqueRange = 5.5f;
        [SerializeField] private float chakraMaximum = 100f;
        [SerializeField] private float chakraCost = 35f;
        [SerializeField] private float chakraRegeneration = 7f;

        private float nextAttackTime;
        private float chakra;
        private readonly Collider[] hitBuffer = new Collider[24];

        public float Chakra => chakra;
        public float MaxChakra => chakraMaximum;

        private void Awake() => chakra = chakraMaximum;

        private void Update()
        {
            chakra = Mathf.Min(chakraMaximum, chakra + chakraRegeneration * Time.deltaTime);
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.Space)) BasicAttack();
            if (Input.GetKeyDown(KeyCode.Q)) CastTechnique();
#endif
        }

        public void BasicAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackCooldown;
            HitTargets(attackRange, attackRadius, attackDamage);
        }

        public void CastTechnique()
        {
            if (chakra < chakraCost) return;
            chakra -= chakraCost;
            HitTargets(techniqueRange, 1.8f, techniqueDamage);
        }

        private void HitTargets(float range, float radius, float damage)
        {
            Vector3 center = transform.position + Vector3.up * 1f + transform.forward * range * 0.55f;
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer, targetLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit == null || hit.transform.IsChildOf(transform)) continue;
                CombatantHealth health = hit.GetComponentInParent<CombatantHealth>();
                if (health == null || health.IsDead || health.transform == transform) continue;
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
