using AdsControl;
using BackendControl;
using CurrencyControl;
using DG.Tweening;
using UIControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class LobbyUI : MonoBehaviour
    {
        private FadeController _fadeController;
        private bool _isSequencePlaying;

        private Sequence _diamondNoticeSequence;
        private Sequence _emeraldNoticeSequence;

        [field: SerializeField] public DiamondCurrency diamondCurrency { get; private set; }
        [field: SerializeField] public EmeraldCurrency emeraldCurrency { get; private set; }

        [SerializeField] private Button startGameButton;

        [SerializeField] private GameObject buttonsObj;
        [SerializeField] private GameObject inGameMoneyObj;

        [SerializeField] private CanvasGroup noticeNeedMoreDia;
        [SerializeField] private CanvasGroup noticeNeedMoreEmerald;
        [SerializeField] private NoticePanel logOutPanel;

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _fadeController = FindAnyObjectByType<FadeController>();

            var noticeDiaRect = noticeNeedMoreDia.GetComponent<RectTransform>();

            _diamondNoticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(noticeDiaRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(noticeDiaRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeNeedMoreDia.DOFade(0, 0.25f).From(1));

            var noticeEmeraldRect = noticeNeedMoreEmerald.GetComponent<RectTransform>();

            _emeraldNoticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(noticeEmeraldRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(noticeEmeraldRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeNeedMoreEmerald.DOFade(0, 0.25f).From(1));
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
            startGameButton.onClick.AddListener(() => { _fadeController.FadeOutScene("MainGameScene").Forget(); });
            logOutPanel.OnConfirmButtonEvent += () =>
            {
                BackendChart.instance.InitItemTable();
                BackendLogin.instance.LogOut();
                _fadeController.FadeOutScene("LoginScene").Forget();
            };
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
    }
}