using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class LanguageController : MonoBehaviour
    {
        private Sequence _panelSequence;

        [SerializeField] private Image languagePanelBlockImage;
        [SerializeField] private CanvasGroup languagePanel;
        [SerializeField] private Transform languageButtons;
        [SerializeField] private Button openLanguagePanelButton;
        [SerializeField] private Button closeLanguagePanelButton;

        private void Start()
        {
            languagePanelBlockImage.enabled = false;
            _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(languagePanel.DOFade(1, 0.25f).From(0).SetUpdate(true))
                .Join(languagePanel.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            languagePanel.blocksRaycasts = false;
            for (var i = 0; i < languageButtons.childCount; i++)
            {
                var index = i;
                var languageButton = languageButtons.GetChild(i).GetComponent<Button>();
                languageButton.onClick
                    .AddListener(() =>
                    {
                        languageButton.transform.DOScale(1, 0.25f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                        SoundManager.PlayUISound(SoundEnum.ButtonSound);
                        LocaleManager.ChangeLocale(index);
                    });
            }

            openLanguagePanelButton.onClick.AddListener(() =>
            {
                openLanguagePanelButton.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack).SetUpdate(true);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                languagePanelBlockImage.enabled = true;
                _panelSequence.OnComplete(() => languagePanel.blocksRaycasts = true).Restart();
            });

            closeLanguagePanelButton.onClick.AddListener(() =>
            {
                languagePanelBlockImage.enabled = false;
                _panelSequence.OnRewind(() => languagePanel.blocksRaycasts = false).PlayBackwards();
            });
        }

        private void OnDestroy()
        {
            _panelSequence?.Kill();
        }
    }
}