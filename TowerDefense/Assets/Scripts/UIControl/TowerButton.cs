using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class TowerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action OnCamDisableEvent, OnCamEnableEvent;
        public event Action<int, Transform> OnPointerDownEvent;
        public event Action OnPointerUpEvent;
        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<int> OnStartPlacement;
        public event Action<PointerEventData> OnDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;
        public static bool IsOnButton { get; private set; }
        public byte buttonIndex { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsOnButton = true;
            UIManager.IsOnUI = true;
            OnPointerDownEvent?.Invoke(buttonIndex, transform);
            OnCamDisableEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UIManager.IsOnUI = false;
            OnPointerUpEvent?.Invoke();
            OnCamEnableEvent?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsOnButton = false;
            OnPointerUpEvent?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData);
            OnStartPlacement?.Invoke(buttonIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
        }
    }
}