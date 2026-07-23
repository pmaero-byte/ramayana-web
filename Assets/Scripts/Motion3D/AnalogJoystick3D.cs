using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jambudweep.Ramayana.Motion3D
{
    public sealed class AnalogJoystick3D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private ThirdPersonMotionController controller;
        [SerializeField] private RectTransform knob;
        [SerializeField] private Image baseImage;
        [SerializeField] private Image knobImage;
        [SerializeField] private float radius = 54f;

        private RectTransform rect;
        private Vector2 startPosition;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            if (knob != null) startPosition = knob.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData) => UpdateInput(eventData);
        public void OnDrag(PointerEventData eventData) => UpdateInput(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            controller?.SetMobileInput(Vector2.zero);
            if (knob != null) knob.anchoredPosition = startPosition;
            SetPressed(false);
        }

        private void UpdateInput(PointerEventData eventData)
        {
            if (rect == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            Vector2 input = Vector2.ClampMagnitude(localPoint / radius, 1f);
            controller?.SetMobileInput(input);
            if (knob != null) knob.anchoredPosition = startPosition + input * radius;
            SetPressed(true);
        }

        private void SetPressed(bool pressed)
        {
            if (baseImage != null) baseImage.color = pressed ? new Color(0.38f, 0.18f, 0.07f, 0.82f) : new Color(0.12f, 0.055f, 0.025f, 0.58f);
            if (knobImage != null) knobImage.color = pressed ? new Color(1f, 0.76f, 0.34f, 0.92f) : new Color(0.95f, 0.82f, 0.55f, 0.78f);
        }
    }
}
