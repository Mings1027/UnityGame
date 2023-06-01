using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class InformationUIController : MonoBehaviour
    {
        private int _towerCoin;
        private int _lifeCount;
        private bool _isPause;

        [SerializeField] private GamePlayManager gamePlayManager;

        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int[] stageStartCoin;
        [SerializeField] private int[] towerBuildCoin;

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

        public void Init()
        {
            _towerCoin = stageStartCoin[0];
            coinText.text = _towerCoin.ToString();
            _lifeCount = 20;
            lifeCountText.text = _lifeCount.ToString();
        }

        public void IncreaseCoin(int amount)
        {
            _towerCoin += amount;
            coinText.text = _towerCoin.ToString();
        }

        public void DecreaseCoin(int index)
        {
            _towerCoin -= towerBuildCoin[index];
            coinText.text = _towerCoin.ToString();
        }

        public bool CheckBuildCoin(int index)
        {
            var t = towerBuildCoin[index];
            return _towerCoin >= t;
        }

        public int GetTowerCoin(Tower tower)
        {
            var towerLevel = tower.IsUniqueTower ? 4 : tower.TowerLevel + 1;
            var sum = 0;
            for (var i = 0; i < towerLevel; i++)
            {
                sum += towerBuildCoin[i];
            }

            return sum;
        }

        public void DecreaseLifeCount()
        {
            _lifeCount -= 1;
            lifeCountText.text = _lifeCount.ToString();
            if (_lifeCount > 0) return;
            gamePlayManager.GameOver();
        }
    }
}