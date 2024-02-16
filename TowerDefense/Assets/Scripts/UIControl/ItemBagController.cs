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
        private Sequence _itemBagSequence;
        private Sequence _inventorySequence;
        private Dictionary<ItemType, ItemButton> _itemDic;
        private Dictionary<ItemType, int> _itemCountDic;
        private ItemType _curItemType;

        [SerializeField] private Button bagButton;
        [SerializeField] private CanvasGroup itemBagGroup;
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image selectIcon;

        private void Start()
        {
            selectIcon.GetComponent<Button>().onClick.AddListener(UseItem);
            selectIcon.gameObject.SetActive(false);
            bagButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OpenBag();
            });

            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                CloseBag();
            });
            _itemBagSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(itemBagGroup.DOFade(1, 0.25f).From(0))
                .Join(itemBagGroup.GetComponent<RectTransform>().DOAnchorPosX(150, 0.25f).From(new Vector2(-100, 0)));
            _itemBagSequence.OnComplete(() => itemBagGroup.blocksRaycasts = true);
            _itemBagSequence.OnRewind(() => itemBagGroup.blocksRaycasts = false);
            itemBagGroup.blocksRaycasts = false;

            _inventorySequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(itemGroup.DOFade(1, 0.25f).From(0))
                .Join(itemGroup.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            _inventorySequence.OnComplete(() => itemGroup.blocksRaycasts = true);
            _inventorySequence.OnRewind(() => itemGroup.blocksRaycasts = false);
            itemGroup.blocksRaycasts = false;

            _itemDic = new Dictionary<ItemType, ItemButton>();
            _itemCountDic = new Dictionary<ItemType, int>();
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemGroup.transform.childCount; i++)
            {
                var itemButton = itemGroup.transform.GetChild(0).GetChild(i).GetComponent<ItemButton>();
                _itemDic.Add(itemButton.itemType, itemButton);
                CustomLog.Log(itemButton.itemType);
                itemButton.OnSetCurItemEvent += SetCurItem;
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);

                _itemCountDic.Add(itemButton.itemType, itemInventory[itemButton.itemType.ToString()]);
            }
        }

        private void OnDestroy()
        {
            _inventorySequence?.Kill();
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

        private void OpenBag()
        {
            _itemBagSequence.PlayBackwards();
            _inventorySequence.Restart();
        }

        private void CloseBag()
        {
            selectIcon.gameObject.SetActive(false);
            _itemBagSequence.Restart();
            _inventorySequence.PlayBackwards();
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

        public void SetActiveItemBag(bool active)
        {
            if (active)
            {
                _itemBagSequence.Restart();
            }
            else
            {
                _itemBagSequence.PlayBackwards();
                _inventorySequence.PlayBackwards();
            }

            gameObject.SetActive(active);
        }
    }
}