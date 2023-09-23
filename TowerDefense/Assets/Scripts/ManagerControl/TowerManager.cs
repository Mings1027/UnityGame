using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using PoolObjectControl;
using TMPro;
using TowerControl;
using UIControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public struct TowerInfo
    {
        public string towerName;
        public int level;
        public int upgradeGold;
        public int damage;
        public int range;
        public float delay;
        public int sellGold;
    }

    public class TowerManager : Singleton<TowerManager>
    {
        private TowerInfo _towerInfo;
        private Camera _cam;

        private List<Tower> _towers;
        private Dictionary<TowerType, TowerData> _towerDataDictionary;

        private Dictionary<TowerType, GameObject> _towerObjDictionary;

        private Dictionary<TowerType, int> _towerCountDictionary;

        private float[] _towerRanRotList;

        private Sequence _onOffTowerBtnSequence;
        private Tween _towerInfoPanelTween;
        private Sequence _pauseSequence;
        private Sequence _notEnoughGoldSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;

        //  Tower Buttons
        private Transform _toggleTowerBtnImage;
        private bool _isShowTowerBtn;

        //  Tower Panel
        private Tower _curSelectedTower;
        private UnitTower _curUnitTower;

        private bool _isPanelOpen;
        // private bool _isTowerSelected;
        private bool _isUnitTower;

        private int _sellTowerGold;
        private float _prevSize;
        private Vector3 _prevPos;

        // Damage Data
        private TMP_Text[] _damageTextList;

        private float _curTowerLife;
        private int _towerGold;
        private int _curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;

        private CameraManager _cameraManager;

        private int TowerGold
        {
            get => _towerGold;
            set
            {
                _towerGold = value;
                goldText.text = "Gold : " + _towerGold;
            }
        }

        public TextMeshProUGUI WaveText => waveText;
        public PlaceTowerController PlaceTowerController => placeTowerButtonController;

        public bool StartWave { get; private set; }
        [Header("----------Tower Buttons----------")] [SerializeField]
        private Button toggleTowerButton;

        [SerializeField] private Transform towerButtons;
        [SerializeField] private PlaceTowerController placeTowerButtonController;

        [FormerlySerializedAs("towerData")] [Header("----------Tower Panel----------"), SerializeField]
        private TowerData[] towerDataList;

        [SerializeField] private string[] towerTierName;

        [SerializeField] private TowerInfoUI towerInfoUI;

        [SerializeField] private Button checkTowerButton;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("------------Damage Data----------"), SerializeField]
        private GameObject damagePanel;

        [Header("------------HUD------------"), SerializeField]
        private GameObject hudPanel;

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

        [SerializeField] private Transform notEnoughGoldPanel;
        [SerializeField, Range(0, 30)] private int lifeCount;
        [SerializeField] private RectTransform playerHealthBar;
        [SerializeField] private Image lifeFillImage;
        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI speedUpText;

        [Header("----------Game Over----------"), SerializeField, Space(10)]
        private GameObject gameOverPanel;

        [SerializeField] private Button reStartButton;

        [Header("----------Indicator----------"), SerializeField]
        private MeshRenderer rangeIndicator;

        [SerializeField] private MeshRenderer selectedTowerIndicator;
        [SerializeField] private ParticleSystem unitDestinationParticle;

        [Header("----------For Unit Tower----------"), SerializeField]
        private GameObject checkUnitMoveButton;

        [SerializeField] private Image cantMoveImage;

        [SerializeField] private Ease camZoomEase;

        [SerializeField] private float camZoomTime;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            _cam = Camera.main;
            _cameraManager = CameraManager.Instance;
            lifeFillImage.fillAmount = 1;
            _curTowerLife = lifeCount;
            lifeCountText.text = _curTowerLife + " / " + lifeCount;
            TowerInit();
            TweenInit();
            TowerButtonInit();
            MenuButtonInit();
            DamageTextInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            checkTowerButton.gameObject.SetActive(true);
            IndicatorInit();
        }

        private void OnDestroy()
        {
            _towerInfoPanelTween.Kill();
            _notEnoughGoldSequence.Kill();
            _pauseSequence.Kill();
            _onOffTowerBtnSequence.Kill();
            _cantMoveImageSequence.Kill();
            _camZoomSequence.Kill();
        }

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        #region Init

        private void TowerInit()
        {
            _towers = new List<Tower>();
            _towerDataDictionary = new Dictionary<TowerType, TowerData>();
            foreach (var t in towerDataList)
            {
                _towerDataDictionary.Add(t.towerType, t);
            }

            _towerObjDictionary = new Dictionary<TowerType, GameObject>();
            foreach (var t in towerDataList)
            {
                _towerObjDictionary.Add(t.towerType, t.tower);
            }

            _towerCountDictionary = new Dictionary<TowerType, int>();
            foreach (var t in towerDataList)
            {
                _towerCountDictionary.Add(t.towerType, 0);
            }

            _towerRanRotList = new float[] { 0, 90, 180, 270 };
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = toggleTowerButton.transform.GetChild(0);
            _onOffTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(toggleTowerButton.transform.DOLocalMoveX(250, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .Join(_toggleTowerBtnImage.DORotate(new Vector3(0, 180, 0), 0.5f, RotateMode.LocalAxisAdd))
                .Join(towerButtons.DOLocalMoveX(0, 0.5f).From(-250).SetEase(Ease.InOutBack));

            _towerInfoPanelTween =
                towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetEase(Ease.OutBack).SetAutoKill(false);

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOLocalMoveY(0, 0.5f).From(Screen.height).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOLocalMoveY(200, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .PrependCallback(() => _cameraManager.enabled = !_cameraManager.enabled);

            _notEnoughGoldSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughGoldPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughGoldPanel.DOScale(0, 0.5f).SetDelay(0.3f).SetEase(Ease.InBounce));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
        }

        private void TowerButtonInit()
        {
            toggleTowerButton.onClick.AddListener(ToggleTowerButtons);

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                TowerUpgrade();
                UpdateTowerInfo();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                FocusUnitTower();
                MoveUnitButton();
            });
            checkUnitMoveButton.GetComponent<Button>().onClick.AddListener(CheckCanMoveUnit);
            checkUnitMoveButton.SetActive(false);
            sellTowerButton.GetComponent<Button>().onClick.AddListener(SellTower);
            checkTowerButton.onClick.AddListener(CheckTowerButton);
        }

        private void MenuButtonInit()
        {
            _towerGold = startGold;
            _curSpeed = 1;
            TowerGold = _towerGold;

            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            bgmButton.onClick.AddListener(BGMButton);
            gameEndButton.onClick.AddListener(GameEnd);
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            speedUpButton.onClick.AddListener(SpeedUp);
        }

        private void DamageTextInit()
        {
            damagePanel.SetActive(false);
            var damageTextPanel = damagePanel.transform.GetChild(0);
            _damageTextList = new TMP_Text[damageTextPanel.childCount];
            for (var i = 0; i < _damageTextList.Length; i++)
            {
                _damageTextList[i] = damageTextPanel.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
            }
        }

        private void GameOverPanelInit()
        {
            reStartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        private void IndicatorInit()
        {
            rangeIndicator.enabled = false;
            selectedTowerIndicator.enabled = false;
            unitDestinationParticle.Stop();
        }

        #endregion

        private void SetDamageText(Dictionary<TowerType, int> damageDic)
        {
            var damageArray = damageDic.Values.ToArray();
            for (var i = 0; i < damageArray.Length; i++)
            {
                _damageTextList[i].text = damageArray[i].ToString();
            }
        }

        public void PlaceUnitTower(ref TowerType towerType, Vector3 snappedPos, RaycastHit hit)
        {
            var unitTower = (UnitTower)Instantiate(_towerObjDictionary[towerType], snappedPos,
                    Quaternion.Euler(0, _towerRanRotList[Random.Range(0, _towerRanRotList.Length)], 0))
                .GetComponent<Tower>();
            unitTower.unitSpawnPosition = hit.point;
            _curSelectedTower = unitTower;

            BuildTower();
        }

        public void PlaceTower(ref TowerType towerType, Vector3 snappedPos)
        {
            _curSelectedTower = Instantiate(_towerObjDictionary[towerType], snappedPos,
                    Quaternion.Euler(0, _towerRanRotList[Random.Range(0, _towerRanRotList.Length)], 0))
                .GetComponent<Tower>();

            BuildTower();
        }

        public void GameStart()
        {
            hudPanel.SetActive(true);
            Time.timeScale = 1;
            _cam.DOOrthoSize(17, 1).From(100).SetEase(Ease.OutQuad);
        }

        private void GameOver()
        {
            gameOverPanel.SetActive(true);
            _cameraManager.enabled = false;
            Time.timeScale = 0;
            DataManager.SaveDamageData();
        }

        public void GameEnd()
        {
            DataManager.SaveDamageData();
            SetDamageText(DataManager.damageDic);
            damagePanel.SetActive(true);
            damagePanel.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void ToggleTowerButtons()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            if (!_isShowTowerBtn)
            {
                _isShowTowerBtn = true;
                InputManager.Instance.enabled = true;
                _onOffTowerBtnSequence.Restart();
            }
            else
            {
                _isShowTowerBtn = false;
                InputManager.Instance.enabled = false;
                _onOffTowerBtnSequence.PlayBackwards();
            }
        }

        private void CheckTowerButton()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit, Mathf.Infinity);

            if (hit.collider != null && hit.collider.TryGetComponent(out IFingerUp fingerUp))
            {
                fingerUp.FingerUp();
            }
            else
            {
                OffUI();
            }
        }

        private void ClickTower(Tower clickedTower)
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            _towerInfoPanelTween.Restart();
            // _isTowerSelected = true;
            _isPanelOpen = true;

            if (clickedTower == _curSelectedTower) return;
            if (_isUnitTower)
            {
                _curUnitTower.OffUnitIndicator();
            }

            _curSelectedTower = clickedTower;

            upgradeButton.SetActive(_curSelectedTower.TowerLevel != 4);

            OpenEditButtonPanel();
            UpdateTowerInfo();
            SetIndicator();
            IfUnitTower();
        }

        private void OpenEditButtonPanel()
        {
            towerInfoUI.enabled = true;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
            if (!towerInfoUI.gameObject.activeSelf) towerInfoUI.gameObject.SetActive(true);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var tempTowerData = _towerDataDictionary[towerType];
            var towerLevelData = tempTowerData.towerLevels[towerLevel];

            _towerInfo.towerName = towerTierName[towerLevel] + towerType;
            _towerInfo.level = towerLevel + 1;
            _towerInfo.upgradeGold = tempTowerData.towerUpgradeGold * (towerLevel + 1);
            _towerInfo.damage = towerLevelData.damage;
            _towerInfo.range = towerLevelData.attackRange;
            _towerInfo.delay = towerLevelData.attackDelay;
            _towerInfo.sellGold = _sellTowerGold = GetSellTowerGold(ref towerType);

            towerInfoUI.SetTowerInfo(_towerInfo);
        }

        private void SetIndicator()
        {
            var curTowerPos = _curSelectedTower.transform.position;
            selectedTowerIndicator.transform.position = curTowerPos;
            selectedTowerIndicator.enabled = true;

            var r = rangeIndicator.transform;
            r.DOScale(new Vector3(_curSelectedTower.TowerRange, 0.5f, _curSelectedTower.TowerRange), 0.15f)
                .SetEase(Ease.OutBack);
            r.position = curTowerPos;
            rangeIndicator.enabled = true;
        }

        private void IfUnitTower()
        {
            _isUnitTower = _curSelectedTower.TryGetComponent(out UnitTower unitTower);
            moveUnitButton.SetActive(_isUnitTower);
            if (!_isUnitTower) return;
            _curUnitTower = unitTower;
        }

        private void BuildTower()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerType;
            var towerLevel = tempTower.TowerLevel;
#if UNITY_EDITOR
            tempTower.transform.SetParent(transform);
#endif
            _towerCountDictionary[towerType]++;
            var t = _towerDataDictionary[towerType];

            tempTower.TowerLevelUp();
            var tt = t.towerLevels[tempTower.TowerLevel];
            TowerGold -= _towerDataDictionary[towerType].towerBuildGold * _towerCountDictionary[towerType];

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(towerLevel != 4);

            tempTower.isSpawn = true;
            _curSelectedTower.OnClickTower += ClickTower;
            _towers.Add(_curSelectedTower);
            _curSelectedTower = null;
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerName = tempTower.TowerType;
#if UNITY_EDITOR
            tempTower.transform.SetParent(transform);
#endif
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);

            var towerData = _towerDataDictionary[towerName];

            if (_towerGold < towerData.towerUpgradeGold * (_curSelectedTower.TowerLevel + 1))
            {
                _notEnoughGoldSequence.Restart();
                return;
            }

            tempTower.TowerLevelUp();
            var towerLevel = tempTower.TowerLevel;
            var tt = towerData.towerLevels[towerLevel];
            TowerGold -= _towerDataDictionary[towerName].towerUpgradeGold * towerLevel;

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(towerLevel != 4);

            SetIndicator();
        }

        private void FocusUnitTower()
        {
            _prevSize = _cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            DOTween.Sequence()
                .Append(_cameraManager.transform.DOMove(_curSelectedTower.transform.position, camZoomTime)
                    .SetEase(camZoomEase))
                .Join(_cam.DOOrthoSize(10, camZoomTime).SetEase(camZoomEase));
        }

        private void RewindCamState()
        {
            DOTween.Sequence()
                .Append(_cameraManager.transform.DOMove(_prevPos, camZoomTime).SetEase(camZoomEase))
                .Join(_cam.DOOrthoSize(_prevSize, camZoomTime).SetEase(camZoomEase));
        }

        private void MoveUnitButton()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            moveUnitButton.SetActive(false);
            towerInfoUI.gameObject.SetActive(false);
            _curUnitTower = _curSelectedTower.GetComponent<UnitTower>();
            checkUnitMoveButton.SetActive(true);
        }

        private void SellTower()
        {
            SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? "SellTower1" :
                _curSelectedTower.TowerLevel < 4 ? "SellTower2" : "SellTower3");
            _towerCountDictionary[_curSelectedTower.TowerType]--;
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, _curSelectedTower.transform.position);

            TowerGold += _sellTowerGold;
            _towers.Remove(_curSelectedTower);
            Destroy(_curSelectedTower.gameObject);
            OffUI();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _towerInfoPanelTween.PlayBackwards();

            // _isTowerSelected = false;
            _isUnitTower = false;
            _isPanelOpen = false;
            _curSelectedTower = null;
            towerInfoUI.enabled = false;

            if (rangeIndicator.enabled)
            {
                rangeIndicator.enabled = false;
            }

            if (selectedTowerIndicator.enabled)
            {
                selectedTowerIndicator.enabled = false;
            }

            if (_curUnitTower == null) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool IsEnoughGold(ref TowerType towerType)
        {
            if (_towerGold >= _towerDataDictionary[towerType].towerBuildGold * (_towerCountDictionary[towerType] + 1))
            {
                return true;
            }

            _notEnoughGoldSequence.Restart();
            return false;
        }

        #region For UnitTower

        private void CheckCanMoveUnit()
        {
            var touch = Input.GetTouch(0);
            if (touch.deltaPosition != Vector2.zero) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider != null && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_curUnitTower.transform.position, hit.point) <=
                _curUnitTower.TowerRange)
            {
                StartMoveUnit(new Vector3(hit.point.x, 0, hit.point.z)).Forget();
            }
            else
            {
                cantMoveImage.transform.position = Input.mousePosition;
                _cantMoveImageSequence.Restart();
            }
        }

        private async UniTaskVoid StartMoveUnit(Vector3 pos)
        {
            unitDestinationParticle.transform.position = pos;
            unitDestinationParticle.Play();
            checkUnitMoveButton.SetActive(false);
            RewindCamState();

            await _curUnitTower.StartUnitMove(pos);
            OffUI();

            unitDestinationParticle.Stop();
        }

        #endregion

        #region HUD

        private void Pause()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            Time.timeScale = 0;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            Time.timeScale = _curSpeed;
            _pauseSequence.PlayBackwards();
        }

        private void BGMButton()
        {
            bgmButton.image.sprite = SoundManager.Instance.BGMToggle() ? musicOnImage : musicOffImage;
        }

        private void SpeedUp()
        {
            SoundManager.Instance.PlaySound(StringManager.ButtonSound);
            _curSpeed = _curSpeed % 3 + 1;
            Time.timeScale = _curSpeed;
            speedUpText.text = $"x{_curSpeed}";
        }

        private int GetSellTowerGold(ref TowerType towerType)
        {
            return _towerDataDictionary[towerType].towerBuildGold +
                   _towerDataDictionary[towerType].towerUpgradeGold * _curSelectedTower.TowerLevel;
        }

        public void GetGoldFromEnemy(int amount)
        {
            TowerGold += amount;
        }

        public void DecreaseLifeCountEvent()
        {
            if (_curTowerLife == 0) return;
            _curTowerLife -= 1;
            playerHealthBar.DOShakePosition(0.2f, 15f, 4);
            lifeFillImage.fillAmount = _curTowerLife / lifeCount;
            lifeCountText.text = _curTowerLife + " / " + lifeCount;
            if (_curTowerLife > 0) return;
            GameOver();
        }

        #endregion

        public void EnableTower()
        {
            StartWave = true;
            for (var i = 0; i < _towers.Count; i++)
            {
                _towers[i].enabled = true;
            }
        }

        public void DisableTower()
        {
            StartWave = false;
            for (var i = 0; i < _towers.Count; i++)
            {
                _towers[i].enabled = false;
            }

            PoolObjectManager.Instance.PoolCleaner();
        }
    }
}