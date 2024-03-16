using System;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UIControl;
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

        public event Action<ItemType> OnSetCurItemEvent;
        public event Action<Vector2> OnClickItemEvent;
        public event Action OnDisplayItemDescEvent;

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
            ItemBagController.isOnItemButton = true;

            OnSetCurItemEvent?.Invoke(itemType);
            OnDisplayItemDescEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            ItemBagController.isOnItemButton = false;
            OnClickItemEvent?.Invoke(_rectTransform.localPosition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween.PlayBackwards();
            ItemBagController.isOnItemButton = false;
        }
    }
}