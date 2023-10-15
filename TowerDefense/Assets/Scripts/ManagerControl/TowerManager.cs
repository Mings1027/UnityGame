using System.Collections.Generic;
using System.Linq;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using PoolObjectControl;
using StatusControl;
using TMPro;
using TowerControl;
using UIControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public struct TowerInfo
    {
        public string towerName;
        public sbyte level;
        public ushort upgradeGold;
        public ushort damage;
        public byte range;
        public float delay;
        public int sellGold;
    }

    public class TowerManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private TowerInfo _towerInfo;
        private Camera _cam;

        private Dictionary<TowerType, TowerData> _towerDataDictionary;
        private Dictionary<TowerType, GameObject> _towerObjDictionary;
        private Dictionary<TowerType, int> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerCostDictionary;

        private Vector3[] fourDirection;

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
        private bool _isUnitTower;

        private int _sellTowerGold;
        private byte _prevSize;
        private Vector3 _prevPos;

        // Damage Data
        private TMP_Text[] _damageTextList;

        private int _towerGold;
        private byte _curSpeed;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;

        public int TowerGold
        {
            get => _towerGold;
            set
            {
                _towerGold = value;
                goldText.text = "Gold : " + _towerGold;
            }
        }

        public TextMeshProUGUI WaveText => waveText;

        [SerializeField] private TowerController towerController;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private Button toggleTowerButton;

        [SerializeField] private Transform towerButtons;

        [Header("----------Tower Panel----------"), SerializeField]
        private TowerData[] towerDataList;

        [SerializeField] private string[] towerTierName;

        [SerializeField] private TowerInfoUI towerInfoUI;

        [SerializeField] private Button checkTowerButton;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("------------Damage Data----------"), SerializeField]
        private GameObject damagePanel;

        [Header("------------UI------------"), SerializeField]
        private GameObject uiPanel;

        [SerializeField] private int startGold;

        [SerializeField] private Transform pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button bgmButton;
        [SerializeField] private Button sfxButton;

        [SerializeField] private Button gameEndButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Sprite musicOnImage;
        [SerializeField] private Sprite musicOffImage;

        [SerializeField] private Transform notEnoughGoldPanel;
        [SerializeField, Range(0, 30)] private byte lifeCount;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI speedUpText;

        [SerializeField] private TMP_Text[] towerCostTexts;

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
        [SerializeField, Range(0, 1)] private float camZoomTime;

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _cam = Camera.main;

            var baseTowerHealth = GetComponentInChildren<BaseTowerHealth>();
            baseTowerHealth.Init(lifeCount);
            baseTowerHealth.OnDeadEvent += GameOver;

            TowerInit();
            TweenInit();
            TowerButtonInit();
            MenuButtonInit();
            DamageTextInit();
            GameOverPanelInit();
        }

        private void Start()
        {
            uiPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            checkTowerButton.gameObject.SetActive(true);
            IndicatorInit();
        }

        private void OnDestroy()
        {
            _towerInfoPanelTween?.Kill();
            _notEnoughGoldSequence?.Kill();
            _pauseSequence?.Kill();
            _onOffTowerBtnSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
        }

        /*=================================================================================================================
    *                                                  Unity Event                                                             *
    //================================================================================================================*/

        #region Init

        private void TowerInit()
        {
            _towerDataDictionary = new Dictionary<TowerType, TowerData>();
            _towerObjDictionary = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, int>();
            _towerCostDictionary = new Dictionary<TowerType, TMP_Text>();

            foreach (var t in towerDataList)
            {
                _towerDataDictionary.Add(t.TowerType, t);
                _towerObjDictionary.Add(t.TowerType, t.Tower);
                _towerCountDictionary.Add(t.TowerType, 0);
            }

            for (var i = 0; i < towerDataList.Length; i++)
            {
                _towerCostDictionary.Add(towerDataList[i].TowerType, towerCostTexts[i]);
                SetTowerCost(towerDataList[i].TowerType,
                    _towerDataDictionary[towerDataList[i].TowerType].TowerBuildGold);
            }

            fourDirection = new[] { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
            for (var i = 0; i < fourDirection.Length; i++)
            {
                fourDirection[i] *= 2;
            }
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = toggleTowerButton.transform.GetChild(0);
            // _onOffTowerBtnSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            //     .Append(toggleTowerButton.transform.DOLocalMoveX(250, 0.5f).SetRelative().SetEase(Ease.InOutBack))
            //     .Join(_toggleTowerBtnImage.DORotate(new Vector3(0, 180, 0), 0.5f, RotateMode.LocalAxisAdd))
            //     .Join(towerButtons.DOLocalMoveX(0, 0.5f).From(-250).SetEase(Ease.InOutBack));

            _towerInfoPanelTween =
                towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOLocalMoveY(0, 0.5f).From(Screen.height).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOLocalMoveY(200, 0.5f).SetRelative().SetEase(Ease.InOutBack))
                .PrependCallback(() => _gameManager.cameraManager.enabled = !_gameManager.cameraManager.enabled);

            _notEnoughGoldSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughGoldPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughGoldPanel.DOScale(0, 0.5f).SetDelay(0.3f).SetEase(Ease.InBounce));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
        }

        private void TowerButtonInit()
        {
            toggleTowerButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                ToggleTowerButtons();
            });

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                TowerUpgrade();
                UpdateTowerInfo();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                FocusUnitTower();
                MoveUnitButton();
            });
            checkUnitMoveButton.GetComponent<Button>().onClick.AddListener(CheckCanMoveUnit);
            checkUnitMoveButton.SetActive(false);
            sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(_curSelectedTower.TowerLevel < 2 ? SoundEnum.SellTower1 :
                    _curSelectedTower.TowerLevel < 4 ? SoundEnum.SellTower2 : SoundEnum.SellTower3);

                SellTower();
            });
            checkTowerButton.onClick.AddListener(CheckTowerButton);
        }

        private void MenuButtonInit()
        {
            _towerGold = startGold;
            _curSpeed = 1;
            TowerGold = _towerGold;

            speedUpText.text = "x1";

            pauseButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = 0;
                _pauseSequence.Restart();
            });
            resumeButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = _curSpeed;
                _pauseSequence.PlayBackwards();
            });
            bgmButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.ToggleBGM();
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
            });
            sfxButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.ToggleSfx();
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
            });
            gameEndButton.onClick.AddListener(GameEnd);
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            speedUpButton.onClick.AddListener(() =>
            {
                _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
                SpeedUp();
            });
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
            reStartButton.onClick.AddListener(() =>
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex));
        }

        private void IndicatorInit()
        {
            rangeIndicator.transform.localScale = Vector3.zero;
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

        public async UniTaskVoid InstantiateTower(TowerType towerType, Vector3 placePos, bool isUnitTower)
        {
            Vector3 towerForward = default;
            var foundGround = false;
            for (var i = 0; i < fourDirection.Length; i++)
            {
                var ray = new Ray(placePos + fourDirection[i] + Vector3.up, Vector3.down);
                Physics.Raycast(ray, out var hit, 2);
                if (hit.collider == null || !hit.collider.CompareTag("Ground")) continue;
                foundGround = true;
                towerForward = -fourDirection[i];
                break;
            }

            if (!foundGround)
            {
                towerForward = fourDirection[Random.Range(0, fourDirection.Length)];
            }

            Instantiate(_towerObjDictionary[towerType], placePos + Vector3.up * 2, Quaternion.identity)
                .TryGetComponent(out Tower t);
            t.transform.forward = towerForward;
            if (isUnitTower)
            {
                SetUnitPosition(t);
            }

            var towerData = _towerDataDictionary[towerType];
            BuildTower(t, towerData);
            DOTween.Sequence().Append(goldText.transform.DOScale(1.5f, 0.15f))
                .Join(goldText.transform.DOShakeRotation(0.2f, 50, 10, 90, true, ShakeRandomnessMode.Harmonic))
                .Append(goldText.transform.DOScale(1, 0.25f));

            await DOTween.Sequence().Append(t.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(t.transform.DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
                    PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos)))
                .WithCancellation(this.GetCancellationTokenOnDestroy());

            TowerSetting(t, towerData);
            towerController.AddTower(t);
        }

        private void SetUnitPosition(Tower t)
        {
            NavMesh.SamplePosition(t.transform.position, out var hit, 5, NavMesh.AllAreas);
            t.TryGetComponent(out UnitTower u);
            u.unitSpawnPosition = hit.position;
        }

        private void BuildTower(Tower t, TowerData towerData)
        {
            var towerType = t.TowerData.TowerType;
#if UNITY_EDITOR
            t.transform.SetParent(transform);
#endif
            _towerCountDictionary[towerType]++;

            t.TowerLevelUp();
            TowerGold -= towerData.TowerBuildGold * _towerCountDictionary[towerType];
            _towerCostDictionary[towerType].text =
                towerData.TowerBuildGold * (_towerCountDictionary[towerType] + 1) + "g";
        }

        private void TowerSetting(Tower t, TowerData towerData)
        {
            var towerInfoData = towerData.TowerLevels[t.TowerLevel];
            t.TowerSetting(towerInfoData.towerMesh, towerInfoData.damage, towerInfoData.attackRange,
                towerInfoData.attackDelay);
            upgradeButton.SetActive(!t.TowerLevel.Equals(4));

            t.OnClickTower += ClickTower;
        }

        private void SetTowerCost(TowerType towerType, int cost)
        {
            _towerCostDictionary[towerType].text = cost + "g";
        }

        public void GameStart()
        {
            uiPanel.SetActive(true);
            Time.timeScale = 1;
            DOTween.Sequence()
                .Append(_cam.transform.DOLocalMove(new Vector3(0, 50, -50), 1.5f).SetEase(Ease.OutQuad)
                    .From(new Vector3(0, 300, -300)))
                .Join(_cam.DOOrthoSize(17, 1.5f).From(100).SetEase(Ease.OutQuad));
        }

        public void DamageInit() => DataManager.InitDamage();

        private void GameOver()
        {
            gameOverPanel.SetActive(true);
            _gameManager.cameraManager.enabled = false;
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
            if (!_isShowTowerBtn)
            {
                _isShowTowerBtn = true;
                _gameManager.inputManager.enabled = true;
                _onOffTowerBtnSequence.Restart();
            }
            else
            {
                _isShowTowerBtn = false;
                _gameManager.inputManager.enabled = false;
                _onOffTowerBtnSequence.PlayBackwards();
            }
        }

        private void CheckTowerButton()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit, Mathf.Infinity);
            if (Input.GetTouch(0).deltaPosition != Vector2.zero) return;
            if (hit.collider && hit.collider.TryGetComponent(out IFingerUp fingerUp))
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
            _gameManager.soundManager.PlaySound(SoundEnum.ButtonSound);
            _towerInfoPanelTween.Restart();
            _isPanelOpen = true;

            if (clickedTower.Equals(_curSelectedTower)) return;
            if (_isUnitTower)
            {
                _curUnitTower.OffUnitIndicator();
            }

            _curSelectedTower = clickedTower;

            upgradeButton.SetActive(!clickedTower.TowerLevel.Equals(4));

            IfUnitTower();
            OpenEditButtonPanel();
            UpdateTowerInfo();
            SetIndicator();
        }

        private void IfUnitTower()
        {
            _isUnitTower = _curSelectedTower.TryGetComponent(out UnitTower unitTower);
            moveUnitButton.SetActive(_isUnitTower);
            if (!_isUnitTower) return;
            _curUnitTower = unitTower;
        }

        private void OpenEditButtonPanel()
        {
            towerInfoUI.enabled = true;
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerData.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var towerData = _towerDataDictionary[towerType];
            var towerLevelData = towerData.TowerLevels[towerLevel];

            _towerInfo.towerName = towerTierName[towerLevel] + towerType;
            _towerInfo.level = (sbyte)(towerLevel + 1);
            _towerInfo.upgradeGold = (ushort)(towerData.TowerUpgradeGold * (towerLevel + 1));
            _towerInfo.damage = towerLevelData.damage;
            _towerInfo.range = towerLevelData.attackRange;
            _towerInfo.delay = towerLevelData.attackDelay;
            _towerInfo.sellGold = _sellTowerGold = GetSellTowerGold(towerType);

            towerInfoUI.SetTowerInfo(in _towerInfo);
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
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerData.TowerType;
#if UNITY_EDITOR
            tempTower.transform.SetParent(transform);
#endif
            var towerData = _towerDataDictionary[towerType];

            if (_towerGold < towerData.TowerUpgradeGold * (_curSelectedTower.TowerLevel + 1))
            {
                _notEnoughGoldSequence.Restart();
                return;
            }

            tempTower.TowerLevelUp();
            var towerLevel = tempTower.TowerLevel;
            var tt = towerData.TowerLevels[towerLevel];
            TowerGold -= _towerDataDictionary[towerType].TowerUpgradeGold * towerLevel;

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, tempTower.transform.position);

            tempTower.TowerSetting(tt.towerMesh, tt.damage, tt.attackRange, tt.attackDelay);
            upgradeButton.SetActive(!towerLevel.Equals(4));

            SetIndicator();
        }

        private void FocusUnitTower()
        {
            _prevSize = (byte)_cam.orthographicSize;
            _prevPos = _gameManager.cameraManager.transform.position;
            DOTween.Sequence()
                .Append(_gameManager.cameraManager.transform.DOMove(_curSelectedTower.transform.position, camZoomTime)
                    .SetEase(camZoomEase))
                .Join(_cam.DOOrthoSize(10, camZoomTime).SetEase(camZoomEase));
        }

        private void RewindCamState()
        {
            DOTween.Sequence()
                .Append(_gameManager.cameraManager.transform.DOMove(_prevPos, camZoomTime).SetEase(camZoomEase))
                .Join(_cam.DOOrthoSize(_prevSize, camZoomTime).SetEase(camZoomEase));
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            checkUnitMoveButton.SetActive(true);
            _towerInfoPanelTween.PlayBackwards();
        }

        private void SellTower()
        {
            _towerCountDictionary[_curSelectedTower.TowerData.TowerType]--;
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, _curSelectedTower.transform.position);

            TowerGold += _sellTowerGold;
            towerController.RemoveTower(_curSelectedTower);
            Destroy(_curSelectedTower.gameObject);
            OffUI();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _towerInfoPanelTween.PlayBackwards();

            _isUnitTower = false;
            _isPanelOpen = false;
            _curSelectedTower = null;
            towerInfoUI.enabled = false;
            rangeIndicator.transform.DOScale(0, 0.2f).SetEase(Ease.InBack);

            if (selectedTowerIndicator.enabled)
            {
                selectedTowerIndicator.enabled = false;
            }

            if (!_curUnitTower) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool IsEnoughGold(in TowerType towerType)
        {
            if (_towerGold >= _towerDataDictionary[towerType].TowerBuildGold * (_towerCountDictionary[towerType] + 1))
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
            if (!touch.deltaPosition.Equals(Vector2.zero)) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_curUnitTower.transform.position, hit.point) <= _curUnitTower.TowerRange)
            {
                _isUnitTower = false;
                checkUnitMoveButton.SetActive(false);
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
            RewindCamState();

            await _curUnitTower.StartUnitMove(pos);
            if (!_isUnitTower)
            {
                OffUI();
            }

            unitDestinationParticle.Stop();
        }

        #endregion

        #region UI

        private void SpeedUp()
        {
            _curSpeed = (byte)(_curSpeed % 3 + 1);
            Time.timeScale = _curSpeed;
            speedUpText.text = $"x{_curSpeed}";
        }

        private ushort GetSellTowerGold(TowerType towerType)
        {
            return (ushort)(_towerDataDictionary[towerType].TowerBuildGold +
                            _towerDataDictionary[towerType].TowerUpgradeGold * _curSelectedTower.TowerLevel);
        }

        #endregion

        public void EnableTower()
        {
            towerController.enabled = true;

            Application.targetFrameRate = 60;
        }

        public void DisableTower()
        {
            towerController.TargetInit();
            towerController.enabled = false;

            Application.targetFrameRate = 30;
            PoolObjectManager.PoolCleaner().Forget();
        }
    }
}