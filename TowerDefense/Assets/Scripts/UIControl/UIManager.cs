using System;
using System.Collections.Generic;
using System.Linq;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using MapControl;
using PoolObjectControl;
using StatusControl;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : Singleton<UIManager>, IPointerDownHandler, IPointerUpHandler
    {
        private TowerManager _towerManager;
        private CameraManager _cameraManager;
        private TowerButtonController _towerButtonController;
        private Camera _cam;
        private Dictionary<TowerType, TowerData> _towerDataDictionary;
        private Dictionary<TowerType, GameObject> _towerObjDictionary;
        private Dictionary<TowerType, int> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerCostTextDictionary;

        // private Sequence _towerCostSequence;
        private Tween _upgradeSellPanelTween;

        // private Tweener _onOffTowerBtnSequence;
        private Sequence _pauseSequence;
        private Sequence _notEnoughCostSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;
        private Tween _towerInfoWindowTween;
        private Tweener _rangeIndicatorTween;

        private Tower _curSelectedTower;
        private UnitTower _curUnitTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _damageTextList;
        private TMP_Text[] _towerCostTexts;
        private TMP_Text _curSpeedText;

        private int _sellTowerCost;
        private int _towerCost;
        private byte _curTimeScale;
        private bool _isShowTowerBtn;
        private bool _isSpeedUp;
        private bool _callNotEnoughTween;
        private bool _isPanelOpen;
        private bool _openInfoWindow;

        private Vector3 _prevPos;

        public int TowerCost
        {
            get => _towerCost;
            set
            {
                _towerCost = value;
                costText.text = CachedNumber.GetUIText(_towerCost);
            }
        }

        public static bool IsOnUI { get; set; }
        public TextMeshProUGUI WaveText => waveText;
        public Health BaseTowerHealth { get; private set; }
        public Dictionary<TowerType, string> towerNameDic { get; private set; }
        public Dictionary<TowerType, string> towerInfoDic { get; private set; }

        [SerializeField] private HealthBar healthBar;
        [SerializeField] private byte lifeCount;
        [SerializeField] private int startCost;

        [SerializeField] private Transform notEnoughCostPanel;
        [SerializeField] private RectTransform towerPanel;
        [SerializeField] private Image cantMoveImage;
        [SerializeField] private Image checkMoveUnitPanel;
        [SerializeField] private Sprite physicalSprite;
        [SerializeField] private Sprite magicSprite;
        [SerializeField] private Sprite rpmSprite;
        [SerializeField] private Sprite reSpawnSprite;

        [Header("----------Tower Panel----------"), SerializeField]
        private RectTransform infoWindow;

        [SerializeField] private TowerData[] towerDataList;

        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject moveUnitButton;
        [SerializeField] private GameObject sellTowerButton;

        [Header("----------Tower Buttons----------")] [SerializeField]
        private GameObject toggleTowerButton;

        [Header("----------Game Over-------------"), SerializeField]
        private GameObject gameOverPanel;

        [SerializeField] private GameObject gameEndPanel;

        [SerializeField] private Transform pausePanel;
        [SerializeField] private Image pausePanelBlockImage;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button centerButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button speedUpButton;

        [SerializeField] private Toggle bgmToggle;
        [SerializeField] private Toggle sfxToggle;

        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI waveText;

        [SerializeField] private MeshRenderer rangeIndicator;

        #region Unity Event

        protected override void Awake()
        {
            base.Awake();
            BaseTowerHealth = GetComponent<Health>();
            _cam = Camera.main;

            TowerButtonInit();
            TowerInit();
            LocaleDictionaryInit();
            TweenInit();
            MenuButtonInit();
            rangeIndicator.transform.localScale = Vector3.zero;

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleDictionary;
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _towerManager = FindObjectOfType<TowerManager>();
            gameOverPanel.SetActive(false);
            gameEndPanel.SetActive(false);
            healthBar.Init(BaseTowerHealth);
            BaseTowerHealth.Init(lifeCount);
            BaseTowerHealth.OnDeadEvent += GameOver;
            GameStart().Forget();
        }

        private void OnDisable()
        {
            // _towerCostSequence?.Kill();
            _upgradeSellPanelTween?.Kill();
            _notEnoughCostSequence?.Kill();
            _pauseSequence?.Kill();
            // _onOffTowerBtnSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _towerInfoWindowTween?.Kill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsOnUI = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsOnUI = false;
        }

        #endregion

        #region Init

        private void TowerInit()
        {
            _towerDataDictionary = new Dictionary<TowerType, TowerData>();
            _towerObjDictionary = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, int>();
            _towerCostTextDictionary = new Dictionary<TowerType, TMP_Text>();

            foreach (var t in towerDataList)
            {
                t.InitState();
                _towerDataDictionary.Add(t.TowerType, t);
                _towerObjDictionary.Add(t.TowerType, t.Tower);
                _towerCountDictionary.Add(t.TowerType, 0);
            }

            for (var i = 0; i < towerDataList.Length; i++)
            {
                var towerType = towerDataList[i].TowerType;
                _towerCostTextDictionary.Add(towerType, _towerCostTexts[i]);
                _towerCostTextDictionary[towerType].text = _towerDataDictionary[towerType].TowerBuildCost + "g";
            }
        }

        private void LocaleDictionaryInit()
        {
            towerNameDic = new Dictionary<TowerType, string>();
            towerInfoDic = new Dictionary<TowerType, string>();
            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) return;
                towerNameDic.Add(towerType, LocaleManager.GetLocalizedString(towerType.ToString()));
                towerInfoDic.Add(towerType, LocaleManager.GetLocalizedString(LocaleManager.CardKey + towerType));
            }
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = toggleTowerButton.transform.GetChild(0).GetComponent<Image>();

            // _towerCostSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            //     .Append(costText.transform.DOShakeScale(0.2f, randomnessMode: ShakeRandomnessMode.Full))
            //     .Join(costText.transform.DOShakeRotation(0.2f, randomnessMode: ShakeRandomnessMode.Full));

            _upgradeSellPanelTween = towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetAutoKill(false).Pause();

            // _onOffTowerBtnSequence = towerPanel.DOAnchorPosY(-250, 0.25f).From().SetAutoKill(false).Pause();

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOScale(0, 0.5f));

            _notEnoughCostSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughCostPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughCostPanel.DOScale(0, 0.5f).SetDelay(0.3f));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                    .SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));

            _towerInfoWindowTween = infoWindow.DOAnchorPosY(200, 0.3f).From()
                .OnComplete(() => _openInfoWindow = true).SetAutoKill(false).Pause();

            _rangeIndicatorTween = rangeIndicator.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
        }

        private void TowerButtonInit()
        {
            _towerButtonController = towerPanel.GetComponentInChildren<TowerButtonController>();
            _towerButtonController.Init();
            for (var i = 0; i < towerDataList.Length; i++)
            {
                _towerButtonController.SetDictionary(i, towerDataList[i]);
            }

            var towerButtons = _towerButtonController.GetComponentsInChildren<TowerButton>();
            _towerCostTexts = new TMP_Text[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerCostTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }

            toggleTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                ToggleTowerButtons();
            });

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!_isPanelOpen) return;
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                TowerUpgrade();
                UpdateTowerInfo();
                upgradeButton.transform.DOScale(1.1f, 0.1f).From(1).SetLoops(2, LoopType.Yoyo);
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                checkMoveUnitPanel.enabled = true;
                _towerManager.FocusUnitTower(_curUnitTower);
                MoveUnitButton();
            });
            checkMoveUnitPanel.enabled = false;

            sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!_isPanelOpen) return;
                SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? SoundEnum.LowCost :
                    _curSelectedTower.TowerLevel < 4 ? SoundEnum.MediumCost : SoundEnum.HighCost);

                SellTower();
            });
        }

        private void MenuButtonInit()
        {
            _towerCost = startCost;
            _curTimeScale = 1;
            TowerCost = _towerCost;

            pauseButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = 0;
                pausePanelBlockImage.raycastTarget = false;
                _pauseSequence.Restart();
            });

            centerButton.onClick.AddListener(() =>
            {
                if (_cameraManager.transform.position == Vector3.zero) return;
                _cameraManager.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
            });
            resumeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Time.timeScale = _curTimeScale;
                pausePanelBlockImage.raycastTarget = true;
                _pauseSequence.PlayBackwards();
            });
            bgmToggle.isOn = false;
            bgmToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.ToggleBGM(!bgmToggle.isOn);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });
            sfxToggle.isOn = false;
            sfxToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.ToggleSfx(!sfxToggle.isOn);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });

            mainMenuButton.onClick.AddListener(delegate
            {
                SceneManager.LoadScene("Lobby");
                DataManager.SaveLastSurvivedWave();
            });

            speedUpButton.onClick.AddListener(() =>
            {
                speedUpButton.transform.DOScale(1, 0.2f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                SpeedUp();
            });
            _curSpeedText = speedUpButton.GetComponentInChildren<TMP_Text>();
        }

        private void ChangeLocaleDictionary(Locale locale)
        {
            foreach (var towerType in towerNameDic.Keys.ToList())
            {
                towerNameDic[towerType] = LocaleManager.GetLocalizedString(towerType.ToString());
            }

            foreach (var towerType in towerInfoDic.Keys.ToList())
            {
                towerInfoDic[towerType] = LocaleManager.GetLocalizedString(LocaleManager.CardKey + towerType);
            }
        }

        #endregion

        public void UpgradeTowerData()
        {
            for (var i = 0; i < towerDataList.Length; i++)
            {
                towerDataList[i].UpgradeData();
            }
        }

        public async UniTaskVoid MapSelectButton(int index)
        {
            FindObjectOfType<MapManager>().MakeMap(index);
            DataManager.SetLevel((byte)index);
            GetComponent<TutorialController>().TutorialButton();

            await transform.GetChild(0).DOScale(0, 0.5f).SetEase(Ease.InBack);
            _towerInfoWindowTween.Restart();
            await towerPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            _cameraManager.enabled = true;
        }

        public Sprite GetTowerType(TowerData t) => t.IsMagicTower ? magicSprite : physicalSprite;
        public Sprite IsUnitTower(TowerData t) => t.IsUnitTower ? reSpawnSprite : rpmSprite;

        public void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var t = Instantiate(_towerObjDictionary[towerType], placePos, Quaternion.identity).GetComponent<Tower>();
            var towerTransform = t.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;

            var towerData = _towerDataDictionary[towerType];
            BuildTower(t, placePos, towerData).Forget();
        }

        public void UIOff()
        {
            OffUI();
            if (!_isShowTowerBtn)
            {
                toggleTowerButton.SetActive(true);
                _toggleTowerBtnImage.DOFade(1, 0.2f);
            }

            if (_openInfoWindow) return;
            _openInfoWindow = true;
            _towerInfoWindowTween.Restart();
        }

        public void SliderUp()
        {
            if (Input.touchCount != 1) return;
            var touch = Input.touches[0];
            if (touch.deltaPosition.y > 15)
            {
                _openInfoWindow = false;
                _towerInfoWindowTween.PlayBackwards();
            }
        }

        public void SlideDown()
        {
            _isShowTowerBtn = false;
            // _onOffTowerBtnSequence.OnRewind(() => toggleTowerButton.SetActive(true)).PlayBackwards();
            toggleTowerButton.SetActive(true);
            ToggleTowerButtonAsync().Forget();
        }

        public void YouCannotMove()
        {
            cantMoveImage.transform.position = Input.mousePosition;
            _cantMoveImageSequence.Restart();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _upgradeSellPanelTween.PlayBackwards();

            _isPanelOpen = false;
            if (_curSelectedTower) _curSelectedTower.Outline.enabled = false;
            _curSelectedTower = null;
            _rangeIndicatorTween.ChangeEndValue(Vector3.zero).Restart();

            if (checkMoveUnitPanel.enabled) checkMoveUnitPanel.enabled = false;
            if (!_curUnitTower) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool IsEnoughCost(TowerType towerType)
        {
            if (_towerCost >= _towerDataDictionary[towerType].TowerBuildCost * (_towerCountDictionary[towerType] + 1))
            {
                return true;
            }

            _notEnoughCostSequence.Restart();
            return false;
        }

        public void GameEnd()
        {
            // resumeButton.interactable = false;
            gameEndPanel.SetActive(true);
            _pauseSequence.Restart();
            // Time.timeScale = 0;
            DataManager.SaveLastSurvivedWave();
        }

        private void ToggleTowerButtons()
        {
            IsOnUI = false;
            if (_isShowTowerBtn) return;
            toggleTowerButton.SetActive(false);
            _isShowTowerBtn = true;
            _towerButtonController.SlideUp();
            // _onOffTowerBtnSequence.Restart();
        }

        private void SetIndicator()
        {
            var curTowerPos = _curSelectedTower.transform.position;
            rangeIndicator.transform.position = curTowerPos;
            _rangeIndicatorTween
                .ChangeStartValue(rangeIndicator.transform.localScale)
                .ChangeEndValue(new Vector3(_curSelectedTower.TowerRange, 0.5f,
                    _curSelectedTower.TowerRange)).Restart();
        }

        private async UniTaskVoid BuildTower(Tower t, Vector3 placePos, TowerData towerData)
        {
            var towerType = t.TowerData.TowerType;

            _towerCountDictionary[towerType]++;

            TowerCost -= towerData.TowerBuildCost * _towerCountDictionary[towerType];
            _towerCostTextDictionary[towerType].text =
                towerData.TowerBuildCost * (_towerCountDictionary[towerType] + 1) + "g";
            await DOTween.Sequence().Join(t.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    })).WithCancellation(this.GetCancellationTokenOnDestroy());

            t.TowerLevelUp();
            t.TowerSetting(towerData.TowerMeshes[t.TowerLevel], towerData.BaseDamage, towerData.AttackRange,
                towerData.AttackRpm);
            t.OnClickTower += ClickTower;

            _towerManager.AddTower(t);
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerData.TowerType;
            var towerData = _towerDataDictionary[towerType];

            var upgradeCost = GetTowerUpgradeCost(in towerType);
            if (_towerCost < upgradeCost)
            {
                _notEnoughCostSequence.Restart();
                return;
            }

            TowerCost -= upgradeCost;

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, tempTower.transform.position);

            tempTower.TowerLevelUp();
            var towerLevel = tempTower.TowerLevel;

            tempTower.TowerSetting(towerData.TowerMeshes[towerLevel], towerData.BaseDamage * (towerLevel + 2),
                towerData.AttackRange, towerData.AttackRpm);
            upgradeButton.SetActive(!towerLevel.Equals(4));

            if (!rangeIndicator.transform.localScale.x.Equals(tempTower.TowerRange))
                _rangeIndicatorTween.ChangeStartValue(rangeIndicator.transform.localScale)
                    .ChangeEndValue(new Vector3(_curSelectedTower.TowerRange, 0.5f,
                        _curSelectedTower.TowerRange)).Restart();
        }

        private void ClickTower(Tower clickedTower)
        {
            if (Input.touchCount != 1) return;
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            _upgradeSellPanelTween.Restart();

            _isPanelOpen = true;
            if (_curSelectedTower) _curSelectedTower.Outline.enabled = false;
            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_curSelectedTower && _curSelectedTower.TowerData.IsUnitTower)
            {
                _curUnitTower.OffUnitIndicator();
            }

            _curSelectedTower = clickedTower;

            upgradeButton.SetActive(!clickedTower.TowerLevel.Equals(4));

            IfUnitTower();
            towerInfoUI.SetFollowTarget(_curSelectedTower.transform.position);
            UpdateTowerInfo();
            SetIndicator();
        }

        private void IfUnitTower()
        {
            var isUnitTower = _curSelectedTower.TowerData.IsUnitTower;
            if (isUnitTower)
            {
                _curUnitTower = _curSelectedTower.GetComponent<UnitTower>();
            }

            moveUnitButton.SetActive(isUnitTower);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerData.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var curTowerData = _towerDataDictionary[towerType];
            _sellTowerCost = curTowerData.TowerUpgradeCost / 2 * _towerCountDictionary[towerType] +
                             curTowerData.TowerBuildCost * towerLevel;
            towerInfoUI.SetTowerInfo(_curSelectedTower, curTowerData.IsUnitTower,
                towerLevel, GetTowerUpgradeCost(in towerType), _sellTowerCost);
        }

        private async UniTaskVoid ToggleTowerButtonAsync()
        {
            await UniTask.Delay(2000);
            if (!_isShowTowerBtn)
            {
                await _toggleTowerBtnImage.DOFade(0, 1);
                toggleTowerButton.SetActive(false);
            }
        }

        private int GetTowerUpgradeCost(in TowerType towerType)
        {
            return _towerCountDictionary[towerType] * _towerDataDictionary[towerType].TowerUpgradeCost +
                   _towerDataDictionary[towerType].TowerBuildCost * (_curSelectedTower.TowerLevel + 1);
        }

        private void MoveUnitButton()
        {
            moveUnitButton.SetActive(false);
            _upgradeSellPanelTween.PlayBackwards();
        }

        private void SellTower()
        {
            var towerData = _curSelectedTower.TowerData;
            _towerCountDictionary[towerData.TowerType]--;
            _towerCostTextDictionary[towerData.TowerType].text =
                towerData.TowerBuildCost * (_towerCountDictionary[towerData.TowerType] + 1) + "g";
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, _curSelectedTower.transform.position);

            TowerCost += _sellTowerCost;
            _sellTowerCost = 0;
            _towerManager.RemoveTower(_curSelectedTower);
            Destroy(_curSelectedTower.gameObject);
            OffUI();
        }

        private async UniTaskVoid GameStart()
        {
            StatusBarUIController.Instance.enabled = true;
            waveText.text = "0";
            Time.timeScale = 1;
            await _cameraManager.GameStartCamZoom();
        }

        private void GameOver()
        {
            pausePanelBlockImage.raycastTarget = false;
            resumeButton.interactable = false;
            gameOverPanel.SetActive(true);
            _pauseSequence.Restart();
            Time.timeScale = 0;
            DataManager.UpdateSurvivedWave((byte)(WaveManager.curWave - 1));
            DataManager.SaveLastSurvivedWave();
        }

        private void SpeedUp()
        {
            _curTimeScale = (byte)(_curTimeScale % 3 + 1);
            Time.timeScale = _curTimeScale;

            _curSpeedText.text = "x" + _curTimeScale;
        }
    }
}