using System;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class Item : MonoBehaviour
    {
        private string _productID;
        private string _itemName;
        private Button _button;
        public event Action<ItemType, string> OnSetExplainTextEvent;

        [SerializeField] private ItemType itemType;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                Debug.Log($"productID : {_productID}");
                Debug.Log($"itemName : {_itemName}");

                OnSetExplainTextEvent?.Invoke(itemType, _productID);
            });
        }

        public void ItemInit(string productID, string itemName)
        {
            _productID = productID;
            _itemName = itemName;
        }
    }
}