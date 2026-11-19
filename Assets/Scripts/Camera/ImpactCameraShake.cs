using UnityEngine;

namespace ShinobiLegacy.CameraSystem
{
    /// <summary>
    /// Small additive impact shake. Runs after the isometric follow camera and
    /// removes its previous offset before applying the next frame's offset.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class ImpactCameraShake : MonoBehaviour
    {
        private float remaining;
        private float duration;
        private float strength;
        private Vector3 previousOffset;

        public void Shake(float intensity, float seconds)
        {
            strength = Mathf.Max(strength, Mathf.Clamp(intensity, 0f, 0.35f));
            duration = Mathf.Max(duration, Mathf.Clamp(seconds, 0.05f, 0.4f));
            remaining = duration;
        }

        private void LateUpdate()
        {
            transform.position -= previousOffset;
            previousOffset = Vector3.zero;

            if (remaining <= 0f)
            {
                strength = 0f;
                duration = 0f;
                return;
            }

            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            float fade = duration > 0f ? remaining / duration : 0f;
            float time = Time.time * 43f;
            float horizontal = Mathf.Sin(time) * strength * fade;
            float vertical = Mathf.Cos(time * 1.37f) * strength * 0.55f * fade;
            previousOffset = transform.right * horizontal + transform.up * vertical;
            transform.position += previousOffset;
        }

        private void OnDisable()
        {
            transform.position -= previousOffset;
            previousOffset = Vector3.zero;
            remaining = 0f;
            strength = 0f;
        }
    }
}