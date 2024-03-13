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
        [SerializeField] private FullscreenAlert deleteSurviveWavePanel;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_canvasGroup.DOFade(1, 0.25f).From(0))
                .Join(_canvasGroup.GetComponent<RectTransform>().DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _canvasGroup.blocksRaycasts = false;
            _panelSequence.OnComplete(() => _canvasGroup.blocksRaycasts = true).Restart();

            for (var i = 0; i < difficultyButtonGroup.childCount; i++)
            {
                var index = (byte)i;
                difficultyButtonGroup.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    UIManager.MapSelectButton(index);
                    _canvasGroup.blocksRaycasts = false;
                    _panelSequence.OnRewind(() => { Destroy(gameObject); }).PlayBackwards();
                });
            }

            var survivedWaves = BackendGameData.userData.survivedWaveList;

            for (var i = 0; i < difficultyButtonGroup.childCount; i++)
            {
                difficultyButtonGroup.GetChild(i).GetChild(4).GetComponent<TMP_Text>().text =
                    survivedWaves[i];
            }

            ButtonInit();
        }

        private void ButtonInit()
        {
            deleteSurviveWavePanel.OnConfirmButtonEvent += () =>
            {
                for (var i = 0; i < difficultyButtonGroup.childCount; i++)
                {
                    BackendGameData.userData.survivedWaveList[i] = "0";
                    difficultyButtonGroup.GetChild(i).GetComponent<DifficultyButton>().survivedText.text =
                        "0";
                }
            };
        }
    }
}