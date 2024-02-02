using System.Collections.Generic;
using System.Linq;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ItemControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utilities;

namespace LobbyUIControl
{
    public class ItemShopController : MonoBehaviour
    {
        private class ItemInfo
        {
            public string itemExplain;
            public int itemCount;

            public ItemInfo(string itemExplain, int itemCount)
            {
                this.itemExplain = itemExplain;
                this.itemCount = itemCount;
            }
        }

        private LobbyUI _lobbyUI;
        private ItemInfo _itemInfo;
        private Tween _shopTween;
        private ItemType _curItemType;

        private Dictionary<ItemType, ItemInfo> _itemInfoTable;
        private int _curQuantity;
        private int _curEmeraldPrice;

        [SerializeField] private Button itemShopButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Transform shopPanel;
        [SerializeField] private Transform itemParent;
        [SerializeField] private Image explainBlockImage;
        [SerializeField] private Button explainBlockImageButton;
        [SerializeField] private Button closeExplainPanelButton;
        [SerializeField] private Transform explainPanel;
        [SerializeField] private Image explainImage;
        [SerializeField] private TMP_Text ownedAmountText;
        [SerializeField] private TMP_Text explainText;
        [SerializeField] private TMP_Text emeraldPriceText;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private TMP_Text quantityText;

        private void Awake()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _itemInfoTable = new Dictionary<ItemType, ItemInfo>();
            backgroundBlockImage.enabled = false;
            _shopTween = shopPanel.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            explainBlockImage.enabled = false;
            explainPanel.localScale = Vector3.zero;
            itemShopButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                backgroundBlockImage.enabled = true;
                shopBlockImage.enabled = false;
                _shopTween.Restart();
                _lobbyUI.SetActiveButtons(false, true);
                _lobbyUI.Off();
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                backgroundBlockImage.enabled = false;
                shopBlockImage.enabled = true;
                _shopTween.PlayBackwards();
                _lobbyUI.SetActiveButtons(true, false);
                _lobbyUI.On();
            });

            increaseButton.onClick.AddListener(IncreaseQuantity);
            decreaseButton.onClick.AddListener(DecreaseQuantity);

            explainBlockImageButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                CloseExplainPanel();
            });
            closeExplainPanelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                CloseExplainPanel();
            });

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemDic;
        }

        private void Start()
        {
            ItemInit();
        }

        private void OnDisable()
        {
            _shopTween?.Kill();
        }

        private void ItemInit()
        {
            CustomLog.Log($"플레이어 데이터: {BackendGameData.userData}");
            CustomLog.Log($"플레이어 아이템 테이블: {BackendGameData.userData.itemInventory}");
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemParent.childCount; i++)
            {
                var item = itemParent.GetChild(i).GetComponent<GameItem>();
                var itemPrice = BackendChart.ItemTable[item.itemType.ToString()];
                item.OnOpenExplainPanelEvent += OpenExplainPanel;
                item.SetText(itemPrice.ToString());

                _itemInfoTable.Add(item.itemType, new ItemInfo(
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, LocaleManager.ItemKey + item.itemType),
                    itemInventory[item.itemType.ToString()]));

                CustomLog.Log($"아이템 타입 : {item.itemType}   가격 : {itemPrice}");
            }

            CustomLog.Log("아이템 초기화");
            purchaseButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                PurchaseItem();
            });
        }

        private void OpenExplainPanel(ItemType itemType, Sprite sprite)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);

            _curItemType = itemType;
            explainText.text = _itemInfoTable[itemType].itemExplain;
            CustomLog.Log($"아이템 설명 : {_itemInfoTable[itemType].itemExplain}");

            explainImage.sprite = sprite;
            ownedAmountText.text = _itemInfoTable[itemType].itemCount.ToString();
            explainBlockImage.enabled = true;
            explainPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()];
            _curQuantity = 1;
            emeraldPriceText.text = _curEmeraldPrice.ToString();
            quantityText.text = _curQuantity.ToString();
        }

        private void CloseExplainPanel()
        {
            explainBlockImage.enabled = false;
            explainPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
        }

        private void PurchaseItem()
        {
            if (_curItemType == ItemType.None || _curQuantity <= 0) return;
            if (_curEmeraldPrice < BackendGameData.userData.emerald)
            {
                BackendGameData.userData.emerald -= _curEmeraldPrice;
                _lobbyUI.emeraldCurrency.SetText();

                var itemCount = BackendGameData.userData.itemInventory[_curItemType.ToString()] += _curQuantity;
                _itemInfoTable[_curItemType].itemCount = itemCount;

                CloseExplainPanel();
            }
            else
            {
                _lobbyUI.emeraldNotifySequence.Restart();
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

        private void IncreaseQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity++;
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()] * _curQuantity;
            emeraldPriceText.text = _curEmeraldPrice.ToString();
            quantityText.text = _curQuantity.ToString();
        }

        private void DecreaseQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_curQuantity <= 0) return;
            _curQuantity--;
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()] * _curQuantity;
            emeraldPriceText.text = _curEmeraldPrice.ToString();
            quantityText.text = _curQuantity.ToString();
        }
    }
}