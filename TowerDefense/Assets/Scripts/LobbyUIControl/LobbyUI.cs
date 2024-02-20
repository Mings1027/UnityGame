using AdsControl;
using CurrencyControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class LobbyUI : MonoBehaviour
    {
        private FadeController _fadeController;
        private bool _isSequencePlaying;

        private Sequence _diamondNoticeSequence;
        private Sequence _emeraldNoticeSequence;
        private Sequence _duplicateSequence;
        private Sequence _successChangeNameSequence;

        [field: SerializeField] public DiamondCurrency diamondCurrency { get; private set; }
        [field: SerializeField] public EmeraldCurrency emeraldCurrency { get; private set; }

        [SerializeField] private Button startGameButton;

        [SerializeField] private GameObject buttonsObj;
        [SerializeField] private GameObject inGameMoneyObj;

        [SerializeField] private CanvasGroup noticeNeedMoreDiaGroup;
        [SerializeField] private CanvasGroup noticeNeedMoreEmeraldGroup;
        [SerializeField] private CanvasGroup noticeDuplicateNameGroup;
        [SerializeField] private CanvasGroup noticeSuccessChangeNameGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image backgroundBlockImage;

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _fadeController = FindAnyObjectByType<FadeController>();

            var noticeDiaRect = noticeNeedMoreDiaGroup.GetComponent<RectTransform>();

            _diamondNoticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(noticeDiaRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(noticeDiaRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeNeedMoreDiaGroup.DOFade(0, 0.25f).From(1));

            var noticeEmeraldRect = noticeNeedMoreEmeraldGroup.GetComponent<RectTransform>();

            _emeraldNoticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(noticeEmeraldRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(noticeEmeraldRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeNeedMoreEmeraldGroup.DOFade(0, 0.25f).From(1));

            var duplicateRect = noticeDuplicateNameGroup.GetComponent<RectTransform>();
            _duplicateSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(duplicateRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(duplicateRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeDuplicateNameGroup.DOFade(0, 0.25f).From(1));

            var successNameRect = noticeSuccessChangeNameGroup.GetComponent<RectTransform>();
            _successChangeNameSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(successNameRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(successNameRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeSuccessChangeNameGroup.DOFade(0, 0.25f).From(1));
        }

        private void Start()
        {
            Init();
        }

        private void OnDisable()
        {
            _diamondNoticeSequence?.Kill();
            _emeraldNoticeSequence?.Kill();
        }

        private void Init()
        {
            FindAnyObjectByType<AdmobManager>().BindLobbyUI(this);
            inGameMoneyObj.SetActive(false);
            startGameButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _fadeController.FadeOutScene("MainGameScene").Forget();
            });

            OffBackgroundImage();
            OffBlockImage();
        }

        public void SetActiveButtons(bool active, bool inGameMoneyActive)
        {
            buttonsObj.SetActive(active);
            inGameMoneyObj.SetActive(inGameMoneyActive);
        }

        public void On()
        {
            diamondCurrency.On();
            emeraldCurrency.On();
        }

        public void Off()
        {
            diamondCurrency.Off();
            emeraldCurrency.Off();
        }

        public void OnBackgroundImage()
        {
            backgroundImage.enabled = true;
            backgroundBlockImage.raycastTarget = true;
        }

        public void OffBackgroundImage() => backgroundImage.enabled = false;

        public void OffBlockImage() => backgroundBlockImage.raycastTarget = false;

        public void NoticeDiaTween()
        {
            if (_isSequencePlaying) return;
            _isSequencePlaying = true;
            _diamondNoticeSequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }

        public void NoticeEmeraldTween()
        {
            if (_isSequencePlaying) return;
            _isSequencePlaying = true;
            _emeraldNoticeSequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }

        public void NoticeDuplicateTween()
        {
            if (_isSequencePlaying) return;
            _isSequencePlaying = true;
            _duplicateSequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }

        public void NoticeSuccessChangeNameTween()
        {
            if (_isSequencePlaying) return;
            _isSequencePlaying = true;
            _successChangeNameSequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }
    }
}