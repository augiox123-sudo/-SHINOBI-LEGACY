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

        public void Configure(RectTransform baseRect, RectTransform handleRect, PlayerMotor motor)
        {
            background = baseRect;
            handle = handleRect;
            player = motor;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null || player == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float radius = Mathf.Min(background.rect.width, background.rect.height) * 0.5f;
            input = Vector2.ClampMagnitude(localPoint / Mathf.Max(1f, radius), 1f);
            if (input.magnitude < deadZone) input = Vector2.zero;
            handle.anchoredPosition = input * radius * 0.62f;
            player.MoveInput = input;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            input = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            if (player != null) player.MoveInput = Vector2.zero;
        }
    }
}
