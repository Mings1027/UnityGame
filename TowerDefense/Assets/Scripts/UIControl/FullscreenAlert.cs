using System;
using System.Collections.Generic;
using System.Linq;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UIControl
{
    public class FullscreenAlert : MonoSingleton<FullscreenAlert>
    {
#region Private

        private Dictionary<FullscreenAlertEnum, string> _fullscreenAlertDic;
        private Tween _noticeTween;
        private Sequence _noticeChildSequence;
        private CanvasGroup _noticePanelGroup;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform noticeText;
        [SerializeField] private RectTransform noticePanel;
        [SerializeField] private TMP_Text noticeInfoText;
        [SerializeField] private CanvasGroup buttonGroup;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

#endregion

#region Action

        public event Action OnConfirmEvent;
        public event Action OnCancelEvent;

#endregion

#region Unity Event

        protected override void Awake()
        {
            base.Awake();

            Init().Forget();
            InitComponent();
            InitTween();
            InitButton();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += ChangeAlertLocale;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeAlertLocale;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _noticeTween?.Kill();
            _noticeChildSequence?.Kill();
        }

#endregion

#region Init

        private async UniTaskVoid Init()
        {
            _fullscreenAlertDic = new Dictionary<FullscreenAlertEnum, string>();

            var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(LocaleManager.FullscreenAlertTable);
            await tableOperation;
            if (tableOperation.Status == AsyncOperationStatus.Succeeded)
            {
                var dic = tableOperation.Result.ToDictionary(v => v.Value);
                var fullscreenAlerts = Enum.GetValues(typeof(FullscreenAlertEnum));

                foreach (FullscreenAlertEnum alert in fullscreenAlerts)
                {
                    foreach (var kvp in dic)
                    {
                        if (alert.ToString() != kvp.Key.Key) continue;
                        _fullscreenAlertDic.Add(alert, kvp.Key.Value);
                        break;
                    }
                }
            }
        }

        private void ChangeAlertLocale(Locale locale)
        {
            foreach (var alertString in _fullscreenAlertDic.Keys.ToList())
            {
                _fullscreenAlertDic[alertString] =
                    LocaleManager.GetLocalizedString(LocaleManager.FullscreenAlertTable, alertString.ToString());
            }
        }

        private void InitComponent()
        {
            _noticePanelGroup = GetComponent<CanvasGroup>();
        }

        private void InitTween()
        {
            _noticePanelGroup.blocksRaycasts = false;
            buttonGroup.blocksRaycasts = false;

            _noticeTween = _noticePanelGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear)
                .SetAutoKill(false).Pause().SetUpdate(true);

            _noticeChildSequence = DOTween.Sequence().SetAutoKill(false).Pause().SetUpdate(true)
                .Append(backgroundImage.DOFade(1, 0.2f).From(0).SetEase(Ease.Linear))
                .Join(noticeText.DOScale(1, 0.25f).From(0).SetEase(Ease.Linear))
                .Join(noticePanel.DOScaleY(0, 0.25f).From().SetEase(Ease.Linear))
                .Join(buttonGroup.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear))
                .Join(buttonGroup.transform.DOLocalMoveY(-340, 0.25f).From().SetEase(Ease.Linear))
                .Append(noticeInfoText.DOFade(1, 0.25f).From(0).SetEase(Ease.Linear));
        }

        private void InitButton()
        {
            confirmButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _noticePanelGroup.blocksRaycasts = false;
                buttonGroup.blocksRaycasts = false;
                _noticeTween.PlayBackwards();
                OnConfirmEvent?.Invoke();
            });
            cancelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _noticePanelGroup.blocksRaycasts = false;
                buttonGroup.blocksRaycasts = false;
                _noticeTween.PlayBackwards();
                OnCancelEvent?.Invoke();
            });
        }

        private void NonCancelableAlertPrivate(FullscreenAlertEnum fullscreenAlertEnum, Action confirmAction,
            Action cancelAction = null)
        {
            noticeInfoText.text = _fullscreenAlertDic[fullscreenAlertEnum];

            _noticePanelGroup.blocksRaycasts = true;
            _noticeTween.Restart();
            _noticeChildSequence.OnComplete(() => buttonGroup.blocksRaycasts = true).Restart();

            cancelButton.gameObject.SetActive(false);

            OnConfirmEvent = null;
            OnConfirmEvent += confirmAction;
            if (cancelAction == null) return;
            OnCancelEvent = null;
            OnCancelEvent += cancelAction;
        }

        private void CancelableAlertPrivate(FullscreenAlertEnum fullscreenAlertEnum, Action confirmAction,
            Action cancelAction = null, string additionalText = null)
        {
            noticeInfoText.text = _fullscreenAlertDic[fullscreenAlertEnum] + additionalText;

            _noticePanelGroup.blocksRaycasts = true;
            _noticeTween.Restart();
            _noticeChildSequence.OnComplete(() => buttonGroup.blocksRaycasts = true).Restart();

            cancelButton.gameObject.SetActive(true);

            OnConfirmEvent = null;
            OnConfirmEvent += confirmAction;
            if (cancelAction == null) return;
            OnCancelEvent = null;
            OnCancelEvent += cancelAction;
        }

#endregion

#region Public Method

        public static void NonCancelableAlert(FullscreenAlertEnum fullscreenAlertEnum, Action confirmAction,
            Action cancelAction = null)
        {
            instance.NonCancelableAlertPrivate(fullscreenAlertEnum, confirmAction, cancelAction);
        }

        public static void CancelableAlert(FullscreenAlertEnum fullscreenAlertEnum, Action confirmAction,
            Action cancelAction = null, string additionalText = null)
        {
            instance.CancelableAlertPrivate(fullscreenAlertEnum, confirmAction, cancelAction, additionalText);
        }

#endregion
    }
}