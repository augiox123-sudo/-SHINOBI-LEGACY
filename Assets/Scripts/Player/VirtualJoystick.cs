using UnityEngine;
using UnityEngine.EventSystems;

namespace ShinobiLegacy.Player
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private PlayerMotor player;
        [SerializeField, Range(0.1f, 1f)] private float deadZone = 0.12f;

        private Vector2 input;
        private int activePointerId = int.MinValue;

        public void Configure(RectTransform baseRect, RectTransform handleRect, PlayerMotor motor)
        {
            background = baseRect;
            handle = handleRect;
            player = motor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue) return;
            activePointerId = eventData.pointerId;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            if (background == null || handle == null || player == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float radius = Mathf.Min(background.rect.width, background.rect.height) * 0.5f;
            Vector2 rawInput = Vector2.ClampMagnitude(localPoint / Mathf.Max(1f, radius), 1f);
            float inputMagnitude = rawInput.magnitude;
            if (inputMagnitude <= deadZone)
                input = Vector2.zero;
            else
                input = rawInput.normalized * Mathf.InverseLerp(deadZone, 1f, inputMagnitude);

            handle.anchoredPosition = input * radius * 0.62f;
            player.MoveInput = input;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            ResetInput();
        }

        private void OnDisable()
        {
            ResetInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ResetInput();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) ResetInput();
        }

        private void ResetInput()
        {
            input = Vector2.zero;
            activePointerId = int.MinValue;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            if (player != null) player.MoveInput = Vector2.zero;
        }
    }
}