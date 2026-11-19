using UnityEngine;

namespace ShinobiLegacy.Combat
{
    /// <summary>
    /// Small, decaying horizontal impulse for CharacterController-based actors.
    /// Applied in LateUpdate so normal player/AI movement runs first.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CombatantHitReaction : MonoBehaviour
    {
        private CharacterController controller;
        private Vector3 velocity;
        private float remaining;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void ApplyImpulse(Vector3 direction, float strength, float duration)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            if (controller == null || !controller.enabled || direction.sqrMagnitude < 0.0001f) return;

            Vector3 impulse = direction.normalized * Mathf.Clamp(strength, 0f, 5f);
            velocity = Vector3.ClampMagnitude(velocity + impulse, 5f);
            remaining = Mathf.Max(remaining, Mathf.Clamp(duration, 0.05f, 0.22f));
        }

        private void LateUpdate()
        {
            if (remaining <= 0f || controller == null || !controller.enabled)
            {
                velocity = Vector3.zero;
                remaining = 0f;
                return;
            }

            float step = Mathf.Min(Time.deltaTime, remaining);
            controller.Move(velocity * step);
            remaining = Mathf.Max(0f, remaining - step);
            velocity = Vector3.Lerp(velocity, Vector3.zero, step / Mathf.Max(0.001f, remaining + step));
        }

        private void OnDisable()
        {
            velocity = Vector3.zero;
            remaining = 0f;
        }
    }
}