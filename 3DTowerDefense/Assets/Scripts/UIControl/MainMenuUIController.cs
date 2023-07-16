using System;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        [FormerlySerializedAs("gamePlayManager")] [SerializeField] private GameController gameController;
        [SerializeField] private InfoUIController infoUIController;
        [SerializeField] private CameraManager cameraManager;

        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private Transform startPanel;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            for (var i = 0; i < stageSelectPanel.transform.childCount; i++)
            {
                var index = i;
                stageSelectPanel.transform.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() =>
                    {
                        SoundManager.Instance.PlayBGM(SoundManager.ForestBGM);
                        stageSelectPanel.SetActive(false);
                        gameController.MapGenerator(index);
                        gameController.ReStart();
                        infoUIController.Init(index);
                        cameraManager.enabled = true;
                    });
            }
        }

        private void Start()
        {
            stageSelectPanel.SetActive(true);
            startPanel.gameObject.SetActive(true);
        }

        private void StartGame()
        {
            Time.timeScale = 1;
            startPanel.DOMoveY(Screen.width * 2, 1).SetEase(Ease.InBack);
            SoundManager.Instance.PlayBGM(SoundManager.BGM);
        }
    }
}