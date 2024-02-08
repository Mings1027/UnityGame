using AdsControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LobbyUIControl
{
    public class DiamondShopController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private AdmobManager _rewardedAds;
        private Tween _shopTween;

        [SerializeField] private CanvasGroup shopPanelGroup;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Button diamondButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Start()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            shopPanelGroup.blocksRaycasts = false;
            _shopTween = shopPanelGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear).SetAutoKill(false).Pause();
            diamondButton.onClick.AddListener(OpenGoldPanel);
            closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;

            backgroundBlockImage.enabled = false;
            freePurchaseButton.onClick.AddListener(() => { ShowRewardedAd().Forget(); });
            SoundManager.PlayBGM(SoundEnum.GameStart);
        }

        private void OnDisable()
        {
            _shopTween?.Kill();
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
            _shopTween.OnComplete(() => shopPanelGroup.blocksRaycasts = true).Restart();
            _lobbyUI.SetActiveButtons(false, true);
            _lobbyUI.Off();
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            backgroundBlockImage.enabled = false;
            shopBlockImage.enabled = true;
            shopPanelGroup.blocksRaycasts = false;
            _shopTween.PlayBackwards();
            _lobbyUI.SetActiveButtons(true, false);
            _lobbyUI.On();
        }
    }
}