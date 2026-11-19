using UnityEngine;

namespace ShinobiLegacy.CameraSystem
{
    public sealed class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 11f, -8f);
        [SerializeField, Range(2f, 15f)] private float followSharpness = 8f;
        [SerializeField] private float lookHeight = 1f;

        public Transform Target { get => target; set => target = value; }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.position + offset;
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, blend);
            }
        }
    }
}
