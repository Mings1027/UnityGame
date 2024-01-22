using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyControl
{
    public class ItemShopController : MonoBehaviour
    {
        private ItemType _curItemType;
        private string _curProductID;
        private DiamondShopController _diamondShopController;
        private Dictionary<ItemType, string> _itemDic;
        [SerializeField] private Button itemShopButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text diamondText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Transform shopPanel;
        [SerializeField] private Transform itemParent;
        [SerializeField] private TMP_Text explainText;

        private void Awake()
        {
            _diamondShopController = FindAnyObjectByType<DiamondShopController>();
            _itemDic = new Dictionary<ItemType, string>();
            backgroundBlockImage.enabled = false;
            shopPanel.localScale = Vector3.zero;
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
                backgroundBlockImage.enabled = false;
                shopBlockImage.enabled = true;
                shopPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
            });
            var itemTypes = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypes)
            {
                if (itemType == ItemType.None) continue;
                _itemDic.Add(itemType,
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + itemType));
            }

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemDic;
        }

        private void Start()
        {
            ItemInit();
        }

        private void ItemInit()
        {
            for (int i = 0; i < itemParent.childCount; i++)
            {
                var uuid = Backend.TBC.GetProductList().Rows()[i]["uuid"]["S"].ToString();
                var itemName = Backend.TBC.GetProductList().Rows()[i]["name"]["S"].ToString();
                var item = itemParent.GetChild(i).GetComponent<Item>();
                item.ItemInit(uuid, itemName);
                item.OnSetExplainTextEvent += SetCurItem;
            }

            diamondText.text = _diamondShopController.SetDiamonds();

            buyButton.onClick.AddListener(BuyItem);
        }

        private void SetCurItem(ItemType itemType, string productID)
        {
            _curItemType = itemType;
            _curProductID = productID;
            explainText.text = _itemDic[itemType];
        }

        private void BuyItem()
        {
            Backend.TBC.UseTBC(_curProductID, $"{_curItemType} 구매");
            diamondText.text = _diamondShopController.SetDiamonds();
        }

        private void ChangeLocaleItemDic(Locale locale)
        {
            foreach (var itemName in _itemDic.Keys.ToList())
            {
                _itemDic[itemName] =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + itemName);
            }

            LocaleManager.ChangeLocaleAsync(LocaleManager.ItemTable, _curItemType.ToString(), explainText).Forget();
        }
    }

    // public class ProductItem
    // {
    //     public string TBC;
    //     public string inDate;
    //     public string uuid;
    //     public string name;
    //     public string explain;
    //
    //     public override string ToString()
    //     {
    //         return $"TBC : {TBC}\ninDate : {inDate}\nuuid : {uuid}\nname : {name}\nexplain : {explain}\n";
    //     }
    // }
}