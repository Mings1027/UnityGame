using System;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        [SerializeField] private GamePlayManager gamePlayManager;

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
                        gamePlayManager.MapGenerator(index);
                        gamePlayManager.ReStart();
                    });
            }
        }

        private void Start()
        {
            stageSelectPanel.SetActive(true);
            startPanel.gameObject.SetActive(true);
        }

        public void SetStageSelectPanel(bool isActive)
        {
            stageSelectPanel.SetActive(isActive);
        }

        private void StartGame()
        {
            startPanel.DOMoveY(Screen.width * 2, 2).SetEase(Ease.InBack);
        }
    }
}