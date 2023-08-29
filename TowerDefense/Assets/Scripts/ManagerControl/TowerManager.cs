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
        private TowerInfo _towerInfo;

        private Camera _cam;

        //  Tower Buttons
        private Sequence _showTowerBtnSequence;
        private bool _isShowTowerBtn;

        //  Tower Panel
        private Tween _towerSelectPanelTween;
        private Tower _curSelectedTower;

        private bool _isPanelOpen;
        private bool _isTower;

        private int _sellTowerGold;
        private float _prevSize;
        private Vector3 _prevPos;

        //  HUD
        public bool IsPause { get; private set; }

        private int _towerGold;
        private int _curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;
        private Tween _menuPanelTween;
        private Tween _notEnoughGoldTween;

        private CameraManager _cameraManager;

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

        [Header("----------Indicator----------")] [SerializeField]
        private GameObject towerRangeIndicator;

        [SerializeField] private MoveUnitIndicator moveUnitIndicator;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
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
            IndicatorInit();
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
            }
        }

        private void TowerEditButtonInit()
        {
            _towerSelectPanelTween = towerInfoUI.transform.DOScale(1, 0.1f).From(0).SetAutoKill(false);
            upgradeButton.GetComponent<Button>().onClick.AddListener(TowerUpgrade);
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                FocusUnitTower();
                MoveUnitButton();
            });
            sellTowerButton.GetComponent<Button>().onClick.AddListener(SellTower);
        }

        private void MenuButtonInit()
        {
            _towerGold = startGold;
            _curSpeed = 1;
            goldText.text = _towerGold.ToString();
            lifeCountText.text = lifeCount.ToString();
            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            returnToMainMenuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(0);
                DataManager.Instance.SaveDamageData();
            });
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

        private void IndicatorInit()
        {
            towerRangeIndicator.SetActive(false);
            moveUnitIndicator.gameObject.SetActive(false);
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
            DataManager.Instance.SaveDamageData();
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

            _towerInfo.towerName = towerTierName[_curSelectedTower.TowerLevel] + towerLevel.towerName;
            _towerInfo.level = _curSelectedTower.TowerLevel + 1;
            _towerInfo.damage = towerLevelData.damage;
            _towerInfo.range = towerLevelData.attackRange;
            _towerInfo.delay = towerLevelData.attackDelay;
            _towerInfo.sellCoin = _sellTowerGold = GetTowerGold(_curSelectedTower);

            towerInfoUI.SetTowerInfo(_towerInfo);

            SetTargetingTowerIndicator();
        }

        private void SetTargetingTowerIndicator()
        {
            if (!_curSelectedTower.TryGetComponent(out TargetingTower targetingTower)) return;
            var indicatorTransform = towerRangeIndicator.transform;
            var pos = targetingTower.transform.position;
            indicatorTransform.position = pos + new Vector3(0, 0.1f, 0);
            indicatorTransform.localScale =
                new Vector3(targetingTower.TowerRange, 0.2f, targetingTower.TowerRange);
            if (!towerRangeIndicator.activeSelf) towerRangeIndicator.SetActive(true);
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
            tempTower.DisableOutline();
            if (!_isPanelOpen) return;
            UpdateTowerInfo();
        }

        private void FocusUnitTower()
        {
            _prevSize = _cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            _cam.DOOrthoSize(10, 0.5f).SetEase(Ease.OutExpo);
            _cameraManager.transform.DOMove(_curSelectedTower.transform.position, 0.5f).SetEase(Ease.OutExpo);
        }

        public void RewindCamState()
        {
            _cam.DOOrthoSize(_prevSize, 0.5f).SetEase(Ease.OutExpo);
            _cameraManager.transform.DOMove(_prevPos, 0.5f).SetEase(Ease.OutExpo);
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);

            _curSelectedTower.TryGetComponent(out UnitTower unitTower);
            moveUnitIndicator.UnitTower = unitTower;
            var moveIndicatorTransform = moveUnitIndicator.transform;
            moveIndicatorTransform.position = _curSelectedTower.transform.position + new Vector3(0, 0.1f, 0);
            moveIndicatorTransform.localScale = new Vector3(unitTower.MoveUnitRange, 0.2f, unitTower.MoveUnitRange);
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
            if (towerRangeIndicator.activeSelf) towerRangeIndicator.SetActive(false);
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
            _curSelectedTower.DisableOutline();
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
            _curSpeed = _curSpeed % 3 + 1;
            Time.timeScale = _curSpeed;
            speedUpText.text = $"x{_curSpeed}";
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