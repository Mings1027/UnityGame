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
        private Sequence _shopPanelSequence;
        private Sequence _purchasePanelSequence;
        private ItemType _curItemType;

        private Dictionary<ItemType, ItemInfo> _itemInfoTable;
        private int _curQuantity;
        private int _curEmeraldPrice;
        private CanvasGroup _shopPanelGroup;

        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private Button itemShopButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Transform itemParent;

        [SerializeField] private CanvasGroup purchasePanelGroup;

        [SerializeField] private Button closePurchasePanelButton;
        [SerializeField] private Button closePurchasePanelBackgroundButton;

        [SerializeField] private RectTransform purchasePanel;
        [SerializeField] private Image explainImage;
        [SerializeField] private TMP_Text ownedAmountText;
        [SerializeField] private TMP_Text explainText;
        [SerializeField] private TMP_Text emeraldPriceText;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private TMP_Text quantityText;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _itemInfoTable = new Dictionary<ItemType, ItemInfo>();
            _shopPanelGroup = GetComponent<CanvasGroup>();
            _shopPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_shopPanelGroup.DOFade(1, 0.25f).From(0))
                .Join(shopPanel.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            _shopPanelGroup.blocksRaycasts = false;

            _purchasePanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(purchasePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(purchasePanel.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            purchasePanelGroup.blocksRaycasts = false;

            itemShopButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _shopPanelSequence.OnComplete(() => _shopPanelGroup.blocksRaycasts = true).Restart();
                _lobbyUI.SetActiveButtons(false, true);
                _lobbyUI.Off();
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _shopPanelSequence.OnRewind(() => _shopPanelGroup.blocksRaycasts = false).PlayBackwards();
                _lobbyUI.SetActiveButtons(true, false);
                _lobbyUI.On();
            });

            increaseButton.onClick.AddListener(IncreaseQuantity);
            decreaseButton.onClick.AddListener(DecreaseQuantity);

            closePurchasePanelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                CloseExplainPanel();
            });
            closePurchasePanelBackgroundButton.onClick.AddListener(() =>
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
            _shopPanelSequence?.Kill();
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
                item.OnOpenExplainPanelEvent += OpenPurchasePanel;
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

        private void OpenPurchasePanel(ItemType itemType, Sprite sprite)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curItemType = itemType;
            explainText.text = _itemInfoTable[itemType].itemExplain;
            CustomLog.Log($"아이템 설명 : {_itemInfoTable[itemType].itemExplain}");

            explainImage.sprite = sprite;
            ownedAmountText.text = _itemInfoTable[itemType].itemCount.ToString();
            _purchasePanelSequence.OnComplete(() => purchasePanelGroup.blocksRaycasts = true).Restart();
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()];
            _curQuantity = 1;
            emeraldPriceText.text = _curEmeraldPrice.ToString();
            quantityText.text = _curQuantity.ToString();
        }

        private void CloseExplainPanel()
        {
            _purchasePanelSequence.OnRewind(() => purchasePanelGroup.blocksRaycasts = false).PlayBackwards();
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
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()] * _curQuantity;
            if (BackendGameData.userData.emerald <= _curEmeraldPrice) return;
            _curQuantity++;
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