using System;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class NoticePanel : MonoBehaviour
    {
#region Private

        private Sequence _noticeSequence;
        private Sequence _noticeChildSequence;

        private CanvasGroup _noticePanelGroup;
        private Image _blockImage;
        private RectTransform _noticeText;
        private RectTransform _noticePanel;
        private TMP_Text _noticePanelText;
        private CanvasGroup _buttonGroup;
        private Button _confirmButton;
        private Button _cancelButton;

#endregion

        [SerializeField] private Button popUpButton;

#region Action

        public event Action OnPopUpButtonEvent;
        public event Action OnOkButtonEvent;
        public event Action OnCancelButtonEvent;

#endregion

#region Unity Event

        private void Awake()
        {
            InitComponent();
            InitTween();
            InitButton();
        }

        private void OnDestroy()
        {
            _noticeSequence?.Kill();
            _noticeChildSequence?.Kill();
        }

#endregion

#region Init

        private void InitComponent()
        {
            _noticePanelGroup = GetComponent<CanvasGroup>();
            _blockImage = transform.GetChild(0).GetComponent<Image>();
            _noticeText = transform.GetChild(1).GetComponent<RectTransform>();
            _noticePanel = transform.GetChild(2).GetComponent<RectTransform>();
            _noticePanelText = _noticePanel.GetComponentInChildren<TMP_Text>();
            _buttonGroup = transform.GetChild(3).GetComponent<CanvasGroup>();
            _confirmButton = _buttonGroup.transform.Find("Confirm Button").GetComponent<Button>();
            _cancelButton = _buttonGroup.transform.Find("Cancel Button").GetComponent<Button>();
        }

        private void InitTween()
        {
            _noticePanelGroup.blocksRaycasts = false;
            _buttonGroup.blocksRaycasts = false;

            _noticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_noticePanelGroup.DOFade(0.97f, 0.25f).From(0).SetEase(Ease.Linear));

            _noticeChildSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_blockImage.DOFade(1, 0.2f).From(0).SetEase(Ease.Linear))
                .Join(_noticeText.DOScale(1, 0.25f).From(0).SetEase(Ease.Linear))
                .Join(_noticePanel.DOScaleY(0, 0.25f).From().SetEase(Ease.Linear))
                .Join(_buttonGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear))
                .Join(_buttonGroup.transform.DOLocalMoveY(-340, 0.25f).From().SetEase(Ease.Linear))
                .Append(_noticePanelText.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear));
        }

        private void InitButton()
        {
            OnOkButtonEvent += () => SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (popUpButton != null)
            {
                popUpButton.onClick.AddListener(OpenPopUp);
            }

            _confirmButton.onClick.AddListener(() =>
            {
                _noticePanelGroup.blocksRaycasts = false;
                _buttonGroup.blocksRaycasts = false;
                _noticeSequence.PlayBackwards();
                OnOkButtonEvent?.Invoke();
            });
            _cancelButton.onClick.AddListener(() =>
            {
                _noticePanelGroup.blocksRaycasts = false;
                _buttonGroup.blocksRaycasts = false;
                _noticeSequence.PlayBackwards();
                OnCancelButtonEvent?.Invoke();
            });
        }

#endregion

#region Public Method

        public void OpenPopUp()
        {
            _noticePanelGroup.blocksRaycasts = true;
            _noticeSequence.Restart();
            _noticeChildSequence.OnComplete(() => _buttonGroup.blocksRaycasts = true).Restart();
            OnPopUpButtonEvent?.Invoke();
        }

#endregion
    }
}