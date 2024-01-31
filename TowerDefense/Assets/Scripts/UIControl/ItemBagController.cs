using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ItemControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UIControl
{
    public class ItemBagController : MonoBehaviour
    {
        private Tween _inventoryTween;
        private Dictionary<ItemType, ItemButton> _itemDic;
        private Dictionary<ItemType, int> _itemCountDic;
        private ItemType _curItemType;
        private Button _button;
        private bool _isInventoryOpen;
        [SerializeField] private RectTransform itemParent;
        [SerializeField] private Image selectIcon;

        private void Start()
        {
            _inventoryTween = itemParent.DOAnchorPosX(0, 0.25f).From(new Vector2(-300, 0)).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
            _button = GetComponent<Button>();
            selectIcon.GetComponent<Button>().onClick.AddListener(UseItem);
            selectIcon.gameObject.SetActive(false);
            _button.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isInventoryOpen)
                {
                    Debug.Log("배낭 닫기");
                    _isInventoryOpen = false;
                    selectIcon.gameObject.SetActive(false);
                    _curItemType = ItemType.None;
                    _inventoryTween.PlayBackwards();
                }
                else
                {
                    Debug.Log("배낭 열기");
                    _isInventoryOpen = true;
                    _inventoryTween.Restart();
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
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _inventoryTween?.Kill();
        }

        private void SetCurItem(ItemType curItemType, Vector2 anchoredPos)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_itemCountDic[curItemType] <= 0) return;
            selectIcon.gameObject.SetActive(true);
            selectIcon.rectTransform.anchoredPosition = anchoredPos;
            _curItemType = curItemType;
        }

        private void UseItem()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_itemCountDic[_curItemType] <= 0) return;
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
            }
        }
    }
}