using System.Collections.Generic;
using BackEnd;
using CustomEnumControl;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace LobbyControl
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        private ItemShopController _itemShopController;
        private Sequence _loadingTween;
        private const string Diamonds500 = "diamonds500";
        private const string Diamonds2000 = "diamonds2000";
        private const string Diamonds5000 = "diamonds5000";
        private const string Diamonds12000 = "diamonds12000";
        private const string Diamonds50000 = "diamonds50000";
        private IStoreController _storeController;
        [SerializeField] private Image blockImage;
        [SerializeField] private Image loadingImage;
        [SerializeField] private Transform contents;

        private void Start()
        {
            _itemShopController = FindAnyObjectByType<ItemShopController>();
            blockImage.enabled = false;
            loadingImage.enabled = false;
            _loadingTween = DOTween.Sequence().SetAutoKill(false).Pause()
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
            builder.AddProduct(Diamonds50000, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        private void InitDiamondButton()
        {
            for (int i = 0; i < contents.childCount; i++)
            {
                var diamondItemParent = contents.GetChild(i);
                if (diamondItemParent.TryGetComponent(out DiamondItem diamondItem))
                {
                    diamondItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Purchase(diamondItem.productId);
                    });
                }
            }
        }

        private void LocalizeItemPrice(Locale locale)
        {
            for (int i = 0; i < contents.childCount; i++)
            {
                var diamondItemParent = contents.GetChild(i);
                if (diamondItemParent.TryGetComponent(out DiamondItem diamondItem))
                {
                    if (diamondItem.transform.GetChild(2).GetChild(0).TryGetComponent(out TMP_Text priceText))
                    {
                        Debug.Log("localize price");
                        var metaData = _storeController.products.WithID(diamondItem.productId).metadata;
                        priceText.text = $"{metaData.localizedPrice} {metaData.isoCurrencyCode}";
                    }
                }
            }
        }

        // 초기화 시 자동 호출
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            LocalizeItemPrice(LocalizationSettings.SelectedLocale);

            Debug.Log("initinitinitinitinitinitinitinitinitinitinitinitinitinitinit");
        }

        //구매 버튼에 연결
        public void Purchase(string productId)
        {
            _storeController.InitiatePurchase(productId);
            blockImage.enabled = true;
            loadingImage.enabled = true;
            _loadingTween.OnComplete(() =>
            {
                blockImage.enabled = false;
                loadingImage.enabled = false;
            }).Restart();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("초기화 실패 : " + error);
            _loadingTween.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("초기화 실패 : " + error + message);
            _loadingTween.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log("구매 실패 : " + failureReason);
            _loadingTween.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            Debug.Log("구매 성공 : " + product.definition.id);

#if UNITY_IPHONE
            ProcessApplePurchase(purchaseEvent);
#endif
            _loadingTween.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;

            return PurchaseProcessingResult.Complete;
        }
#if UNITY_IPHONE
        private void ProcessApplePurchase(PurchaseEventArgs args)
        {
            // var validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "구매한 유료다이아");
            // if (validation.IsSuccess())
            // {
            Debug.Log("purchase success");
            var chargeTbc = Backend.TBC.ChargeTBC(args.purchasedProduct.definition.id,
                args.purchasedProduct.receipt, "다이아 충전");
            if (chargeTbc.IsSuccess())
            {
                Debug.Log("충전 성공");
                _itemShopController.SetDiamondText();
            }
            else
            {
                Debug.Log("충전 실패");
                var code = chargeTbc.GetErrorCode();
                print(code);
            }
            // }
            // else
            // {
            //     Debug.Log($"ProcessPurchase: FAIL. Unrecognized product : {args.purchasedProduct.definition.id}");
            // }
        }
#endif
    }
}