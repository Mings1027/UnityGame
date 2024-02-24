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

        // [SerializeField] private CanvasGroup shopPanelGroup;
        // [SerializeField] private Image shopBlockImage;
        // [SerializeField] private Button diamondButton;
        // [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Start()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            // shopPanelGroup.blocksRaycasts = false;
            // _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            //     .Append(shopPanelGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear))
            //     .Join(shopPanelGroup.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f).From(new Vector2(100, 0)));
            // _panelSequence.OnComplete(() => shopPanelGroup.blocksRaycasts = true);
            // _panelSequence.OnRewind(() => { _lobbyUI.OffBlockImage(); });

            // diamondButton.onClick.AddListener(OpenGoldPanel);
            // closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;

            freePurchaseButton.onClick.AddListener(() =>
            {
                freePurchaseButton.interactable = false;
                ShowRewardedAd().Forget();
            });
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
            freePurchaseButton.interactable = true;
            loadingImage.localScale = Vector3.zero;
            _rewardedAds.ShowRewardedAd();
        }

        // private void OpenGoldPanel()
        // {
        //     SoundManager.PlayUISound(SoundEnum.ButtonSound);
        //     shopBlockImage.enabled = false;
        //     _lobbyUI.OnBackgroundImage();
        //     _lobbyUI.SetActiveButtons(false, true);
        //     _lobbyUI.Off();
        //     _panelSequence.Restart();
        // }
        //
        // private void ClosePanel()
        // {
        //     SoundManager.PlayUISound(SoundEnum.ButtonSound);
        //     shopBlockImage.enabled = true;
        //     _lobbyUI.OffBackgroundImage();
        //     _lobbyUI.SetActiveButtons(true, false);
        //     _lobbyUI.On();
        //     shopPanelGroup.blocksRaycasts = false;
        //     _panelSequence.PlayBackwards();
        // }
    }
}