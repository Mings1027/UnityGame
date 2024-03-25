using BackendControl;
using CustomEnumControl;
using DataControl;
using DG.Tweening;
using ItemControl;
using ManagerControl;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class ItemShopController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Sequence _purchasePanelSequence;
        private ItemType _curItemType;

        private int _curQuantity;
        private int _curEmeraldPrice;
        private CanvasGroup _shopPanelGroup;
        private string _localizedOwnedText;

        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Transform itemParent;

        [SerializeField] private Image purchaseBackgroundBlockImage;
        [SerializeField] private CanvasGroup purchasePanelGroup;

        [SerializeField] private Button closePurchasePanelButton;

        [SerializeField] private RectTransform purchasePanel;
        [SerializeField] private Image explainImage;
        [SerializeField] private TMP_Text ownedAmountText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text emeraldPriceText;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private TMP_Text quantityText;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _shopPanelGroup = shopPanel.GetComponent<CanvasGroup>();
            _shopPanelGroup.blocksRaycasts = false;
            _purchasePanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(purchasePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(purchasePanel.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            _purchasePanelSequence.OnComplete(() => { purchasePanelGroup.blocksRaycasts = true; });

            purchasePanelGroup.blocksRaycasts = false;
            purchaseBackgroundBlockImage.enabled = false;

            increaseButton.onClick.AddListener(IncreaseQuantity);
            decreaseButton.onClick.AddListener(DecreaseQuantity);

            closePurchasePanelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                ClosePurchasePanel();
            });
            _localizedOwnedText = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "OwnedText");
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleOwnedText;
        }

        private void Start()
        {
            ItemInit();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeLocaleOwnedText;
        }

        private void ItemInit()
        {
            BackendChart.instance.ChartGet();

            for (var i = 0; i < itemParent.childCount; i++)
            {
                var item = itemParent.GetChild(i).GetComponent<GameItem>();
                var itemPrice = BackendChart.ItemTable[item.itemType.ToString()];
                item.OnOpenExplainPanelEvent += OpenPurchasePanel;
                item.SetText(itemPrice.ToString());
            }

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
            itemNameText.text = TowerDataManager.ItemInfoTable[itemType].itemName;
            descriptionText.text = TowerDataManager.ItemInfoTable[itemType].itemDescription;

            explainImage.sprite = sprite;
            ownedAmountText.text = _localizedOwnedText + TowerDataManager.ItemInfoTable[itemType].itemCount;
            purchaseBackgroundBlockImage.enabled = true;
            _purchasePanelSequence.Restart();
            _curQuantity = 1;
            _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()];
            emeraldPriceText.text = _curEmeraldPrice.ToString();
            quantityText.text = _curQuantity.ToString();
        }

        private void ClosePurchasePanel()
        {
            purchaseBackgroundBlockImage.enabled = false;
            purchasePanelGroup.blocksRaycasts = false;
            _purchasePanelSequence.PlayBackwards();
        }

        private void IncreaseQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity++;
            if (BackendGameData.userData.emerald >= BackendChart.ItemTable[_curItemType.ToString()] * _curQuantity)
            {
                _curEmeraldPrice = BackendChart.ItemTable[_curItemType.ToString()] * _curQuantity;
                emeraldPriceText.text = _curEmeraldPrice.ToString();
                quantityText.text = _curQuantity.ToString();
            }
            else
            {
                _curQuantity--;
            }
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

        private void PurchaseItem()
        {
            if (_curQuantity <= 0) return;
            if (_curEmeraldPrice <= BackendGameData.userData.emerald)
            {
                BackendGameData.userData.emerald -= _curEmeraldPrice;
                _lobbyUI.emeraldCurrency.SetText();

                var itemCount = BackendGameData.userData.itemInventory[_curItemType.ToString()] += _curQuantity;
                TowerDataManager.ItemInfoTable[_curItemType].itemCount = itemCount;

                ClosePurchasePanel();
            }
            else
            {
                FloatingNotification.FloatingNotify(FloatingNotifyEnum.NeedMoreEmerald);
            }
        }

        private void ChangeLocaleOwnedText(Locale locale)
        {
            _localizedOwnedText = LocaleManager.GetLocalizedString(LocaleManager.LobbyUITable, "OwnedText");
        }
    }
}