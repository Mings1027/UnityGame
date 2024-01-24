using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ItemControl;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UIControl
{
    public class ItemBagController : MonoBehaviour
    {
        private Dictionary<ItemType, ItemButton> _itemDic;
        private Dictionary<ItemType, int> _itemCountDic;
        private ItemType _curItemType;
        private Button _button;
        private bool _isInventoryOpen;
        [SerializeField] private RectTransform itemParent;
        [SerializeField] private Image selectIcon;

        private void Start()
        {
            _button = GetComponent<Button>();
            selectIcon.GetComponent<Button>().onClick.AddListener(UseItem);
            selectIcon.gameObject.SetActive(false);
            itemParent.DOScaleY(0, 0);
            _button.onClick.AddListener(() =>
            {
                if (_isInventoryOpen)
                {
                    _isInventoryOpen = false;
                    selectIcon.gameObject.SetActive(false);
                    _curItemType = ItemType.None;
                    itemParent.DOScaleY(1, 0.25f).From(0).SetEase(Ease.OutBack);
                }
                else
                {
                    _isInventoryOpen = true;
                    itemParent.DOScaleY(0, 0.25f).From(1).SetEase(Ease.InBack);
                }
            });
            _itemDic = new Dictionary<ItemType, ItemButton>();
            _itemCountDic = new Dictionary<ItemType, int>();
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemParent.childCount; i++)
            {
                var itemButton = itemParent.GetChild(i).GetComponent<ItemButton>();
                _itemDic.Add(itemButton.itemType, itemButton);
                CustomLog.Log(itemButton.itemType);
                itemButton.OnSetCurItemEvent += SetCurItem;
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);

                _itemCountDic.Add(itemButton.itemType, itemInventory[itemButton.itemType.ToString()]);
                // itemCount += itemInventory[itemButton.itemType.ToString()];
            }

            gameObject.SetActive(false);
            // gameObject.SetActive(itemCount > 0);
            CustomLog.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            foreach (var itemDicKey in _itemDic.Keys)
            {
                CustomLog.Log(itemDicKey);
            }

            foreach (var itemCountKey in _itemCountDic.Keys)
            {
                CustomLog.Log(itemCountKey);
            }
        }

        private void SetCurItem(ItemType itemType, Vector2 anchoredPos)
        {
            CustomLog.Log($"아이템 누름 : {itemType}");
            if (!selectIcon.gameObject.activeSelf) selectIcon.gameObject.SetActive(true);
            selectIcon.rectTransform.anchoredPosition = anchoredPos;
            CustomLog.Log($"selectIcon anchoredPos : {selectIcon.rectTransform.anchoredPosition}");
            CustomLog.Log($"curItem pos : {anchoredPos}");
            _curItemType = itemType;
        }

        private void UseItem()
        {
            CustomLog.Log("아이템사용");
            _itemDic[_curItemType].Spawn();
            _itemDic[_curItemType].DecreaseItemCount();
            selectIcon.gameObject.SetActive(false);
            _itemCountDic[_curItemType] -= 1;
            _curItemType = ItemType.None;
        }

        public void UpdateInventory()
        {
            var itemTypes = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypes)
            {
                if (itemType == ItemType.None) continue;
                BackendGameData.userData.itemInventory[itemType.ToString()] = _itemCountDic[itemType];
                CustomLog.Log(BackendGameData.userData.itemInventory[itemType.ToString()]);
            }

            BackendGameData.instance.GameDataUpdate();
        }
    }
}