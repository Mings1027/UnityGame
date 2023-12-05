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
        private Sequence _changeLanguageSequence;

        [SerializeField] private Transform languagePanel;
        [SerializeField] private Transform languageButtons;
        [SerializeField] private Button openLanguagePanelButton;
        [SerializeField] private Button closeLanguagePanelButton;

        private void Start()
        {
            _changeLanguageSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(languagePanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack));

            for (int i = 0; i < languageButtons.childCount; i++)
            {
                var index = i;
                languageButtons.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() =>
                    {
                        SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                        LocaleManager.ChangeLocale(index);
                    });
            }

            openLanguagePanelButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _changeLanguageSequence.Restart();
            });

            closeLanguagePanelButton.onClick.AddListener(() => { _changeLanguageSequence.PlayBackwards(); });
        }
    }
}