using UnityEngine;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private float cooldown = 1.25f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashSpeed = 10.5f;
        [SerializeField] private float invulnerabilityDuration = 0.18f;
        private CharacterController controller;
        private PlayerMotor motor;
        private CombatantHealth health;
        private Transform cameraTransform;
        private Vector3 dashDirection;
        private float dashEndsAt;
        private float nextDodgeAt;
        private bool dashing;

        public float CooldownRemaining => Mathf.Max(0f, nextDodgeAt - Time.time);
        public float CooldownDuration => cooldown;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            motor = GetComponent<PlayerMotor>();
            health = GetComponent<CombatantHealth>();
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        public bool TryDodge()
        {
            if (Time.time < nextDodgeAt || (health != null && health.IsDead) || !controller.enabled)
                return false;
            Vector2 input = motor != null ? motor.MoveInput : Vector2.zero;
            Vector3 forward = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized
                : Vector3.right;
            Vector3 direction = right * input.x + forward * input.y;
            if (direction.sqrMagnitude < 0.01f) direction = transform.forward;
            dashDirection = direction.normalized;
            transform.rotation = Quaternion.LookRotation(dashDirection, Vector3.up);
            nextDodgeAt = Time.time + cooldown;
            dashEndsAt = Time.time + dashDuration;
            dashing = true;
            if (health != null) health.GrantTemporaryInvulnerability(invulnerabilityDuration);
            return true;
        }

        private void LateUpdate()
        {
            if (!dashing) return;
            if (Time.time >= dashEndsAt || !controller.enabled)
            {
                dashing = false;
                return;
            }
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        }

        private void OnDisable()
        {
            dashing = false;
        }
    }
}