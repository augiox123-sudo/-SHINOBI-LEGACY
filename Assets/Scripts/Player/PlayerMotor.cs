using UnityEngine;

namespace ShinobiLegacy.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private Transform cameraTransform;
        private CharacterController controller;
        private Vector2 externalMoveInput;
        private bool movementEnabled = true;

        public Vector2 MoveInput
        {
            get => externalMoveInput;
            set => externalMoveInput = Vector2.ClampMagnitude(value, 1f);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        public void SetMovementEnabled(bool enabled) { movementEnabled = enabled; }

        private void Update()
        {
            if (!movementEnabled) return;
            Vector2 input = externalMoveInput;
#if UNITY_EDITOR || UNITY_STANDALONE
            Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (keyboard.sqrMagnitude > 0.001f) input = Vector2.ClampMagnitude(keyboard, 1f);
#endif
            Vector3 forward = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 right = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized
                : Vector3.right;
            Vector3 direction = right * input.x + forward * input.y;
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            Vector3 velocity = direction * moveSpeed;
            velocity.y = -2f;
            controller.Move(velocity * Time.deltaTime);

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
