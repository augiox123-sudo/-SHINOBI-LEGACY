using UnityEngine;
using System.Collections;
using ShinobiLegacy.Combat;
using ShinobiLegacy.CameraSystem;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;
using System.Collections.Generic;

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
        private AbilityVfxController impactVfx;
        private ImpactCameraShake impactCameraShake;
        private float nextSlamTime;
        private float windupRemaining;
        private float windupTotal;
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
        private const int MaxActiveSummons = 2;
        private const float SummonInterval = 19f;
        private GameObject summonTemplate;
        private PlayerProgression summonProgression;
        private readonly List<GameObject> activeSummons = new List<GameObject>(MaxActiveSummons);
        private float nextSummonTime;
        private int summonWave;

        public void ConfigureSummons(GameObject template, PlayerProgression progression)
        {
            summonTemplate = template;
            summonProgression = progression;
        }

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
            impactVfx = GetComponent<AbilityVfxController>();
            if (impactVfx == null) impactVfx = gameObject.AddComponent<AbilityVfxController>();
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                impactCameraShake = mainCamera.GetComponent<ImpactCameraShake>();
                if (impactCameraShake == null) impactCameraShake = mainCamera.gameObject.AddComponent<ImpactCameraShake>();
            }
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
                nextSummonTime = Time.time + 2.5f;
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
            if (phaseTwo) TrySummonMinions();

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
                windupTotal = windupRemaining;
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
                windupTotal = windupRemaining;
                nextSlamTime = Time.time + slamCooldown;
                if (bodyRenderer != null) SetBodyColor(new Color(1f, 0.55f, 0.08f));
                SetTelegraphVisible(true);
                UpdateAttackTelegraph();
            }
        }

        private void TrySummonMinions()
        {
            if (summonTemplate == null || summonProgression == null || Time.time < nextSummonTime) return;

            for (int i = activeSummons.Count - 1; i >= 0; i--)
            {
                GameObject summon = activeSummons[i];
                if (summon == null)
                {
                    activeSummons.RemoveAt(i);
                    continue;
                }

                CombatantHealth summonHealth = summon.GetComponent<CombatantHealth>();
                if (summonHealth == null || summonHealth.IsDead) activeSummons.RemoveAt(i);
            }

            int availableSlots = MaxActiveSummons - activeSummons.Count;
            if (availableSlots <= 0) return;

            int spawnCount = Mathf.Min(2, availableSlots);
            nextSummonTime = Time.time + SummonInterval;
            StartCoroutine(ShowSummonWarningsAndSpawn(spawnCount));
        }

        private IEnumerator ShowSummonWarningsAndSpawn(int spawnCount)
        {
            Vector3[] spawnPositions = new Vector3[spawnCount];
            LineRenderer[] warningRings = new LineRenderer[spawnCount];

            for (int i = 0; i < spawnCount; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Vector3 position = transform.position + transform.right * (side * 2.4f) + transform.forward * 1.6f;
                position.y = 0.08f;
                spawnPositions[i] = position;
                warningRings[i] = CreateSummonWarning(position);
            }

            const float warningDuration = 0.9f;
            float elapsed = 0f;
            while (elapsed < warningDuration)
            {
                if (health == null || health.IsDead || target == null || !phaseTwo)
                {
                    DestroySummonWarnings(warningRings);
                    yield break;
                }

                CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
                Vector3 flatOffset = target.position - transform.position;
                flatOffset.y = 0f;
                if (targetHealth == null || targetHealth.IsDead || flatOffset.magnitude > detectionRange)
                {
                    DestroySummonWarnings(warningRings);
                    yield break;
                }

                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.08f;
                for (int i = 0; i < warningRings.Length; i++)
                {
                    LineRenderer ring = warningRings[i];
                    if (ring == null) continue;
                    ring.widthMultiplier = Mathf.Lerp(0.08f, 0.17f, elapsed / warningDuration);
                    ring.transform.position = spawnPositions[i] + Vector3.up * (0.025f + pulse * 0.01f);
                }
                yield return null;
            }

            DestroySummonWarnings(warningRings);
            if (health == null || health.IsDead || target == null || summonTemplate == null) yield break;

            int spawned = 0;
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                if (activeSummons.Count >= MaxActiveSummons) break;
                Vector3 position = spawnPositions[i];
                position.y = transform.position.y;
                GameObject minion = Instantiate(summonTemplate, position, Quaternion.identity);
                minion.name = "Guardian_Summoned_Raider_" + (++summonWave);
                minion.SetActive(false);

                EnemyCombatAI minionAI = minion.GetComponent<EnemyCombatAI>();
                if (minionAI != null)
                {
                    minionAI.ConfigureBehavior(summonWave % 2);
                    minionAI.SetTarget(target);
                }

                EnemyRewardDrop rewards = minion.GetComponent<EnemyRewardDrop>();
                if (rewards != null) rewards.Configure(summonProgression, 45, 0.85f, false);

                minion.SetActive(true);
                CombatantHealth minionHealth = minion.GetComponent<CombatantHealth>();
                if (minionHealth != null) minionHealth.SetMaxHealth(85f);
                activeSummons.Add(minion);
                spawned++;
            }

            if (spawned > 0)
                Debug.Log("SHINOBI LEGACY: Stone Guardian summoned " + spawned + " raider(s).");
        }

        private LineRenderer CreateSummonWarning(Vector3 position)
        {
            GameObject ringObject = new GameObject("Boss_Summon_Warning");
            ringObject.transform.position = position;
            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = TelegraphSegments;
            ring.numCapVertices = 2;
            ring.widthMultiplier = 0.08f;
            ring.sharedMaterial = telegraphMaterial;
            Color warning = new Color(0.72f, 0.28f, 1f, 0.9f);
            ring.startColor = warning;
            ring.endColor = warning;
            const float radius = 0.95f;
            for (int i = 0; i < TelegraphSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / TelegraphSegments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
            return ring;
        }

        private void DestroySummonWarnings(LineRenderer[] rings)
        {
            if (rings == null) return;
            for (int i = 0; i < rings.Length; i++)
                if (rings[i] != null) Destroy(rings[i].gameObject);
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
            bool targetHit = Vector3.Distance(transform.position, flatTarget) <= slamRange + 0.8f;
            if (animationBridge != null) animationBridge.PlayHeavyAttack();
            PlayImpactFeedback(transform.position + Vector3.up * 0.12f, slamRange + 0.8f,
                new Color(1f, 0.42f, 0.12f), targetHit ? 0.2f : 0f);

            if (targetHit)
            {
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

            CombatantHealth targetHealth = target.GetComponent<CombatantHealth>();
            if (targetHealth == null || targetHealth.IsDead) return;
            if (animationBridge != null) animationBridge.PlayHeavyAttack();

            // The special is a dodgeable close-range area attack. The warning ring
            // uses the same profile radius that is checked when the attack resolves.
            float impactRadius = SpecialImpactRadius();
            Vector3 flatOffset = target.position - transform.position;
            flatOffset.y = 0f;
            bool targetInsideImpact = flatOffset.sqrMagnitude <= impactRadius * impactRadius;
            float shakeStrength = targetInsideImpact && bossProfile != 1 ? 0.24f : 0f;
            PlayImpactFeedback(transform.position + Vector3.up * 0.12f, impactRadius, ProfileColor(), shakeStrength);

            switch (bossProfile)
            {
                case 0: // Ashen: heavy impact followed by a short burn.
                    if (targetInsideImpact)
                    {
                        targetHealth.TakeDamage(slamDamage * 1.55f);
                        if (!targetHealth.IsDead) targetHealth.ApplyBurn(Mathf.Max(2f, slamDamage * 0.12f), 3f, 1f);
                    }
                    break;

                case 1: // Verdant: defensive recovery; the player can use the heal window to reposition.
                    health.Restore(health.MaxHealth * 0.10f);
                    break;

                case 2: // Thunder: area burst and a longer interrupting stun.
                    if (targetInsideImpact)
                    {
                        targetHealth.TakeDamage(slamDamage * 1.2f);
                        if (!targetHealth.IsDead) targetHealth.ApplyStun(1.25f);
                    }
                    break;

                default: // Drowned: heavier area hit with a shorter control effect.
                    if (targetInsideImpact)
                    {
                        targetHealth.TakeDamage(slamDamage * 1.35f);
                        if (!targetHealth.IsDead) targetHealth.ApplyStun(0.8f);
                    }
                    break;
            }
        }

        private void PlayImpactFeedback(Vector3 position, float radius, Color color, float shakeStrength)
        {
            if (impactVfx != null) impactVfx.PlayBossImpact(position, radius, color);
            if (shakeStrength > 0f && impactCameraShake != null)
                impactCameraShake.Shake(shakeStrength, 0.22f);
        }

        private float SpecialImpactRadius()
        {
            switch (bossProfile)
            {
                case 1: return 3.8f; // Verdant's special is a self-heal, but its warning remains compact.
                case 2: return 5.2f; // Thunder has the widest damaging shockwave.
                case 3: return 4.2f; // Drowned trades radius for stronger control pressure.
                default: return 4.6f; // Ashen.
            }
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
            float radius = specialWindup ? SpecialImpactRadius() : slamRange + 0.8f;
            float progress = windupTotal > 0f ? 1f - Mathf.Clamp01(windupRemaining / windupTotal) : 1f;
            float pulseSpeed = Mathf.Lerp(5f, 19f, progress);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * Mathf.Lerp(0.025f, 0.075f, progress);
            for (int i = 0; i < TelegraphSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / TelegraphSegments;
                attackTelegraph.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius * pulse, 0.12f, Mathf.Sin(angle) * radius * pulse));
            }
            float lineWidth = Mathf.Lerp(0.09f, 0.2f, progress);
            attackTelegraph.startWidth = lineWidth;
            attackTelegraph.endWidth = lineWidth * 0.72f;
            Color warning = specialWindup ? ProfileColor() : new Color(1f, 0.28f, 0.06f);
            warning.a = Mathf.Lerp(0.55f, 1f, progress);
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
