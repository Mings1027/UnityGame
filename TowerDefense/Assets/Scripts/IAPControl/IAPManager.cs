using System.Collections.Generic;
using BackEnd;
using DG.Tweening;
using ItemControl;
using LobbyUIControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
using Utilities;

namespace IAPControl
{
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        private Dictionary<string, TMP_Text> _priceDic;
        private LobbyUI _lobbyUI;
        private Sequence _loadingSequence;
        private const string Diamonds500 = "diamonds500";
        private const string Diamonds2000 = "diamonds2000";
        private const string Diamonds5000 = "diamonds5000";
        private const string Diamonds12000 = "diamonds12000";
        private const string Diamonds30000 = "diamonds30000";
        private IStoreController _storeController;
        [SerializeField] private Image blockImage;
        [SerializeField] private Image loadingImage;
        [SerializeField] private Transform contents;

        private void Start()
        {
            _priceDic = new Dictionary<string, TMP_Text>();
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            blockImage.enabled = false;
            loadingImage.enabled = false;
            _loadingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(loadingImage.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true))
                .Join(loadingImage.transform.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360)
                    .SetLoops(30));

            InitIAP();
            InitDiamondButton();
            LocalizationSettings.SelectedLocaleChanged += LocalizeItemPrice;
        }

        private void InitIAP()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(Diamonds500, ProductType.Consumable);
            builder.AddProduct(Diamonds2000, ProductType.Consumable);
            builder.AddProduct(Diamonds5000, ProductType.Consumable);
            builder.AddProduct(Diamonds12000, ProductType.Consumable);
            builder.AddProduct(Diamonds30000, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        private void InitDiamondButton()
        {
            for (int i = 0; i < contents.childCount; i++)
            {
                var diamondItemParent = contents.GetChild(i);
                if (diamondItemParent.TryGetComponent(out DiamondItem diamondItem))
                {
                    diamondItem.purchaseButton.onClick.AddListener(() => Purchase(diamondItem.productId));
                }
            }
        }

        private void LocalizeItemPrice(Locale locale)
        {
            foreach (var productId in _priceDic.Keys)
            {
                var metaData = _storeController.products.WithID(productId).metadata;
                CustomLog.Log(
                    $"storeController : {_storeController}   productId : {productId}  metaData.localizedPrice : {metaData.localizedPrice}  metaData.isoCurrencyCode : {metaData.isoCurrencyCode}");
                _priceDic[productId].text = $"{metaData.localizedPrice} {metaData.isoCurrencyCode}";
            }
        }

        // Automatically called on initialization
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            for (int i = 0; i < contents.childCount; i++)
            {
                if (contents.GetChild(i).TryGetComponent(out DiamondItem diamondItem))
                {
                    _priceDic.Add(diamondItem.productId, diamondItem.priceText);
                }
            }

            LocalizeItemPrice(LocalizationSettings.SelectedLocale);

            CustomLog.Log("initinitinitinitinitinitinitinitinitinitinitinitinitinitinit");
        }

        private void Purchase(string productId)
        {
            _storeController.InitiatePurchase(productId);
            blockImage.enabled = true;
            loadingImage.enabled = true;
            _loadingSequence.OnComplete(() =>
            {
                blockImage.enabled = false;
                loadingImage.enabled = false;
            }).Restart();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            CustomLog.Log("초기화 실패 : " + error);
            _loadingSequence.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            CustomLog.Log("초기화 실패 : " + error + message);
            _loadingSequence.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            CustomLog.Log("구매 실패 : " + failureReason);
            _loadingSequence.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            CustomLog.Log("구매 실패 : " + product.definition.id + "이유 : " + failureDescription.message);
            _loadingSequence.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            CustomLog.Log("구매 성공 : " + product.definition.id);

#if UNITY_IPHONE
            ProcessApplePurchase(purchaseEvent);
#endif
            _loadingSequence.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;

            return PurchaseProcessingResult.Complete;
        }
#if UNITY_IPHONE
        private void ProcessApplePurchase(PurchaseEventArgs args)
        {
            CustomLog.Log("purchase success");
            var chargeTbc = Backend.TBC.ChargeTBC(args.purchasedProduct.definition.id,
                args.purchasedProduct.receipt, "다이아 충전");
            if (chargeTbc.IsSuccess())
            {
                CustomLog.Log("충전 성공");
                _lobbyUI.diamondCurrency.TextInit();
            }
            else
            {
                CustomLog.Log("충전 실패");
                CustomLog.Log(chargeTbc.GetErrorCode());
            }
        }
#endif
    }
}