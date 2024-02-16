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
        private Sequence _panelSequence;

        [SerializeField] private CanvasGroup shopPanelGroup;
        [SerializeField] private Image backgroundBlockImage;
        [SerializeField] private Image shopBlockImage;
        [SerializeField] private Button diamondButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Start()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            shopPanelGroup.blocksRaycasts = false;
            _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(shopPanelGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear))
                .Join(shopPanelGroup.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f).From(new Vector2(100, 0)));
            diamondButton.onClick.AddListener(OpenGoldPanel);
            closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;

            backgroundBlockImage.enabled = false;
            freePurchaseButton.onClick.AddListener(() => { ShowRewardedAd().Forget(); });
            SoundManager.PlayBGM(SoundEnum.GameStart);
        }

        private void OnDisable()
        {
            _panelSequence?.Kill();
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
            _panelSequence.OnComplete(() => shopPanelGroup.blocksRaycasts = true).Restart();
            _lobbyUI.SetActiveButtons(false, true);
            _lobbyUI.Off();
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            backgroundBlockImage.enabled = false;
            shopBlockImage.enabled = true;
            _panelSequence.OnRewind(() => shopPanelGroup.blocksRaycasts = false).PlayBackwards();
            _lobbyUI.SetActiveButtons(true, false);
            _lobbyUI.On();
        }
    }
}