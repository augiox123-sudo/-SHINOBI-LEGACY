using UnityEngine;

namespace ShinobiLegacy.Combat
{
    /// <summary>
    /// Lightweight fallback motion for the procedural blockout rig.
    /// Replace with authored Animator clips when skinned character assets are integrated.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShinobiProceduralMotion : MonoBehaviour
    {
        [SerializeField] private float runCycleSpeed = 10f;
        [SerializeField] private float runSwingAngle = 28f;
        [SerializeField] private float idleBobAmount = 0.025f;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform torso;
        private Vector3 basePosition;
        private Quaternion leftArmBase;
        private Quaternion rightArmBase;
        private Quaternion leftLegBase;
        private Quaternion rightLegBase;
        private Quaternion torsoBase;
        private float moveSpeed;
        private float attackTimer;
        private float hitTimer;
        private float abilityTimer;
        private float heavyAttackTimer;
        private bool dead;
        private float cycle;

        private void Awake()
        {
            basePosition = transform.localPosition;
            leftArm = transform.Find("ArmLeft");
            rightArm = transform.Find("ArmRight");
            leftLeg = transform.Find("LegLeft");
            rightLeg = transform.Find("LegRight");
            torso = transform.Find("Torso");
            if (leftArm != null) leftArmBase = leftArm.localRotation;
            if (rightArm != null) rightArmBase = rightArm.localRotation;
            if (leftLeg != null) leftLegBase = leftLeg.localRotation;
            if (rightLeg != null) rightLegBase = rightLeg.localRotation;
            if (torso != null) torsoBase = torso.localRotation;
        }

        public void SetMoveSpeed(float value)
        {
            moveSpeed = Mathf.Clamp01(value / 5.2f);
        }

        public void PlayAttack()
        {
            if (!dead) attackTimer = 0.32f;
        }

        public void PlayHit()
        {
            if (!dead) hitTimer = 0.18f;
        }

        public void PlayAbility()
        {
            if (!dead) abilityTimer = 0.42f;
        }

        public void PlayHeavyAttack()
        {
            if (!dead)
            {
                heavyAttackTimer = 0.62f;
                attackTimer = 0.42f;
            }
        }

        public void SetDead(bool value)
        {
            dead = value;
            if (dead) moveSpeed = 0f;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            cycle += dt * runCycleSpeed * Mathf.Lerp(0.25f, 1f, moveSpeed);
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            hitTimer = Mathf.Max(0f, hitTimer - dt);
            abilityTimer = Mathf.Max(0f, abilityTimer - dt);
            heavyAttackTimer = Mathf.Max(0f, heavyAttackTimer - dt);

            float stride = Mathf.Sin(cycle) * runSwingAngle * moveSpeed;
            float armCounter = -stride;
            float attackWeight = attackTimer > 0f ? Mathf.Sin((1f - attackTimer / 0.42f) * Mathf.PI) : 0f;
            float abilityWeight = abilityTimer > 0f ? Mathf.Sin((1f - abilityTimer / 0.42f) * Mathf.PI) : 0f;
            float heavyWeight = heavyAttackTimer > 0f ? Mathf.Sin((1f - heavyAttackTimer / 0.62f) * Mathf.PI) : 0f;

            if (leftLeg != null) leftLeg.localRotation = leftLegBase * Quaternion.Euler(stride, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = rightLegBase * Quaternion.Euler(-stride, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = leftArmBase * Quaternion.Euler(armCounter + heavyWeight * -72f, 0f, 0f);
            float rightArmAngle = stride + attackWeight * -105f + abilityWeight * -55f + heavyWeight * -48f;
            if (rightArm != null) rightArm.localRotation = rightArmBase * Quaternion.Euler(rightArmAngle, 0f, 0f);

            float bob = dead ? -0.12f : Mathf.Abs(Mathf.Sin(cycle * 2f)) * idleBobAmount * (0.35f + moveSpeed);
            transform.localPosition = basePosition + Vector3.up * bob;
            if (torso != null)
            {
                float recoil = hitTimer > 0f ? Mathf.Sin((1f - hitTimer / 0.18f) * Mathf.PI) * 12f : 0f;
                torso.localRotation = torsoBase * Quaternion.Euler(recoil + abilityWeight * -8f + heavyWeight * 18f, 0f, 0f);
            }
        }
    }
}