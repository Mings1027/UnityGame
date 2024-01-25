using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utilities;

namespace LobbyControl
{
    public class ItemShopController : MonoBehaviour
    {
        private class ItemInfo
        {
            public string itemExplain;
            public readonly int itemTbc;

            public ItemInfo(string itemExplain, int itemTbc)
            {
                this.itemExplain = itemExplain;
                this.itemTbc = itemTbc;
            }
        }

        private int _curTbc;
        private ItemInfo _itemInfo;
        private Sequence _notEnoughDiaSequence;
        private ItemType _curItemType;
        private string _curProductID;
        private DiamondShopController _diamondShopController;
        private Dictionary<ItemType, ItemInfo> _itemInfoDic;
        private Dictionary<ItemType, TMP_Text> _itemCountDic;

        [SerializeField] private Button itemShopButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text diamondText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Transform shopPanel;
        [SerializeField] private Transform itemParent;
        [SerializeField] private TMP_Text explainText;
        [SerializeField] private Transform notEnoughDiaText;
        [SerializeField] private Image selectObj;

        private void Awake()
        {
            _itemInfoDic = new Dictionary<ItemType, ItemInfo>();
            _itemCountDic = new Dictionary<ItemType, TMP_Text>();
            backgroundBlockImage.enabled = false;
            shopPanel.localScale = Vector3.zero;
            selectObj.enabled = false;
            itemShopButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                backgroundBlockImage.enabled = true;
                shopBlockImage.enabled = false;
                shopPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                // selectObj.enabled = false;
                backgroundBlockImage.enabled = false;
                shopBlockImage.enabled = true;
                shopPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
            });

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemDic;
        }

        private void Start()
        {
            ItemInit();
            _notEnoughDiaSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughDiaText.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(notEnoughDiaText.DOScaleX(0, 0.25f).From(1).SetEase(Ease.InBack).SetDelay(1));
            notEnoughDiaText.DOScaleX(0, 0);
        }

        private void ItemInit()
        {
            _diamondShopController = FindAnyObjectByType<DiamondShopController>();
            CustomLog.Log($"플레이어 데이터: {BackendGameData.userData}");
            CustomLog.Log($"플레이어 아이템 딕: {BackendGameData.userData.itemInventory}");
            var itemInventory = BackendGameData.userData.itemInventory;
            for (var i = 0; i < itemParent.childCount; i++)
            {
                var uuid = Backend.TBC.GetProductList().Rows()[i]["uuid"]["S"].ToString();
                var itemTbc = Backend.TBC.GetProductList().Rows()[i]["TBC"]["N"].ToString();
                var item = itemParent.GetChild(i).GetComponent<GameItem>();
                item.ItemInit(uuid);
                item.OnCurItemEvent += SetCurItem;

                _itemCountDic.Add(item.itemType, item.transform.GetChild(2).GetComponent<TMP_Text>());
                _itemCountDic[item.itemType].text = itemInventory[item.itemType.ToString()].ToString();

                _itemInfoDic.Add(item.itemType, new ItemInfo(
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + item.itemType),
                    int.Parse(itemTbc)));
                CustomLog.Log($"아이템 타입 : {item.itemType}   가격 : {itemTbc}");
            }

            CustomLog.Log("아이템 초기화");
            diamondText.text = _diamondShopController.diamondText.text;
            buyButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                BuyItem();
            });
            SetDiamondText();
        }

        private void SetCurItem(ItemType itemType, string productID, Vector2 anchoredPos)
        {
            _curItemType = itemType;
            _curProductID = productID;
            if (!selectObj.enabled) selectObj.enabled = true;
            selectObj.rectTransform.anchoredPosition = anchoredPos;
            explainText.text = _itemInfoDic[itemType].itemExplain;
            CustomLog.Log($"아이템 설명 : {_itemInfoDic[itemType].itemExplain}");
        }

        private void BuyItem()
        {
            if (_curItemType == ItemType.None) return;
            if (_itemInfoDic[_curItemType].itemTbc < _curTbc)
            {
                buyButton.interactable = false;
                var useTbc = Backend.TBC.UseTBC(_curProductID, $"{_curItemType} 구매");
                if (useTbc.IsSuccess())
                {
                    SetDiamondText();

                    var itemCount = BackendGameData.userData.itemInventory[_curItemType.ToString()] += 1;
                    CustomLog.Log($"itemCount : {itemCount}");
                    _itemCountDic[_curItemType].text = itemCount.ToString();
                    CustomLog.Log(_itemCountDic[_curItemType].name);
                    buyButton.interactable = true;
                }
            }
            else
            {
                _notEnoughDiaSequence.Restart();
            }
        }

        public void SetDiamondText()
        {
            var bro = Backend.TBC.GetTBC();
            if (bro.IsSuccess())
            {
                CustomLog.Log("tbc 찾음");
                var amountTbc = int.Parse(bro.GetReturnValuetoJSON()["amountTBC"].ToString());
                diamondText.text = amountTbc.ToString();
                _diamondShopController.diamondText.text = amountTbc.ToString();
                _curTbc = amountTbc;
            }
            else
            {
                CustomLog.Log("tbc 못찾음");
            }
        }

        private void ChangeLocaleItemDic(Locale locale)
        {
            foreach (var itemName in _itemInfoDic.Keys.ToList())
            {
                _itemInfoDic[itemName].itemExplain =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + itemName);
            }

            LocaleManager.ChangeLocaleAsync(LocaleManager.ItemTable, _curItemType.ToString(), explainText).Forget();
        }
    }
}