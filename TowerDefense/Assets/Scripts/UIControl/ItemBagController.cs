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
        private TowerCardController _towerCardController;
        private Tween _disappearItemBagTween;
        private Tween _itemBagTween;
        private Dictionary<ItemType, ItemButton> _itemDic;
        private Dictionary<ItemType, int> _itemCountDic;
        private ItemType _curItemType;
        private bool _isOpenBag;

        [SerializeField] private Button bagButton;
        [SerializeField] private CanvasGroup itemBagGroup;
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Image selectIcon;

        private void Start()
        {
            _towerCardController = FindAnyObjectByType<TowerCardController>();
            selectIcon.GetComponent<Button>().onClick.AddListener(UseItem);
            selectIcon.gameObject.SetActive(false);
            bagButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isOpenBag)
                {
                    _isOpenBag = false;
                    CloseBag();
                }
                else
                {
                    _isOpenBag = true;
                    OpenBag();
                }
            });

            _disappearItemBagTween = itemBagGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _disappearItemBagTween.OnComplete(() => itemBagGroup.blocksRaycasts = true);
            itemBagGroup.blocksRaycasts = false;

            _itemBagTween = itemGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _itemBagTween.OnComplete(() => itemGroup.blocksRaycasts = true);
            itemGroup.blocksRaycasts = false;

            _itemDic = new Dictionary<ItemType, ItemButton>();
            _itemCountDic = new Dictionary<ItemType, int>();
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemGroup.transform.childCount; i++)
            {
                var itemButton = itemGroup.transform.GetChild(i).GetComponent<ItemButton>();
                _itemDic.Add(itemButton.itemType, itemButton);
                CustomLog.Log(itemButton.itemType);
                itemButton.OnSetCurItemEvent += SetCurItem;
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);

                _itemCountDic.Add(itemButton.itemType, itemInventory[itemButton.itemType.ToString()]);
            }

            var itemCount = itemGroup.transform.childCount;
            var lastItemPosX = itemGroup.transform.GetChild(itemCount - 1).GetComponent<RectTransform>().position.x;
        }

        private void OnDestroy()
        {
            _itemBagTween?.Kill();
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
            _itemBagTween.Restart();
            if (!SafeArea.activeSafeArea)
            {
                _towerCardController.scaleTween.Restart();
            }
        }

        private void CloseBag()
        {
            selectIcon.gameObject.SetActive(false);
            _itemBagTween.PlayBackwards();
            _curItemType = ItemType.None;
            if (!SafeArea.activeSafeArea)
            {
                _towerCardController.scaleTween.PlayBackwards();
            }
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
                _disappearItemBagTween.Restart();
            }
            else
            {
                _isOpenBag = false;
                itemBagGroup.blocksRaycasts = false;
                itemGroup.blocksRaycasts = false;
                _disappearItemBagTween.PlayBackwards();
                _itemBagTween.PlayBackwards();
                if (!SafeArea.activeSafeArea)
                {
                    _towerCardController.scaleTween.PlayBackwards();
                }
            }
        }
    }
}