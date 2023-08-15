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
    public class TowerManager : Singleton<TowerManager>
    {
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
        private int _lifeCount;
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

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        private void TowerButtonInit()
        {
            _showTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerBtn = towerButtons.GetChild(i);
                _showTowerBtnSequence.Append(towerBtn.DOLocalMoveY(-150, 0.1f)
                    .From().SetEase(Ease.OutBack).SetRelative());
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
            goldText.text = _towerGold.ToString();
            _lifeCount = 20;
            lifeCountText.text = _lifeCount.ToString();
            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            returnToMainMenuButton.onClick.AddListener(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
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
            towerInfoUI.enabled = true;
            OffIndicator();
            _isPanelOpen = true;

            _curSelectedTower = clickedTower;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);

            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);
            moveUnitButton.SetActive(_curSelectedTower.IsUnitTower);

            OpenEditButtonPanel();
            SetTowerInfo();
        }

        private void OpenEditButtonPanel()
        {
            _towerSelectPanelTween.Restart();

            if (!towerInfoUI.gameObject.activeSelf) towerInfoUI.gameObject.SetActive(true);
            offUIButton.gameObject.SetActive(true);

            if (_curSelectedTower.IsUnitTower) return;
            var targetingTower = _curSelectedTower.GetComponent<TargetingTower>();
            var indicatorTransform = towerRangeIndicator.transform;
            var position = _curSelectedTower.transform.position;
            indicatorTransform.position = new Vector3(position.x, position.y + 0.1f, position.z);
            indicatorTransform.localScale =
                new Vector3(targetingTower.TowerRange * 2, targetingTower.TowerRange * 2, 0);

            towerRangeIndicator.enabled = true;
        }

        private void SetTowerInfo()
        {
            var towerLevel = towerData[(int)_curSelectedTower.towerTypeEnum];
            var towerLevelData = towerLevel.towerLevels[_curSelectedTower.TowerLevel];

            _sellTowerGold = GetTowerGold(_curSelectedTower);

            towerInfoUI.SetText(_curSelectedTower.TowerLevel.ToString(), towerLevelData.damage.ToString(),
                towerLevelData.attackRange.ToString(),
                towerLevelData.attackDelay.ToString(CultureInfo.InvariantCulture),
                _sellTowerGold.ToString(),
                towerTierName[_curSelectedTower.TowerLevel] + towerLevel.towerName);

            if (_curSelectedTower.TryGetComponent(out TargetingTower targetingTower))
            {
                var indicatorTransform = towerRangeIndicator.transform;
                var position = targetingTower.transform.position;
                indicatorTransform.position = new Vector3(position.x, position.y + 0.1f, position.z);
                indicatorTransform.localScale =
                    new Vector3(targetingTower.TowerRange * 2, targetingTower.TowerRange * 2, 0);
            }
        }

        private void TowerUpgrade()
        {
            if (!CheckGold())
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
            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);

            if (!_isPanelOpen) return;
            SetTowerInfo();
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);
            OffIndicator();

            moveUnitIndicator.UnitTower = _curSelectedTower.GetComponent<UnitTower>();
            var moveIndicatorTransform = moveUnitIndicator.transform;
            moveIndicatorTransform.position = _curSelectedTower.transform.position;
            moveIndicatorTransform.localScale = new Vector3(moveUnitRange, moveUnitRange);
            moveUnitIndicator.gameObject.SetActive(true);
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? "SellTower1" :
                _curSelectedTower.TowerLevel < 4 ? "SellTower2" : "SellTower3");

            _curSelectedTower.gameObject.SetActive(false);
            ObjectPoolManager.Get(PoolObjectName.BuildSmoke, _curSelectedTower.transform);
            // ObjectPoolManager.Get(PoolObjectName.BuildingPoint, _curSelectedTower.transform);

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

            offUIButton.gameObject.SetActive(false);
            towerInfoUI.enabled = false;
            _isPanelOpen = false;
            _towerSelectPanelTween.PlayBackwards();
            OffIndicator();
        }

        public bool CheckGold()
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
            _isSpeedUp = !_isSpeedUp;
            if (_isSpeedUp)
            {
                Time.timeScale = 2f;
                speedUpText.text = "x2";
                return;
            }

            Time.timeScale = 1f;
            speedUpText.text = "x1";
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
            _lifeCount -= 1;
            lifeCountText.text = _lifeCount.ToString();
            if (_lifeCount > 0) return;
            GameOver();
        }

        #endregion
    }
}