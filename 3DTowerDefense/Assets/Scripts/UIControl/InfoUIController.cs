using DG.Tweening;
using ManagerControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControl
{
    public class InfoUIController : MonoBehaviour
    {
        private int _towerCoin;
        private int _lifeCount;
        private Tween _menuPanelTween;
        private Tween _gameUITween;

        public bool IsPause { get; private set; }

        [SerializeField] private GamePlayManager gamePlayManager;

        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int[] stageStartCoin;
        [SerializeField] private int[] towerBuildCoin;

        [SerializeField] private Transform gameUI;
        [SerializeField] private Transform menuPanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button bgmButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Sprite musicOnImage;
        [SerializeField] private Sprite musicOffImage;

        private void Awake()
        {
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

            _gameUITween = gameUI.DOLocalMoveY(0, 0.5f)
                .From(Screen.width).SetUpdate(true).SetAutoKill(false).Pause();

            _menuPanelTween = menuPanel.DOLocalMoveY(0, 0.5f)
                .From(Screen.width).SetEase(Ease.OutBack).SetUpdate(true).SetAutoKill(false).Pause();
        }

        private void Pause()
        {
            IsPause = true;
            Time.timeScale = 0;
            _gameUITween.PlayBackwards();
            _menuPanelTween.Restart();
        }

        private void Resume()
        {
            IsPause = false;
            Time.timeScale = 1;
            _menuPanelTween.PlayBackwards();
            _gameUITween.Restart();
        }

        private void BGMButton()
        {
            bgmButton.image.sprite = SoundManager.Instance.BGMToggle() ? musicOnImage : musicOffImage;
        }

        public void Init()
        {
            _gameUITween.Restart();
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