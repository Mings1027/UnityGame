using AdsControl;
using BackEnd;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LobbyControl
{
    public class DiamondShopController : MonoBehaviour
    {
        private AdmobManager _rewardedAds;
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Button diamondButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        [field: SerializeField] public TMP_Text diamondText { get; private set; }

        private void Start()
        {
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            diamondButton.onClick.AddListener(OpenGoldPanel);
            closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;

            shopPanel.anchoredPosition = new Vector2(0, Screen.height);
            backgroundBlockImage.enabled = false;
            freePurchaseButton.onClick.AddListener(() =>
            {
                ShowRewardedAd().Forget();
            });
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
            shopPanel.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            backgroundBlockImage.enabled = false;
            shopBlockImage.enabled = true;
            shopPanel.DOAnchorPosY(Screen.height, 0.5f).SetEase(Ease.InBack);
        }
    }
}