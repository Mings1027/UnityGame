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
        private Button _button;
        public event Action<ItemType, string, Vector2> OnCurItemEvent;

        [field: SerializeField] public ItemType itemType { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            var rectTransform = GetComponent<RectTransform>();
            _button.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnCurItemEvent?.Invoke(itemType, _productID, rectTransform.anchoredPosition);
            });
        }

        public void ItemInit(string productId)
        {
            _productID = productId;
        }
    }
}