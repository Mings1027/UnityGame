using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MapSelectPanel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Sequence _panelSequence;

        [SerializeField] private RectTransform difficultyButtonGroup;
        [SerializeField] private NoticePanel deleteSurviveWavePanel;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_canvasGroup.DOFade(1, 0.25f).From(0))
                .Join(_canvasGroup.GetComponent<RectTransform>().DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _canvasGroup.blocksRaycasts = false;
            _panelSequence.OnComplete(() => _canvasGroup.blocksRaycasts = true).Restart();

            var difficultySelectButtons = transform.GetChild(0);
            for (var i = 0; i < difficultySelectButtons.childCount; i++)
            {
                var index = (byte)i;
                difficultySelectButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    UIManager.instance.MapSelectButton(index).Forget();
                    _panelSequence.OnRewind(() =>
                    {
                        _canvasGroup.blocksRaycasts = false;
                        Destroy(gameObject);
                    }).PlayBackwards();
                });
            }

            var survivedWaves = BackendGameData.userData.survivedWaveList;

            for (var i = 0; i < difficultySelectButtons.childCount; i++)
            {
                difficultySelectButtons.GetChild(i).GetChild(3).GetComponent<TMP_Text>().text =
                    survivedWaves[i];
            }

            ButtonInit();
        }

        private void ButtonInit()
        {
            deleteSurviveWavePanel.OnConfirmButtonEvent += () =>
            {
                var survivedWaves = BackendGameData.userData.survivedWaveList;

                for (var i = 0; i < difficultyButtonGroup.childCount; i++)
                {
                    difficultyButtonGroup.GetChild(i).GetComponent<DifficultyButton>().survivedText.text =
                        survivedWaves[i];
                }
            };
        }
    }
}