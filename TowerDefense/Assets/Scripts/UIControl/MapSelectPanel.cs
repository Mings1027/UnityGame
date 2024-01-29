using System;
using BackendControl;
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
        private Tween _deletePanelTween;

        [SerializeField] private Transform deleteWaveDataPanel;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button dataDeleteButton;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private void Awake()
        {
            _deletePanelTween = deleteWaveDataPanel.GetChild(1).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
        }

        private void OnDestroy()
        {
            _deletePanelTween?.Kill();
        }

        private void Start()
        {
            _eventSystem = EventSystem.current;
            _eventSystem.enabled = false;
            blockImage.enabled = false;

            transform.DOScale(1, 0.5f).From(0.7f).SetEase(Ease.OutBack).OnComplete(() => _eventSystem.enabled = true);
            var difficultySelectButtons = transform.GetChild(0);
            Debug.Log("==============================================================");
            for (var i = 0; i < difficultySelectButtons.childCount; i++)
            {
                var index = (byte)i;
                difficultySelectButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    UIManager.instance.MapSelectButton(index).Forget();
                    _eventSystem.enabled = false;
                    transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        _eventSystem.enabled = true;
                        Destroy(gameObject);
                    });
                });
                Debug.Log(index);
            }

            var survivedWaves = BackendGameData.userData.survivedWaveList;
            Debug.Log($"wave length : {survivedWaves.Count}");
            for (int i = 0; i < survivedWaves.Count; i++)
            {
                Debug.Log($"survivedwaves : {survivedWaves[i]}");
            }

            for (var i = 0; i < difficultySelectButtons.childCount; i++)
            {
                Debug.Log($"text : {survivedWaves[i]}");
                difficultySelectButtons.GetChild(i).GetChild(3).GetComponent<TMP_Text>().text =
                    survivedWaves[i].ToString();
            }

            ButtonInit();
        }

        private void ButtonInit()
        {
            dataDeleteButton.onClick.AddListener(() =>
            {
                blockImage.enabled = true;
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                _deletePanelTween.OnComplete(() => _eventSystem.enabled = true).Restart();
            });
            yesButton.onClick.AddListener(() =>
            {
                blockImage.enabled = false;
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                // DataManager.WaveDataInit();
                _deletePanelTween.OnRewind(() => _eventSystem.enabled = true).PlayBackwards();

                // DataManager.LoadData();
                var survivedWaves = BackendGameData.userData.survivedWaveList;
                var difficultySelectButtons = transform.GetChild(0);
                for (var i = 0; i < difficultySelectButtons.childCount; i++)
                {
                    difficultySelectButtons.GetChild(i).GetChild(3).GetComponent<TMP_Text>().text =
                        survivedWaves[i].ToString();
                }
            });
            noButton.onClick.AddListener(() =>
            {
                blockImage.enabled = false;
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _eventSystem.enabled = false;
                _deletePanelTween.OnRewind(() => _eventSystem.enabled = true).PlayBackwards();
            });
        }
    }
}