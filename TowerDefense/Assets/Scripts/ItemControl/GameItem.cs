using System;
using CustomEnumControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemControl
{
    public class GameItem : MonoBehaviour
    {
        private Sprite _itemSprite;
        private Button _button;
        [SerializeField] private TMP_Text priceText;

        public event Action<ItemType, Sprite> OnOpenExplainPanelEvent;

        [field: SerializeField] public ItemType itemType { get; private set; }

        private void Awake()
        {
            _itemSprite = transform.GetChild(0).GetComponent<Image>().sprite;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnOpenExplainPanelEvent?.Invoke(itemType, _itemSprite);
            });
        }

        public void SetText(string price)
        {
            priceText.text = price;
        }
    }
}