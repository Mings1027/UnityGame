using System;
using GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class InformationUIController : Singleton<InformationUIController>
    {
        private bool _isPause;

        // public int[] TowerCoin => towerCoin;
        // public int[] GetTowerCoin => getTowerCoin;
        //
        // [SerializeField] private int[] towerCoin;
        // [SerializeField] private int[] getTowerCoin;

        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button pauseButton;

        private void Awake()
        {
            pauseButton.onClick.AddListener(Pause);
        }

        private void Pause()
        {
            _isPause = !_isPause;
            Time.timeScale = _isPause ? 0 : 1;
            menuPanel.SetActive(_isPause);
        }
    }
}