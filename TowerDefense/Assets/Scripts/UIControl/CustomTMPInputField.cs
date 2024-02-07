using System;
using TMPro;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class CustomTMPInputField : TMP_InputField
    {
        public event Action OnPointerDownEvent, OnPointerUpEvent;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            base.OnPointerDown(eventData);
            OnPointerDownEvent?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            base.OnPointerUp(eventData);
            OnPointerUpEvent?.Invoke();
        }
    }
}