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
        private CombatAnimationBridge animationBridge;
        private CombatantHealth health;
        private float nextSlamTime;
        private float windupRemaining;
        private bool windingUp;
        private bool phaseTwo;
        private bool specialWindup;
        private int bossProfile;
        private float nextSpecialTime;
        private Color baseColor;
        private MaterialPropertyBlock bodyColorBlock;
        private int bodyColorPropertyId;
        private bool supportsBodyColor;
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorProperty = Shader.PropertyToID("_Color");
        private LineRenderer attackTelegraph;
        private Material telegraphMaterial;
        private const int TelegraphSegments = 48;

        public void SetTarget(Transform value) => target = value;

        // 0 Ashen, 1 Verdant, 2 Thunder, 3 Drowned.
        public void ConfigureProfile(int profile)
        {
            bossProfile = Mathf.Clamp(profile, 0, 3);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animationBridge = GetComponent<CombatAnimationBridge>();
            if (animationBridge == null) animationBridge = gameObject.AddComponent<CombatAnimationBridge>();
            health = GetComponent<CombatantHealth>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            if (bodyRenderer != null)
            {
                Material shared = bodyRenderer.sharedMaterial;
                if (shared != null)
                {
                    if (shared.HasProperty(BaseColorProperty))
                    {
                        bodyColorPropertyId = BaseColorProperty;
                        supportsBodyColor = true;
                    }
                    else if (shared.HasProperty(LegacyColorProperty))
                    {
                        bodyColorPropertyId = LegacyColorProperty;
                        supportsBodyColor = true;
                    }
                    if (supportsBodyColor)
                    {
                        baseColor = shared.GetColor(bodyColorPropertyId);
                        bodyColorBlock = new MaterialPropertyBlock();
                        bodyRenderer.GetPropertyBlock(bodyColorBlock);
                    }
                }
            }
            CreateAttackTelegraph();
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
            if (telegraphMaterial != null)
            {
                if (Application.isPlaying) Destroy(telegraphMaterial);
                else DestroyImmediate(telegraphMaterial);
            }
        }

        private void Update()
        {
            if (health == null || health.IsDead || target == null) return;
            CombatantHealth currentTargetHealth = target.GetComponent<CombatantHealth>();
            if (currentTargetHealth == null || currentTargetHealth.IsDead)
            {
                target = null;
                windingUp = false;
                specialWindup = false;
                windupRemaining = 0f;
                SetTelegraphVisible(false);
                if (bodyRenderer != null) SetBodyColor(phaseTwo ? ProfileColor() : baseColor);
                return;
            }
            if (health.IsStunned)
            {
                windingUp = false;
                specialWindup = false;
                windupRemaining = 0f;
                SetTelegraphVisible(false);
                if (bodyRenderer != null) SetBodyColor(phaseTwo ? ProfileColor() : baseColor);
                return;
            }

            float healthRatio = health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 1f;
            if (!phaseTwo && healthRatio <= phaseTwoHealthThreshold)
            {
                phaseTwo = true;
                moveSpeed *= 1.35f;
                slamDamage *= 1.3f;
                slamCooldown = Mathf.Max(1.8f, slamCooldown * 0.72f);
                if (bodyRenderer != null) SetBodyColor(ProfileColor());
                Debug.Log("SHINOBI LEGACY: Boss entered phase two: " + gameObject.name);
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > detectionRange)
            {
                // Cancel a pending attack if the player escapes the boss encounter range.
                if (windingUp)
                {
                    windingUp = false;
                    specialWindup = false;
                    windupRemaining = 0f;
                    SetTelegraphVisible(false);
                    if (bodyRenderer != null) SetBodyColor(phaseTwo ? ProfileColor() : baseColor);
                }
                return;
            }

            Face(offset.normalized);

            if (windingUp)
            {
                UpdateAttackTelegraph();
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
                if (bodyRenderer != null) SetBodyColor(ProfileColor());
                SetTelegraphVisible(true);
                UpdateAttackTelegraph();
                return;
            }

            if (animationBridge != null) animationBridge.SetMoveSpeed(distance > slamRange ? moveSpeed : 0f);
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
                if (bodyRenderer != null) SetBodyColor(new Color(1f, 0.55f, 0.08f));
                SetTelegraphVisible(true);
                UpdateAttackTelegraph();
            }
        }

        private void PerformSlam()
        {
            windingUp = false;
            specialWindup = false;
            SetTelegraphVisible(false);
            if (bodyRenderer != null) SetBodyColor(phaseTwo ? ProfileColor() : baseColor);
            if (target == null) return;

            Vector3 flatTarget = target.position;
            flatTarget.y = transform.position.y;
            if (Vector3.Distance(transform.position, flatTarget) <= slamRange + 0.8f)
            {
                if (animationBridge != null) animationBridge.PlayAttack();
                CombatantHealth playerHealth = target.GetComponent<CombatantHealth>();
                if (playerHealth != null && !playerHealth.IsDead) playerHealth.TakeDamage(slamDamage);
            }
        }

        private void PerformSpecial()
        {
            windingUp = false;
            specialWindup = false;
            SetTelegraphVisible(false);
            if (bodyRenderer != null) SetBodyColor(ProfileColor());
            if (target == null) return;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > specialRange + 1f) return;

            CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
            if (targetHealth == null || targetHealth.IsDead) return;
            if (bossProfile == 1)
            {
                // Verdant boss briefly regenerates instead of dealing burst damage.
                health.Restore(health.MaxHealth * 0.08f);
                return;
            }

            if (animationBridge != null) animationBridge.TriggerAbility();
            float damage = slamDamage * (bossProfile == 0 ? 1.55f : 1.2f);
            targetHealth.TakeDamage(damage);
            if (bossProfile == 2) targetHealth.ApplyStun(1.15f);
            else if (bossProfile == 3) targetHealth.ApplyStun(0.55f);
        }

        private void SetBodyColor(Color color)
        {
            if (bodyRenderer == null || !supportsBodyColor || bodyColorBlock == null) return;
            bodyRenderer.GetPropertyBlock(bodyColorBlock);
            bodyColorBlock.SetColor(bodyColorPropertyId, color);
            bodyRenderer.SetPropertyBlock(bodyColorBlock);
        }

        private void CreateAttackTelegraph()
        {
            GameObject ringObject = new GameObject("Boss_Attack_Telegraph");
            ringObject.transform.SetParent(transform, false);
            attackTelegraph = ringObject.AddComponent<LineRenderer>();
            attackTelegraph.useWorldSpace = false;
            attackTelegraph.loop = true;
            attackTelegraph.positionCount = TelegraphSegments;
            attackTelegraph.startWidth = 0.12f;
            attackTelegraph.endWidth = 0.12f;
            attackTelegraph.numCapVertices = 2;
            Shader telegraphShader = Shader.Find("Sprites/Default");
            if (telegraphShader == null)
            {
                attackTelegraph.enabled = false;
                return;
            }
            telegraphMaterial = new Material(telegraphShader);
            attackTelegraph.material = telegraphMaterial;
            attackTelegraph.startColor = new Color(1f, 0.28f, 0.08f, 0.9f);
            attackTelegraph.endColor = attackTelegraph.startColor;
            attackTelegraph.enabled = false;
        }

        private void UpdateAttackTelegraph()
        {
            if (attackTelegraph == null) return;
            float radius = specialWindup ? specialRange : slamRange + 0.8f;
            float pulse = 1f + Mathf.Sin(Time.time * 9f) * 0.035f;
            for (int i = 0; i < TelegraphSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / TelegraphSegments;
                attackTelegraph.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius * pulse, 0.12f, Mathf.Sin(angle) * radius * pulse));
            }
            Color warning = specialWindup ? ProfileColor() : new Color(1f, 0.28f, 0.06f);
            warning.a = 0.9f;
            attackTelegraph.startColor = warning;
            attackTelegraph.endColor = warning;
        }

        private void SetTelegraphVisible(bool visible)
        {
            if (attackTelegraph != null) attackTelegraph.enabled = visible;
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
            SetTelegraphVisible(false);
            if (bodyRenderer != null) SetBodyColor(phaseTwo ? ProfileColor() : baseColor);
            enabled = false;
            if (controller != null) controller.enabled = false;
            Destroy(gameObject, 2f);
        }
    }
}
