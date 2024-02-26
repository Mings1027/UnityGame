using System;
using System.Collections.Generic;
using System.Linq;
using AdsControl;
using CurrencyControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class LobbyUI : MonoBehaviour
    {
        private bool _isSequencePlaying;
        private Dictionary<NoticeTableEnum, string> _noticeDic;

        private Sequence _noticeSequence;
        private Sequence _emeraldNoticeSequence;
        private Sequence _duplicateSequence;
        private Sequence _successChangeNameSequence;

        [field: SerializeField] public DiamondCurrency diamondCurrency { get; private set; }
        [field: SerializeField] public EmeraldCurrency emeraldCurrency { get; private set; }

        [SerializeField] private Button startGameButton;

        [SerializeField] private GameObject buttonsObj;
        [SerializeField] private GameObject inGameMoneyObj;

        [SerializeField] private CanvasGroup noticeGroup;
        [SerializeField] private TMP_Text noticeText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image backgroundBlockImage;

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            var noticeDiaRect = noticeGroup.GetComponent<RectTransform>();

            _noticeSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(noticeDiaRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(noticeDiaRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(noticeGroup.DOFade(0, 0.25f).From(1));
        }

        private void Start()
        {
            Init();
        }

        private void OnDisable()
        {
            _noticeSequence?.Kill();
            _emeraldNoticeSequence?.Kill();
        }

        private void Init()
        {
            FindAnyObjectByType<AdmobManager>().BindLobbyUI(this);
            inGameMoneyObj.SetActive(false);
            startGameButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                FadeController.FadeOutScene("MainGameScene");
            });

            OffBackgroundImage();
            OffBlockImage();
            SetNoticeDic().Forget();
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleNotice;
        }

        private async UniTaskVoid SetNoticeDic()
        {
            _noticeDic = new Dictionary<NoticeTableEnum, string>();
            var loadOperation = LocalizationSettings.StringDatabase.GetTableAsync(LocaleManager.NoticeTable);
            await loadOperation;
            if (loadOperation.Status == AsyncOperationStatus.Succeeded)
            {
                var dic = loadOperation.Result.ToDictionary(p => p.Value);

                var noticeTable = Enum.GetValues(typeof(NoticeTableEnum));
                foreach (NoticeTableEnum key in noticeTable)
                {
                    foreach (var d in dic)
                    {
                        if (key.ToString() == d.Key.Key)
                        {
                            _noticeDic.Add(key, d.Key.Value);
                            break;
                        }
                    }
                }
            }
        }

        private void ChangeLocaleNotice(Locale locale)
        {
            foreach (var noticeString in _noticeDic.Keys.ToList())
            {
                _noticeDic[noticeString] =
                    LocaleManager.GetLocalizedString(LocaleManager.NoticeTable, noticeString.ToString());
                Debug.Log($"key : {noticeString}  value : {_noticeDic[noticeString]}");
            }
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

        public void NoticeTween(NoticeTableEnum noticeTableEnum)
        {
            if (_isSequencePlaying) return;
            noticeText.text = _noticeDic[noticeTableEnum];
            _isSequencePlaying = true;
            _noticeSequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }
    }
}