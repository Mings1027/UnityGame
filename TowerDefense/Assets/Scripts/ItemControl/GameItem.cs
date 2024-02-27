using System;
using CustomEnumControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ItemControl
{
    public class GameItem : MonoBehaviour
    {
        private Button _button;
        
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text priceText;

        public event Action<ItemType, Sprite> OnOpenExplainPanelEvent;

        [field: SerializeField] public ItemType itemType { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnOpenExplainPanelEvent?.Invoke(itemType, itemImage.sprite);
            });
        }

        public void SetText(string price)
        {
            priceText.text = price;
        }
    }
}