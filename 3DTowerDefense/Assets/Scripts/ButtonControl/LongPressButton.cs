using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButtonControl
{
    public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool _isShortPressed, _isLongPressed;
        public float checkTime;

        [SerializeField] private Slider slider;
        [SerializeField] private float holdDuration;
        [SerializeField] private UnityEvent<int> onShortPress, onLongPress;


        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isShortPressed) return;
            _isShortPressed = true;
            checkTime = 0;
            slider.value = 0;
            slider.maxValue = holdDuration;
        }

        private void Update() //holdDuration 보다 길게 누르면 OnLongPressEvent 수행
        {
            if (!_isShortPressed) return;

            if (checkTime < holdDuration)
            {
                checkTime += Time.deltaTime;
                slider.value = checkTime;
            }
            else
            {
                _isLongPressed = true;
                onLongPress?.Invoke(transform.GetSiblingIndex());
                _isShortPressed = false;
            }
        }

        public void OnPointerUp(PointerEventData eventData) //holdDuration 보다 짧게 누렀다때면 OnShortPressEvent 수행
        {
            slider.value = 0;
            if (_isShortPressed)
                _isShortPressed = false;
            if (!_isLongPressed)
            {
                onShortPress?.Invoke(transform.GetSiblingIndex());
            }

            _isLongPressed = false;
        }
    }
}