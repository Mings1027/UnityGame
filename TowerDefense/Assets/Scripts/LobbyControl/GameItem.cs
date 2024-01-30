using System;
using CustomEnumControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class GameItem : MonoBehaviour
    {
        private string _productID;
        private Sprite _itemSprite;
        private Button _button;
        public event Action<Sprite> OnOpenExplainPanelEvent;
        public event Action<ItemType, string> OnCurItemEvent;

        [field: SerializeField] public ItemType itemType { get; private set; }

        private void Awake()
        {
            _itemSprite = transform.GetChild(0).GetComponent<Image>().sprite;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnCurItemEvent?.Invoke(itemType, _productID);
                OnOpenExplainPanelEvent?.Invoke(_itemSprite);
            });
        }

        public void ItemInit(string productId)
        {
            _productID = productId;
        }
    }
}