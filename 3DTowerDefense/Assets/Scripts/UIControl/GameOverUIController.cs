using System;
using GameControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControl
{
    public class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private GamePlayManager gamePlayManager;
        [SerializeField] private MainMenuUIController mainMenuUIController;
        [SerializeField] private WaveManager waveManager;

        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button reStartButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            reStartButton.onClick.AddListener(() =>
            {
                ObjectPoolManager.ReStart();
                mainMenuUIController.SetStageSelectPanel(true);
                gamePlayManager.ReStart();
                gamePlayManager.BuildPointInit();
                waveManager.Init();
            });
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        public void SetGameOverPanel(bool isActive)
        {
            gameOverPanel.SetActive(isActive);
        }
    }
}