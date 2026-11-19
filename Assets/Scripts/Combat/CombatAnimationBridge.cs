using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShinobiLegacy.Combat
{
    /// <summary>
    /// Optional bridge between gameplay events and an authored Animator Controller.
    /// It safely ignores missing Animators and parameters, so placeholder characters remain playable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private ShinobiProceduralMotion proceduralMotion;
        private readonly HashSet<int> floatParameters = new HashSet<int>();
        private readonly HashSet<int> boolParameters = new HashSet<int>();
        private readonly HashSet<int> triggerParameters = new HashSet<int>();
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int AbilityHash = Animator.StringToHash("Ability");
        private bool parametersCached;
        private Coroutine attackStateRoutine;

        private ShinobiProceduralMotion ResolveProceduralMotion()
        {
            if (proceduralMotion == null) proceduralMotion = GetComponentInChildren<ShinobiProceduralMotion>();
            return proceduralMotion;
        }

        private Animator ResolveAnimator()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null && !parametersCached) CacheParameters();
            return animator;
        }

        private void CacheParameters()
        {
            floatParameters.Clear();
            boolParameters.Clear();
            triggerParameters.Clear();
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float) floatParameters.Add(parameter.nameHash);
                else if (parameter.type == AnimatorControllerParameterType.Bool) boolParameters.Add(parameter.nameHash);
                else if (parameter.type == AnimatorControllerParameterType.Trigger) triggerParameters.Add(parameter.nameHash);
            }
            parametersCached = true;
        }

        public void SetMoveSpeed(float speed)
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.SetMoveSpeed(speed);
            Animator current = ResolveAnimator();
            if (current != null && floatParameters.Contains(MoveSpeedHash))
                current.SetFloat(MoveSpeedHash, Mathf.Max(0f, speed), 0.12f, Time.deltaTime);
        }

        public void SetAttacking(bool value)
        {
            Animator current = ResolveAnimator();
            if (current != null && boolParameters.Contains(IsAttackingHash))
                current.SetBool(IsAttackingHash, value);
        }

        public void SetDead(bool value)
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.SetDead(value);
            if (value && attackStateRoutine != null)
            {
                StopCoroutine(attackStateRoutine);
                attackStateRoutine = null;
            }
            Animator current = ResolveAnimator();
            if (current != null && boolParameters.Contains(IsDeadHash))
                current.SetBool(IsDeadHash, value);
        }

        public void PlayWindup(bool heavy)
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayWindup(heavy);
        }

        public void CancelWindup()
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.CancelWindup();
        }

        public void PlayAttack(float activeDuration = 0.28f)
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayAttack();
            Trigger(AttackHash);
            if (attackStateRoutine != null) StopCoroutine(attackStateRoutine);
            if (ResolveAnimator() != null && boolParameters.Contains(IsAttackingHash))
                attackStateRoutine = StartCoroutine(AttackState(activeDuration));
        }

        private IEnumerator AttackState(float duration)
        {
            SetAttacking(true);
            yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
            SetAttacking(false);
            attackStateRoutine = null;
        }

        private void OnDisable()
        {
            if (attackStateRoutine != null)
            {
                StopCoroutine(attackStateRoutine);
                attackStateRoutine = null;
            }
            if (animator != null && parametersCached && boolParameters.Contains(IsAttackingHash))
                animator.SetBool(IsAttackingHash, false);
        }

        public void TriggerHit()
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayHit();
            Trigger(HitHash);
        }

        public void TriggerHeavyHit()
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayHit(true);
            Trigger(HitHash);
        }

        public void TriggerAbility()
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayAbility();
            Trigger(AbilityHash);
        }

        public void PlayHeavyAttack()
        {
            ShinobiProceduralMotion motion = ResolveProceduralMotion();
            if (motion != null) motion.PlayHeavyAttack();
            Trigger(AttackHash);
            if (attackStateRoutine != null) StopCoroutine(attackStateRoutine);
            if (ResolveAnimator() != null && boolParameters.Contains(IsAttackingHash))
                attackStateRoutine = StartCoroutine(AttackState(0.48f));
        }

        private void Trigger(int parameterHash)
        {
            Animator current = ResolveAnimator();
            if (current != null && triggerParameters.Contains(parameterHash))
                current.SetTrigger(parameterHash);
        }
    }
}