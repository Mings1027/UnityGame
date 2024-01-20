using BackEnd;
using DG.Tweening;
using UIControl;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private Sequence loadingTween;
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
        loadingTween = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(loadingImage.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true))
            .Join(loadingImage.transform.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360)
                .SetLoops(5));

        InitIAP();
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
        loadingTween.OnComplete(() =>
        {
            blockImage.enabled = false;
            loadingImage.enabled = false;
        }).Restart();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("초기화 실패 : " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("초기화 실패 : " + error + message);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("구매 실패 : " + failureReason);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
        Debug.Log("구매 설공 : " + product.definition.id);
        switch (product.definition.id)
        {
            case Diamonds500:
                _diamondShopController.PurchaseDiamond(500);
                break;
            case Diamonds2000:
                _diamondShopController.PurchaseDiamond(2000);
                break;
            case Diamonds5000:
                _diamondShopController.PurchaseDiamond(5000);
                break;
            case Diamonds12000:
                _diamondShopController.PurchaseDiamond(12000);
                break;
            case Diamonds50000:
                _diamondShopController.PurchaseDiamond(50000);
                break;
        }
#if UNITY_IPHONE
        Backend.TBC.ChargeTBC(product.definition.id, purchaseEvent.purchasedProduct.receipt, (callback) =>
        {
            loadingTween.Pause();
            loadingImage.enabled = false;
            blockImage.enabled = false;
        });
#endif
        return PurchaseProcessingResult.Complete;
    }
}