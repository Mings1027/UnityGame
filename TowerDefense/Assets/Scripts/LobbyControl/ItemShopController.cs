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
            public readonly TMP_Text itemCountText;

            public ItemInfo(string itemExplain, int itemTbc, TMP_Text itemCountText)
            {
                this.itemExplain = itemExplain;
                this.itemTbc = itemTbc;
                this.itemCountText = itemCountText;
            }
        }

        private ItemInfo _itemInfo;
        private Sequence _notEnoughDiaSequence;
        private ItemType _curItemType;
        private string _curProductID;
        private Dictionary<ItemType, ItemInfo> _itemInfoTable;
        private CurrencyController _currencyController;

        [SerializeField] private GameObject buttons;
        [SerializeField] private RectTransform inGameMoney;
        [SerializeField] private Button itemShopButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private Button purchaseButton;

        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Transform shopPanel;
        [SerializeField] private Image explainBlockImage;
        [SerializeField] private Button closeExplainPanelButton;
        [SerializeField] private Transform explainPanel;
        [SerializeField] private Transform itemParent;
        [SerializeField] private Image explainImage;
        [SerializeField] private TMP_Text explainText;
        [SerializeField] private Transform needMoreDiaText;

        private void Awake()
        {
            _currencyController = FindAnyObjectByType<CurrencyController>();
            _itemInfoTable = new Dictionary<ItemType, ItemInfo>();
            backgroundBlockImage.enabled = false;
            shopPanel.localScale = Vector3.zero;
            explainBlockImage.enabled = false;
            explainPanel.localScale = Vector3.zero;
            itemShopButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                backgroundBlockImage.enabled = true;
                shopBlockImage.enabled = false;
                shopPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
                buttons.SetActive(false);
                inGameMoney.SetParent(transform);
                _currencyController.Off();
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                backgroundBlockImage.enabled = false;
                shopBlockImage.enabled = true;
                shopPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
                buttons.SetActive(true);
                inGameMoney.SetParent(buttons.transform);
                _currencyController.On();
            });

            closeExplainPanelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                explainBlockImage.enabled = false;
                explainPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
            });

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemDic;
        }

        private void Start()
        {
            ItemInit();
            _notEnoughDiaSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(needMoreDiaText.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(needMoreDiaText.DOScaleX(0, 0.25f).From(1).SetEase(Ease.InBack).SetDelay(1));
            needMoreDiaText.DOScaleX(0, 0);
        }

        private void ItemInit()
        {
            CustomLog.Log($"플레이어 데이터: {BackendGameData.userData}");
            CustomLog.Log($"플레이어 아이템 테이블: {BackendGameData.userData.itemInventory}");
            var itemInventory = BackendGameData.userData.itemInventory;
            var productList = Backend.TBC.GetProductList().Rows();
            for (var i = 0; i < itemParent.childCount; i++)
            {
                var productId = productList[i]["uuid"]["S"].ToString();
                var itemTbc = productList[i]["TBC"]["N"].ToString();
                var item = itemParent.GetChild(i).GetComponent<GameItem>();
                item.ItemInit(productId);
                item.OnCurItemEvent += SetCurItem;
                item.OnOpenExplainPanelEvent += OpenExplainPanel;

                _itemInfoTable.Add(item.itemType, new ItemInfo(
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable,
                        LocaleManager.ItemKey + item.itemType),
                    int.Parse(itemTbc),
                    item.transform.GetChild(2).GetComponent<TMP_Text>()));

                _itemInfoTable[item.itemType].itemCountText.text = itemInventory[item.itemType.ToString()].ToString();
                CustomLog.Log($"아이템 타입 : {item.itemType}   가격 : {itemTbc}");
            }

            CustomLog.Log("아이템 초기화");
            purchaseButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                PurchaseItem();
            });
            _currencyController.diamondCurrency.SetText();
            _currencyController.emeraldCurrency.SetText();
        }

        private void SetCurItem(ItemType itemType, string productID)
        {
            _curItemType = itemType;
            _curProductID = productID;
            explainText.text = _itemInfoTable[itemType].itemExplain;
            CustomLog.Log($"아이템 설명 : {_itemInfoTable[itemType].itemExplain}");
        }

        private void OpenExplainPanel(Sprite sprite)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            explainImage.sprite = sprite;
            explainPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
        }

        private void PurchaseItem()
        {
            if (_curItemType == ItemType.None) return;
            if (_itemInfoTable[_curItemType].itemTbc < BackendGameData.curTbc)
            {
                purchaseButton.interactable = false;
                var useTbc = Backend.TBC.UseTBC(_curProductID, $"{_curItemType} 구매");
                if (useTbc.IsSuccess())
                {
                    _currencyController.diamondCurrency.SetText();

                    var itemCount = BackendGameData.userData.itemInventory[_curItemType.ToString()] += 1;
                    _itemInfoTable[_curItemType].itemCountText.text = itemCount.ToString();
                    purchaseButton.interactable = true;
                }
            }
            else
            {
                _notEnoughDiaSequence.Restart();
            }
        }

        private void ChangeLocaleItemDic(Locale locale)
        {
            foreach (var itemName in _itemInfoTable.Keys.ToList())
            {
                _itemInfoTable[itemName].itemExplain =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + itemName);
            }

            LocaleManager.ChangeLocaleAsync(LocaleManager.ItemTable, _curItemType.ToString(), explainText).Forget();
        }
    }
}