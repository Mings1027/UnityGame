using BackEnd;
using DG.Tweening;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private Sequence _loadingTween;
    private const string Diamonds500 = "diamonds500";
    private const string Diamonds2000 = "diamonds2000";
    private const string Diamonds5000 = "diamonds5000";
    private const string Diamonds12000 = "diamonds12000";
    private const string Diamonds50000 = "diamonds50000";
    private IStoreController _storeController;
    private DiamondShopController _diamondShopController;
    [SerializeField] private Image blockImage;
    [SerializeField] private Image loadingImage;

    private void Start()
    {
        _diamondShopController = FindAnyObjectByType<DiamondShopController>();
        blockImage.enabled = false;
        loadingImage.enabled = false;
        _loadingTween = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(loadingImage.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true))
            .Join(loadingImage.transform.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360)
                .SetLoops(10));

        InitIAP();
        print(Backend.TBC.GetProductList());
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

    // 초기화 시 자동 호출
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;

        print("initinitinitinitinitinitinitinitinitinitinitinitinitinitinit");
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
        Debug.Log("구매 설공 : " + product.definition.id);
        var diamond = product.definition.id switch
        {
            Diamonds500 => 500,
            Diamonds2000 => 2000,
            Diamonds5000 => 5000,
            Diamonds12000 => 12000,
            Diamonds50000 => 50000,
            _ => 0
        };

#if UNITY_IPHONE
        ProcessApplePurchase(diamond, purchaseEvent);
        _loadingTween.Pause();
        loadingImage.enabled = false;
        blockImage.enabled = false;
#endif
        _diamondShopController.PurchaseDiamond();
        return PurchaseProcessingResult.Complete;
    }
#if UNITY_IPHONE
    private void ProcessApplePurchase(int diamond, PurchaseEventArgs args)
    {
        var validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "구매한 유료다이아");
        if (validation.IsSuccess())
        {
            Debug.Log("purchase success");
            DataManager.diamond += diamond;

            Backend.TBC.ChargeTBC(args.purchasedProduct.definition.id, args.purchasedProduct.receipt);
        }
        else
        {
            Debug.Log($"ProcessPurchase: FAIL. Unrecognized product : {args.purchasedProduct.definition.id}");
        }
    }
#endif
}