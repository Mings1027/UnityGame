using System;
using CustomEnumControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemControl
{
    public abstract class ItemButton : MonoBehaviour
    {
        private TMP_Text _remainingText;
        private int _remainingCount;
        private Button _button;

        protected CameraManager cameraManager;

        public event Action<ItemType, Vector2> OnSetCurItemEvent;

        public ItemType itemType { get; protected set; }

        protected virtual void Awake()
        {
            _remainingText = transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
            _button = GetComponent<Button>();
            var rectTransform = GetComponent<RectTransform>();
            _button.onClick.AddListener(() => { OnSetCurItemEvent?.Invoke(itemType, rectTransform.anchoredPosition); });
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
    }
}