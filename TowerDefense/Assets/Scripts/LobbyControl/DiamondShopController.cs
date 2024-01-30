using AdsControl;
using BackEnd;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LobbyControl
{
    public class DiamondShopController : MonoBehaviour
    {
        private AdmobManager _rewardedAds;
        private CurrencyController _currencyController;
        [SerializeField] private GameObject buttons;
        [SerializeField] private RectTransform inGameMoney;
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Button diamondButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Start()
        {
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            _currencyController = FindAnyObjectByType<CurrencyController>();
            diamondButton.onClick.AddListener(OpenGoldPanel);
            closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;

            // shopPanel.anchoredPosition = new Vector2(0, Screen.height);
            shopPanel.localScale = new Vector2(0, 1);
            backgroundBlockImage.enabled = false;
            freePurchaseButton.onClick.AddListener(() => { ShowRewardedAd().Forget(); });
            SoundManager.PlayBGM(SoundEnum.GameStart);
        }

        private async UniTaskVoid ShowRewardedAd()
        {
            SoundManager.MuteBGM(true);
            loadingImage.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
            loadingImage.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetLoops(2);
            await UniTask.Delay(Random.Range(5, 8) * 100);
            loadingImage.localScale = Vector3.zero;
            _rewardedAds.ShowRewardedAd();
        }

        private void OpenGoldPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            backgroundBlockImage.enabled = true;
            shopBlockImage.enabled = false;
            shopPanel.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack);
            buttons.SetActive(false);
            inGameMoney.SetParent(transform);
            _currencyController.Off();
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            backgroundBlockImage.enabled = false;
            shopBlockImage.enabled = true;
            shopPanel.DOScaleX(0, 0.25f).From(1).SetEase(Ease.InBack);
            buttons.SetActive(true);
            inGameMoney.SetParent(buttons.transform);
            _currencyController.On();
        }
    }
}