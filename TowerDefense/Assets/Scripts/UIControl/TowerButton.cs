using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class TowerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Tween _scaleTween;
        public event Action OnCamDisableEvent, OnCamEnableEvent;
        public event Action<int, Transform> OnPointerDownEvent;
        public event Action OnPointerUpEvent;
        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<int> OnStartPlacement;
        public event Action<PointerEventData> OnDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;
        public static bool IsOnButton { get; private set; }
        public byte buttonIndex { get; set; }

        private void Awake()
        {
            _scaleTween = transform.DOScale(1.1f, 0.25f).From(1).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _scaleTween.Restart();
            IsOnButton = true;
            UIManager.IsOnUI = true;
            OnPointerDownEvent?.Invoke(buttonIndex, transform);
            OnCamDisableEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            UIManager.IsOnUI = false;
            OnPointerUpEvent?.Invoke();
            OnCamEnableEvent?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
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