using System;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class LanguageController : MonoBehaviour
    {
        private Tween _changeLanguageTween;

        [SerializeField] private Transform languagePanel;
        [SerializeField] private Transform languageButtons;
        [SerializeField] private Button openLanguagePanelButton;
        [SerializeField] private Button closeLanguagePanelButton;

        private void Start()
        {
            _changeLanguageTween =
                languagePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true).SetAutoKill(false)
                    .Pause();

            for (var i = 0; i < languageButtons.childCount; i++)
            {
                var index = i;
                var languageButton = languageButtons.GetChild(i).GetComponent<Button>();
                languageButton.onClick
                    .AddListener(() =>
                    {
                        languageButton.transform.DOScale(1, 0.25f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                        SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                        LocaleManager.ChangeLocale(index);
                    });
            }

            openLanguagePanelButton.onClick.AddListener(() =>
            {
                openLanguagePanelButton.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack).SetUpdate(true);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _changeLanguageTween.Restart();
            });

            closeLanguagePanelButton.onClick.AddListener(() => { _changeLanguageTween.PlayBackwards(); });
        }

        private void OnDestroy()
        {
            _changeLanguageTween?.Kill();
        }
    }
}