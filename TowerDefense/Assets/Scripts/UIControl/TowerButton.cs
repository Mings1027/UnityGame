using System;
using CustomEnumControl;
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
        public event Action<TowerType> OnOpenCardEvent;
        public event Action OnCloseCardEvent;
        public event Action<TowerType> OnStartPlacement;
        public event Action<PointerEventData> OnDragEvent;
        public event Action OnTryPlaceTowerEvent;
        public static bool isOnButton { get; private set; }
        
        [SerializeField] private TowerType towerType;

        private void Awake()
        {
            _scaleTween = transform.DOScale(1.1f, 0.25f).From(1).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _scaleTween.Restart();
            isOnButton = true;
            OnCamDisableEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            OnOpenCardEvent?.Invoke(towerType);
            OnCamEnableEvent?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            isOnButton = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnStartPlacement?.Invoke(towerType);
            OnCloseCardEvent?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isOnButton) return;
            OnTryPlaceTowerEvent?.Invoke();
        }
    }
}