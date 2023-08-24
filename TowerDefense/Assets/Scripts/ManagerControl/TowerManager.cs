using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using TMPro;
using TowerControl;
using UIControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public struct TowerInfo
    {
        public string towerName;
        public int level;
        public int damage;
        public int range;
        public float delay;
        public int sellCoin;
    }

    public class TowerManager : Singleton<TowerManager>
    {
        private TowerInfo towerInfo;

        //  Tower Buttons
        private Sequence _showTowerBtnSequence;
        private bool _isShowTowerBtn;

        //  Tower Panel
        private Tween _towerSelectPanelTween;
        private Tower _curSelectedTower;

        private bool _isPanelOpen;
        private bool _isTower;

        private int _sellTowerGold;

        //  HUD
        public bool IsPause { get; private set; }

        private int _towerGold;
        private int curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;
        private Tween _menuPanelTween;
        private Tween _notEnoughGoldTween;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private Transform towerButtons;

        [SerializeField] private InputManager inputManager;

        [Header("----------Tower Panel----------")] [SerializeField]
        private TowerData[] towerData;

        [SerializeField] private string[] towerTierName;

        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private Button offUIButton;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("------------HUD------------")] [SerializeField]
        private GameObject hudPanel;

        [SerializeField] private int[] towerBuildGold;
        [SerializeField] private int startGold;

        [SerializeField] private Transform menuPanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button bgmButton;
        [SerializeField] private Button returnToMainMenuButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Sprite musicOnImage;
        [SerializeField] private Sprite musicOffImage;

        [SerializeField] private GameObject notEnoughGoldPanel;
        [SerializeField, Range(0, 30)] private int lifeCount;
        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI speedUpText;

        [Header("----------Game Over----------")] [SerializeField, Space(10)]
        private GameObject gameOverPanel;

        [SerializeField] private Button reStartButton;
        [SerializeField] private SpriteRenderer towerRangeIndicator;
        [SerializeField] private MoveUnitIndicator moveUnitIndicator;
        [SerializeField] private float moveUnitRange;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            TowerButtonInit();
            TowerEditButtonInit();
            MenuButtonInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            hudPanel.SetActive(false);
            _menuPanelTween.PlayBackwards();
            _notEnoughGoldTween.PlayBackwards();
            moveUnitIndicator.gameObject.SetActive(false);
            gameOverPanel.SetActive(false);
            offUIButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _towerSelectPanelTween.Kill();
            _notEnoughGoldTween.Kill();
            _menuPanelTween.Kill();
        }

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        private void TowerButtonInit()
        {
            _showTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerBtn = towerButtons.GetChild(i);
                _showTowerBtnSequence.Append(towerBtn.DOLocalMoveY(-200, 0.1f)
                    .From().SetEase(Ease.InOutBack).SetRelative());
                var eventTrigger = towerBtn.GetComponent<EventTrigger>();
                var entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };
                var towerBtnName = towerBtn.name.Replace("Button", "");

                entry.callback.AddListener(_ =>
                {
                    inputManager.enabled = true;
                    inputManager.StartPlacement(towerBtnName);
                });
                eventTrigger.triggers.Add(entry);
            }
        }

        private void TowerEditButtonInit()
        {
            _towerSelectPanelTween = towerInfoUI.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
            upgradeButton.GetComponent<Button>().onClick.AddListener(TowerUpgrade);
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            sellTowerButton.GetComponent<Button>().onClick.AddListener(SellTower);
        }

        private void MenuButtonInit()
        {
            _towerGold = startGold;
            curSpeed = 1;
            goldText.text = _towerGold.ToString();
            lifeCountText.text = lifeCount.ToString();
            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            returnToMainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            speedUpButton.onClick.AddListener(SpeedUp);

            _menuPanelTween = menuPanel.DOScale(1, 0.25f)
                .From(0).SetAutoKill(false)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            _notEnoughGoldTween = notEnoughGoldPanel.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                .SetAutoKill(false);
        }

        private void GameOverPanelInit()
        {
            reStartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        public void PlaceTower(string towerName, Vector3 snappedPos)
        {
            _curSelectedTower = ObjectPoolManager.Get<Tower>(towerName, snappedPos);
            _curSelectedTower.OnClickTower += ClickTower;
            TowerUpgrade();
        }

        public void GameStart()
        {
            hudPanel.SetActive(true);
            Time.timeScale = 1;
        }

        private void GameOver()
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }

        public void ShowTowerButtons()
        {
            if (!_isShowTowerBtn)
            {
                _isShowTowerBtn = true;
                _showTowerBtnSequence.Restart();
                inputManager.enabled = true;
            }
            else
            {
                _isShowTowerBtn = false;
                _showTowerBtnSequence.PlayBackwards();
                inputManager.enabled = false;
            }
        }

        private void ClickTower(Tower clickedTower)
        {
            _isTower = true;
            towerInfoUI.enabled = true;
            // OffIndicator();
            _isPanelOpen = true;

            _curSelectedTower = clickedTower;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);

            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);
            moveUnitButton.SetActive(_curSelectedTower.TryGetComponent(out UnitTower _));

            OpenEditButtonPanel();
            UpdateTowerInfo();
        }

        private void OpenEditButtonPanel()
        {
            _towerSelectPanelTween.Restart();

            if (!towerInfoUI.gameObject.activeSelf) towerInfoUI.gameObject.SetActive(true);
            offUIButton.gameObject.SetActive(true);

            SetTargetingTowerIndicator();
        }

        private void UpdateTowerInfo()
        {
            var towerLevel = towerData[(int)_curSelectedTower.towerTypeEnum];
            var towerLevelData = towerLevel.towerLevels[_curSelectedTower.TowerLevel];

            towerInfo.towerName = towerTierName[_curSelectedTower.TowerLevel] + towerLevel.towerName;
            towerInfo.level = _curSelectedTower.TowerLevel + 1;
            towerInfo.damage = towerLevelData.damage;
            towerInfo.range = towerLevelData.attackRange;
            towerInfo.delay = towerLevelData.attackDelay;
            towerInfo.sellCoin = _sellTowerGold = GetTowerGold(_curSelectedTower);

            towerInfoUI.SetTowerInfo(towerInfo);

            SetTargetingTowerIndicator();
        }

        private void SetTargetingTowerIndicator()
        {
            if (!_curSelectedTower.TryGetComponent(out TargetingTower targetingTower)) return;
            var indicatorTransform = towerRangeIndicator.transform;
            var pos = targetingTower.transform.position;
            indicatorTransform.position = pos + new Vector3(0, 0.1f, 0);
            indicatorTransform.localScale =
                new Vector3(targetingTower.TowerRange * 2, targetingTower.TowerRange * 2, 0);
            if (!towerRangeIndicator.enabled) towerRangeIndicator.enabled = true;
        }

        private void TowerUpgrade()
        {
            if (!EnoughGold())
            {
                notEnoughGoldPanel.transform.DOScale(1, 0.1f).SetEase(Ease.OutBounce);
                return;
            }

            var tempTower = _curSelectedTower;
            var t = towerData[(int)tempTower.towerTypeEnum];
            tempTower.TowerLevelUp();
            var tt = t.towerLevels[tempTower.TowerLevel];
            DecreaseGold(tempTower.TowerLevel);

            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(tempTower.TowerLevel != 4);

            if (!_isPanelOpen) return;
            UpdateTowerInfo();
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);
            // OffIndicator();

            moveUnitIndicator.UnitTower = _curSelectedTower.GetComponent<UnitTower>();
            var moveIndicatorTransform = moveUnitIndicator.transform;
            var pos = _curSelectedTower.transform.position;
            moveIndicatorTransform.position = pos + new Vector3(0, 0.1f, 0);
            moveIndicatorTransform.localScale = new Vector3(moveUnitRange, moveUnitRange);
            moveUnitIndicator.gameObject.SetActive(true);
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? "SellTower1" :
                _curSelectedTower.TowerLevel < 4 ? "SellTower2" : "SellTower3");

            _curSelectedTower.gameObject.SetActive(false);
            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);

            IncreaseGold(_sellTowerGold);
            ResetUI();
        }

        private void OffIndicator()
        {
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (!moveUnitIndicator.enabled) return;

            moveUnitIndicator.gameObject.SetActive(false);
        }

        public void ResetUI()
        {
            if (!_isPanelOpen) return;
            _isTower = false;
            offUIButton.gameObject.SetActive(false);
            towerInfoUI.enabled = false;
            _isPanelOpen = false;
            _towerSelectPanelTween.PlayBackwards();
            OffIndicator();
        }

        public bool EnoughGold()
        {
            var enoughGold = _towerGold > towerBuildGold[_isTower ? _curSelectedTower.TowerLevel + 1 : 0];
            if (enoughGold) return true;
            NotEnoughGoldPrint().Forget();
            return false;
        }

        private async UniTaskVoid NotEnoughGoldPrint()
        {
            if (_callNotEnoughTween) return;
            _callNotEnoughTween = true;
            notEnoughGoldPanel.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce);
            await UniTask.Delay(1000);
            notEnoughGoldPanel.transform.DOScale(0, 0.2f);
            _callNotEnoughTween = false;
        }

        #region HUD

        private void Pause()
        {
            IsPause = true;
            Time.timeScale = 0;
            _menuPanelTween.Restart();
        }

        private void Resume()
        {
            IsPause = false;
            Time.timeScale = 1;
            _menuPanelTween.PlayBackwards();
        }

        private void BGMButton()
        {
            bgmButton.image.sprite = SoundManager.Instance.BGMToggle() ? musicOnImage : musicOffImage;
        }

        private void SpeedUp()
        {
            curSpeed = curSpeed % 3 + 1;
            Time.timeScale = curSpeed;
            speedUpText.text = $"x{curSpeed}";
        }

        private void DecreaseGold(int index)
        {
            _towerGold -= towerBuildGold[index];
            goldText.text = _towerGold.ToString();
        }

        private int GetTowerGold(Tower tower)
        {
            var towerLevel = tower.TowerLevel + 1;
            var sum = 0;
            for (var i = 0; i < towerLevel; i++)
            {
                sum += towerBuildGold[i];
            }

            return sum;
        }

        public void IncreaseGold(int amount)
        {
            _towerGold += amount;
            goldText.text = _towerGold.ToString();
        }

        public void DecreaseLifeCount()
        {
            if (lifeCount == 0) return;
            lifeCount -= 1;
            lifeCountText.text = lifeCount.ToString();
            if (lifeCount > 0) return;
            GameOver();
        }

        #endregion
    }
}