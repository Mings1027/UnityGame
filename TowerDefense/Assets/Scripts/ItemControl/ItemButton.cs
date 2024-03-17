using System;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemControl
{
    public abstract class ItemButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Tween _scaleTween;
        private TMP_Text _remainingText;
        private int _remainingCount;
        private Button _button;
        private RectTransform _rectTransform;

        protected CameraManager cameraManager;

        public event Action OnPointerDownEvent;
        public event Action OnPointerUpEvent;
        public event Action<ItemType, Vector2> OnClickEvent;

        [field: SerializeField] public ItemType itemType { get; protected set; }

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _remainingText = transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
            _button = GetComponent<Button>();
            _scaleTween = transform.DOScale(1.1f, 0.25f).From(1).SetAutoKill(false).Pause();
        }

        protected virtual void Start()
        {
            cameraManager = FindAnyObjectByType<CameraManager>();
        }

        public abstract void Spawn();

        public void SetRemainingText(int amount)
        {
            _remainingCount = amount;
            _remainingText.text = amount.ToString();
        }

        public void DecreaseItemCount()
        {
            _remainingCount -= 1;
            _remainingText.text = _remainingCount.ToString();
            if (_remainingCount <= 0)
            {
                _button.interactable = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _scaleTween.Restart();
            OnPointerDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            OnPointerUpEvent?.Invoke();
            OnClickEvent?.Invoke(itemType, _rectTransform.localPosition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
        }
    }
}