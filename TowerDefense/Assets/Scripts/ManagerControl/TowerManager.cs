using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using TMPro;
using TowerControl;
using UIControl;
using UnitControl;
using UnityEngine;
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

        private Dictionary<string, GameObject> _towerDictionary;

        //  Tower Buttons
        private Sequence _onOffTowerBtnSequence;
        private bool _isShowTowerBtn;

        //  Tower Panel
        private Tween _towerSelectPanelTween;
        private Tower _curSelectedTower;

        private bool _isPanelOpen;
        private bool _isTower;

        private int _sellTowerGold;
        private float _prevSize;
        private Vector3 _prevPos;

        // Damage Data
        private TMP_Text[] _damageTexts;

        //  HUD
        public bool IsPause { get; private set; }

        private int _towerGold;
        private int _curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;

        private Sequence _pauseSequence;
        private Tween _notEnoughGoldTween;

        private CameraManager _cameraManager;
        private string[] _towerName;

        [SerializeField] private GameObject[] towerArray;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private Transform towerButtons;

        [SerializeField] private Button showTowerButton;
        [SerializeField] private Button hideTowerButton;

        [SerializeField] private InputManager inputManager;

        [Header("----------Tower Panel----------")] [SerializeField]
        private TowerData[] towerData;

        [SerializeField] private string[] towerTierName;

        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private Button offUIButton;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("------------Damage Data----------")] [SerializeField]
        private GameObject damagePanel;

        [Header("------------HUD------------")] [SerializeField]
        private GameObject hudPanel;

        [SerializeField] private int[] towerBuildGold;
        [SerializeField] private int startGold;

        [SerializeField] private Transform pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button bgmButton;

        [SerializeField] private Button gameEndButton;
        [SerializeField] private Button mainMenuButton;
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
        private MeshRenderer towerRangeIndicator;

        [SerializeField] private MeshRenderer selectedTowerIndicator;

        [SerializeField] private MoveUnitIndicator moveUnitIndicator;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _cameraManager = FindObjectOfType<CameraManager>();
            TowerInit();
            TweenInit();
            TowerButtonInit();
            TowerEditButtonInit();
            MenuButtonInit();
            DamageTextInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            hudPanel.SetActive(false);
            _notEnoughGoldTween.PlayBackwards();
            gameOverPanel.SetActive(false);
            offUIButton.gameObject.SetActive(false);
            IndicatorInit();
        }

        private void OnDestroy()
        {
            _towerSelectPanelTween.Kill();
            _notEnoughGoldTween.Kill();
            _pauseSequence.Kill();
            _onOffTowerBtnSequence.Kill();
        }

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        #region Init

        private void TowerInit()
        {
            _towerName = new string[towerArray.Length];
            for (int i = 0; i < _towerName.Length; i++)
            {
                _towerName[i] = towerArray[i].name;
            }

            _towerDictionary = new Dictionary<string, GameObject>();
            for (int i = 0; i < towerArray.Length; i++)
            {
                _towerDictionary.Add(_towerName[i], towerArray[i]);
            }
        }

        private void TweenInit()
        {
            _onOffTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(showTowerButton.transform.DOLocalMoveY(-650, 0.5f).From(-450).SetEase(Ease.InOutBack))
                .Join(towerButtons.DOLocalMoveY(-200, 0.5f).From(true).SetEase(Ease.InOutBack));

            _towerSelectPanelTween =
                towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetEase(Ease.OutBack).SetAutoKill(false);

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOLocalMoveY(0, 0.5f).From(1200).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOLocalMoveY(665, 0.5f).From(465).SetEase(Ease.InOutBack))
                .PrependCallback(() => _cameraManager.enabled = !_cameraManager.enabled);

            _notEnoughGoldTween = notEnoughGoldPanel.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                .SetAutoKill(false);
        }

        private void TowerButtonInit()
        {
            showTowerButton.onClick.AddListener(ShowTowerButtons);
            hideTowerButton.onClick.AddListener(HideTowerButtons);
            hideTowerButton.gameObject.SetActive(false);
        }

        private void TowerEditButtonInit()
        {
            upgradeButton.TryGetComponent(out Button upgradeBtn);
            upgradeBtn.onClick.AddListener(TowerUpgrade);
            moveUnitButton.TryGetComponent(out Button moveUnitBtn);
            moveUnitBtn.onClick.AddListener(() =>
            {
                FocusUnitTower();
                MoveUnitButton();
            });
            sellTowerButton.TryGetComponent(out Button sellTowerBtn);
            sellTowerBtn.onClick.AddListener(SellTower);
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
            gameEndButton.onClick.AddListener(() =>
            {
                DataManager.SaveDamageData();
                damagePanel.SetActive(true);
                damagePanel.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            });
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            speedUpButton.onClick.AddListener(SpeedUp);
        }

        private void DamageTextInit()
        {
            damagePanel.SetActive(false);
            var damageTextPanel = damagePanel.transform.GetChild(0);
            _damageTexts = new TMP_Text[damageTextPanel.childCount];
            for (var i = 0; i < _damageTexts.Length; i++)
            {
                _damageTexts[i] = damageTextPanel.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
            }
        }

        private void GameOverPanelInit()
        {
            reStartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        private void IndicatorInit()
        {
            towerRangeIndicator.enabled = false;
            selectedTowerIndicator.enabled = false;
            moveUnitIndicator.enabled = false;
        }

        #endregion

        public void SetDamageText(Dictionary<string, int> damageDic)
        {
            var damageArray = damageDic.Values.ToArray();
            for (var i = 0; i < damageArray.Length; i++)
            {
                _damageTexts[i].text = damageArray[i].ToString();
            }
        }

        public void PlaceUnitTower(string towerName, Vector3 snappedPos, RaycastHit hit)
        {
            var unitTower = Instantiate(_towerDictionary[towerName], snappedPos, Quaternion.identity)
                .GetComponent<UnitTower>();
            unitTower.unitSpawnPosition = hit.point;
            _curSelectedTower = unitTower;
#if UNITY_EDITOR
            _curSelectedTower.transform.SetParent(transform);
#endif
            _curSelectedTower.OnClickTower += ClickTower;
            TowerUpgrade();
        }

        public void PlaceTower(string towerName, Vector3 snappedPos)
        {
            _curSelectedTower = Instantiate(_towerDictionary[towerName], snappedPos, Quaternion.identity)
                .GetComponent<Tower>();
#if UNITY_EDITOR
            _curSelectedTower.transform.SetParent(transform);
#endif
            _curSelectedTower.OnClickTower += ClickTower;
            TowerUpgrade();
        }

        public void GameStart()
        {
            hudPanel.SetActive(true);
            Time.timeScale = 1;
            _cam.DOOrthoSize(20, 1).From(100).SetEase(Ease.OutQuad);
        }

        private void GameOver()
        {
            gameOverPanel.SetActive(true);
            _cameraManager.enabled = false;
            Time.timeScale = 0;
            DataManager.SaveDamageData();
        }

        private void ShowTowerButtons()
        {
            if (_isShowTowerBtn) return;
            _isShowTowerBtn = true;
            inputManager.enabled = true;
            _onOffTowerBtnSequence.Restart();
            hideTowerButton.gameObject.SetActive(true);
        }

        private void HideTowerButtons()
        {
            if (!_isShowTowerBtn) return;
            if (Input.GetTouch(0).deltaPosition != Vector2.zero) return;
            _isShowTowerBtn = false;
            inputManager.enabled = false;
            _onOffTowerBtnSequence.PlayBackwards();
            hideTowerButton.gameObject.SetActive(false);
        }

        private void ClickTower(Tower clickedTower)
        {
            _isTower = true;
            _isPanelOpen = true;

            _curSelectedTower = clickedTower;

            selectedTowerIndicator.transform.position = _curSelectedTower.transform.position;
            if (!selectedTowerIndicator.enabled)
                selectedTowerIndicator.enabled = true;

            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);
            moveUnitButton.SetActive(_curSelectedTower.TryGetComponent(out UnitTower _));

            OpenEditButtonPanel();
            UpdateTowerInfo();
        }

        private void OpenEditButtonPanel()
        {
            _towerSelectPanelTween.Restart();

            towerInfoUI.enabled = true;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
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
            if (!towerRangeIndicator.enabled) towerRangeIndicator.enabled = true;
        }

        private void TowerUpgrade()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            if (!EnoughGold())
            {
                _notEnoughGoldTween.Restart();
                return;
            }

            var tempTower = _curSelectedTower;
            var t = towerData[(int)tempTower.towerTypeEnum];
            tempTower.TowerLevelUp();
            var tt = t.towerLevels[tempTower.TowerLevel];
            DecreaseGold(tempTower.TowerLevel);

            ObjectPoolManager.Get(StringManager.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(tempTower.TowerLevel != 4);
            if (!_isPanelOpen) return;
            UpdateTowerInfo();
        }

        private void FocusUnitTower()
        {
            _prevSize = _cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            _cameraManager.transform.DOMove(_curSelectedTower.transform.position, 0.5f).SetEase(Ease.InSine);
            _cam.DOOrthoSize(10, 0.5f).SetEase(Ease.InSine);
        }

        public void RewindCamState()
        {
            _cameraManager.transform.DOMove(_prevPos, 0.5f).SetEase(Ease.InSine);
            _cam.DOOrthoSize(_prevSize, 0.5f).SetEase(Ease.InSine);
        }

        private void MoveUnitButton()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);

            _curSelectedTower.TryGetComponent(out UnitTower unitTower);
            moveUnitIndicator.UnitTower = unitTower;
            moveUnitIndicator.distance = unitTower.MoveUnitRange;
            moveUnitIndicator.enabled = true;
            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = _curSelectedTower.transform.position + new Vector3(0, 0.1f, 0);
            indicatorTransform.localScale = new Vector3(unitTower.MoveUnitRange, 0.2f, unitTower.MoveUnitRange);
            if (!towerRangeIndicator.enabled) towerRangeIndicator.enabled = true;
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? "SellTower1" :
                _curSelectedTower.TowerLevel < 4 ? "SellTower2" : "SellTower3");

            ObjectPoolManager.Get(StringManager.BuildSmoke, _curSelectedTower.transform);
            Destroy(_curSelectedTower.gameObject);

            IncreaseGold(_sellTowerGold);
            ResetUI();
        }

        private void OffIndicator()
        {
            if (towerRangeIndicator.enabled) towerRangeIndicator.enabled = false;
            if (!moveUnitIndicator.enabled) return;

            moveUnitIndicator.enabled = false;
        }

        public void ResetUI()
        {
            if (!_isPanelOpen) return;
            _isTower = false;
            _curSelectedTower = null;
            offUIButton.gameObject.SetActive(false);
            towerInfoUI.enabled = false;
            _isPanelOpen = false;
            _towerSelectPanelTween.PlayBackwards();
            OffIndicator();
            selectedTowerIndicator.enabled = false;
        }

        public bool EnoughGold()
        {
            var enoughGold = _towerGold > towerBuildGold[_isTower ? _curSelectedTower.TowerLevel + 1 : 0];
            if (enoughGold) return true;
            NotEnoughGoldPopUp().Forget();
            return false;
        }

        private async UniTaskVoid NotEnoughGoldPopUp()
        {
            if (_callNotEnoughTween) return;
            _callNotEnoughTween = true;
            _notEnoughGoldTween.Restart();
            await UniTask.Delay(1000);
            _notEnoughGoldTween.PlayBackwards();
            _callNotEnoughTween = false;
        }

        #region HUD

        private void Pause()
        {
            IsPause = true;
            Time.timeScale = 0;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            IsPause = false;
            Time.timeScale = _curSpeed;
            _pauseSequence.PlayBackwards();
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

        public void DecreaseLifeCountEvent()
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