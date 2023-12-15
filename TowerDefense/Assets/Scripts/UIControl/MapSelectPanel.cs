using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class MapSelectPanel : MonoBehaviour
    {
        private EventSystem _eventSystem;
        private Transform _deleteDataPanel;
        private Tween _deletePanelTween;

        [SerializeField] private Image deleteWaveDataPanel;
        [SerializeField] private Button dataDeleteButton;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private void Awake()
        {
            _deleteDataPanel = deleteWaveDataPanel.transform.GetChild(0);
            _deletePanelTween = _deleteDataPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false)
                .Pause();
        }

        private void Start()
        {
            _eventSystem = EventSystem.current;
            _eventSystem.enabled = false;

            deleteWaveDataPanel.enabled = false;
            transform.DOScale(1, 0.5f).From(0.3f).SetEase(Ease.OutBack).OnComplete(() => _eventSystem.enabled = true);
            var difficultySelectButtons = transform.GetChild(0);
            for (var i = 0; i < difficultySelectButtons.childCount; i++)
            {
                var index = i;
                difficultySelectButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (Input.touchCount > 1) return;
                    SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                    SoundManager.Instance.PlayBGM(SoundEnum.WaveEnd);
                    UIManager.Instance.MapSelectButton(index + 1).Forget();
                    _eventSystem.enabled = false;
                    transform.DOScale(0, 0.5f).SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            _eventSystem.enabled = true;
                            Destroy(gameObject);
                        });
                });
            }

            DataManager.LoadData();
            var survivedWaves = DataManager.SurvivedWaves.survivedWave;
            for (int i = 0; i < difficultySelectButtons.childCount; i++)
            {
                difficultySelectButtons.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text =
                    survivedWaves[i].ToString();
            }

            ButtonInit();
        }

        private void ButtonInit()
        {
            dataDeleteButton.onClick.AddListener(() =>
            {
                if (Input.touchCount > 1) return;
                deleteWaveDataPanel.enabled = true;
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                _deletePanelTween.OnComplete(() => _eventSystem.enabled = true).Restart();
            });
            yesButton.onClick.AddListener(() =>
            {
                if (Input.touchCount > 1) return;
                deleteWaveDataPanel.enabled = false;
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                DataManager.WaveDataInit();
                _deletePanelTween.OnRewind(() => _eventSystem.enabled = true).PlayBackwards();

                DataManager.LoadData();
                var survivedWaves = DataManager.SurvivedWaves.survivedWave;
                var difficultySelectButtons = transform.GetChild(0);
                for (var i = 0; i < difficultySelectButtons.childCount; i++)
                {
                    difficultySelectButtons.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text =
                        survivedWaves[i].ToString();
                }
            });
            noButton.onClick.AddListener(() =>
            {
                if (Input.touchCount > 1) return;
                deleteWaveDataPanel.enabled = false;
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                _deletePanelTween.OnRewind(() => _eventSystem.enabled = true).PlayBackwards();
            });
        }
    }
}