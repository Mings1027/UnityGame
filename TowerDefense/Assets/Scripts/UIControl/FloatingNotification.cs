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
    public class FloatingNotification : MonoSingleton<FloatingNotification>
    {
        private Dictionary<FloatingNotifyEnum, string> _floatingNotifyDic;
        private Sequence _notifySequence;
        private bool _isSequencePlaying;

        [SerializeField] private CanvasGroup notifyGroup;
        [SerializeField] private TMP_Text notifyText;

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += ChangeNotifyLocale;
        }

        private void Start()
        {
            Init().Forget();
            InitTween();
        }

        private void OnDisable()
        {
            _notifySequence?.Kill();
            LocalizationSettings.SelectedLocaleChanged -= ChangeNotifyLocale;
        }

        private async UniTaskVoid Init()
        {
            _floatingNotifyDic = new Dictionary<FloatingNotifyEnum, string>();

            var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(LocaleManager.FloatingNotifyTable);
            await tableOperation;
            if (tableOperation.Status == AsyncOperationStatus.Succeeded)
            {
                var dic = tableOperation.Result.ToDictionary(v => v.Value);
                var floatingNotifies = Enum.GetValues(typeof(FloatingNotifyEnum));

                foreach (FloatingNotifyEnum notify in floatingNotifies)
                {
                    foreach (var kvp in dic)
                    {
                        if (notify.ToString() != kvp.Key.Key) continue;
                        _floatingNotifyDic.Add(notify, kvp.Key.Value);
                        break;
                    }
                }
            }
        }

        private void ChangeNotifyLocale(Locale locale)
        {
            foreach (var notifyString in _floatingNotifyDic.Keys.ToList())
            {
                _floatingNotifyDic[notifyString] =
                    LocaleManager.GetLocalizedString(LocaleManager.FloatingNotifyTable, notifyString.ToString());
            }
        }

        private void InitTween()
        {
            var noticeDiaRect = notifyGroup.GetComponent<RectTransform>();

            _notifySequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notifyGroup.DOFade(1, 0.2f).From(0))
                .Join(noticeDiaRect.DOAnchorPosX(0, 0.25f).From(new Vector2(1000, -50)))
                .Append(noticeDiaRect.DOAnchorPosY(100, 0.25f).SetDelay(2))
                .Join(notifyGroup.DOFade(0, 0.25f).From(1));
        }

        private void FloatingNotifyPrivate(FloatingNotifyEnum floatingNotifyEnum)
        {
            if (_isSequencePlaying) return;
            _isSequencePlaying = true;
            notifyText.text = _floatingNotifyDic[floatingNotifyEnum];
            _notifySequence.OnComplete(() => _isSequencePlaying = false).Restart();
        }

        public static void FloatingNotify(FloatingNotifyEnum floatingNotifyEnum)
        {
            instance.FloatingNotifyPrivate(floatingNotifyEnum);
        }
    }
}