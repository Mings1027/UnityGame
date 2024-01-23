using System;
using CustomEnumControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public abstract class ItemButton : MonoBehaviour
    {
        private TMP_Text _remainingText;
        private int _remainingCount;
        private Button _button;

        [field: SerializeField] public ItemType itemType;
        public event Action<ItemType, Vector2> OnSetCurItemEvent;

        private void Awake()
        {
            _remainingText = transform.GetChild(1).GetComponent<TMP_Text>();
            _button = GetComponent<Button>();
            var rectTransform = GetComponent<RectTransform>();
            _button.onClick.AddListener(() => { OnSetCurItemEvent?.Invoke(itemType, rectTransform.anchoredPosition); });
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
            if (_remainingCount <= 0)
            {
                _button.interactable = false;
            }
        }
    }
}