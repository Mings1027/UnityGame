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
        public event Action<TowerType> OnOpenCardEvent;
        public event Action<TowerType> OnStartPlacement;
        public event Action<bool> OnPointerOverEvent; 
        public event Action OnCamDisableEvent, OnCamEnableEvent;
        public event Action OnCloseCardEvent;
        public event Action OnTryPlaceTowerEvent;
        
        [SerializeField] private TowerType towerType;

        private void Awake()
        {
            _scaleTween = transform.DOScale(1.1f, 0.25f).From(1).SetAutoKill(false).Pause();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _scaleTween.Restart();
            OnPointerOverEvent?.Invoke(true);
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
            OnPointerOverEvent?.Invoke(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnStartPlacement?.Invoke(towerType);
            OnCloseCardEvent?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnTryPlaceTowerEvent?.Invoke();
        }
    }
}