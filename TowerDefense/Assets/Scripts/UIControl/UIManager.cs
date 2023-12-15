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
using TextControl;
using TMPro;
using TowerControl;
using UnitControl.TowerUnitControl;
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
        private TowerRangeIndicator _towerRangeIndicator;
        private TowerManager _towerManager;
        private CameraManager _cameraManager;
        private TowerCardController _towerCardController;
        private Camera _cam;
        private Dictionary<TowerType, TowerData> _towerDataDictionary;
        private Dictionary<TowerType, GameObject> _towerObjDictionary;
        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerCostTextDictionary;

        private Tween _upgradeSellPanelTween;
        private Sequence _pauseSequence;
        private Sequence _notEnoughCostSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;
        private Tween _towerInfoWindowTween;
        private Tween _upgradeButtonTween;

        private Tower _curSelectedTower;
        private UnitTower _curUnitTower;
        private SupportTower _curSupportTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _towerCostTexts;
        private TMP_Text _curSpeedText;

        private ushort _sellTowerCost;
        private int _towerCost;
        private byte _curTimeScale;
        private bool _isShowTowerBtn;
        private bool _isSpeedUp;
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
        public Health BaseTowerHealth => baseTowerHealth;
        public Mana Mana => mana;
        public Dictionary<TowerType, string> towerNameDic { get; private set; }
        public Dictionary<TowerType, string> towerInfoDic { get; private set; }

        [SerializeField] private HealthBar healthBar;
        [SerializeField] private ObjectHealth baseTowerHealth;
        [SerializeField] private ManaBar manaBar;
        [SerializeField] private Mana mana;
        [SerializeField] private int lifeCount;
        [SerializeField] private int towerMana;
        [SerializeField] private ushort startCost;

        [SerializeField] private Transform notEnoughCostPanel;
        [SerializeField] private RectTransform towerPanel;
        [SerializeField] private Image cantMoveImage;
        [SerializeField] private Image checkMoveUnitPanel;
        [SerializeField] private Sprite physicalSprite;
        [SerializeField] private Sprite magicSprite;

        [Header("----------Tower Panel----------"), SerializeField]
        private RectTransform infoWindow;

        [SerializeField] private TowerData[] towerDataList;
        [SerializeField] private GameObject[] towerPrefabs;
        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private TowerCardUI towerCardUI;
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

        [SerializeField] private MoveUnitController moveUnitController;
        [SerializeField] private GameObject bonusMapPanel;

        #region Unity Event

        protected override void Awake()
        {
            base.Awake();
            Input.multiTouchEnabled = false;
            _cam = Camera.main;

            TowerButtonInit();
            TowerInit();
            LocaleDictionaryInit();
            TweenInit();
            MenuButtonInit();

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleDictionary;
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _towerRangeIndicator = FindObjectOfType<TowerRangeIndicator>();
            _towerManager = FindObjectOfType<TowerManager>();
            gameOverPanel.SetActive(false);
            gameEndPanel.SetActive(false);
            mana.Init(towerMana);
            manaBar.Init(mana);
            BaseTowerHealth.Init(lifeCount);
            healthBar.Init(BaseTowerHealth);
            BaseTowerHealth.OnDeadEvent += GameOver;
            GameStart().Forget();
        }

        private void OnDisable()
        {
            _upgradeSellPanelTween?.Kill();
            _notEnoughCostSequence?.Kill();
            _pauseSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _towerInfoWindowTween?.Kill();
            _upgradeButtonTween?.Kill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsOnUI = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsOnUI = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Application.targetFrameRate = 60;
                Resume();
            }
            else
            {
                Pause();
            }
        }

        #endregion

        #region Init

        private void TowerInit()
        {
            _towerDataDictionary = new Dictionary<TowerType, TowerData>();
            _towerObjDictionary = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, ushort>();
            _towerCostTextDictionary = new Dictionary<TowerType, TMP_Text>();

            for (var i = 0; i < towerDataList.Length; i++)
            {
                var t = towerDataList[i];
                t.InitState();
                _towerDataDictionary.Add(t.TowerType, t);
                _towerObjDictionary.Add(t.TowerType, towerPrefabs[i]);
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

            _upgradeSellPanelTween = towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetAutoKill(false).Pause();

            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(pausePanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOScale(0, 0.2f));

            _notEnoughCostSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughCostPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughCostPanel.DOScale(0, 0.2f).SetDelay(0.3f));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                    .SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));

            _towerInfoWindowTween = infoWindow.DOAnchorPosY(200, 0.3f).From()
                .OnComplete(() => _openInfoWindow = true).SetAutoKill(false).Pause();
            _upgradeButtonTween = upgradeButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack)
                .SetUpdate(true).SetAutoKill(false);
        }

        private void TowerButtonInit()
        {
            _towerCardController = towerPanel.GetComponentInChildren<TowerCardController>();
            _towerCardController.Init();
            for (var i = 0; i < towerDataList.Length; i++)
            {
                _towerCardController.SetDictionary(i, towerDataList[i]);
            }

            var towerButtons = _towerCardController.GetComponentsInChildren<TowerButton>();
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
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                TowerUpgrade();
                _upgradeButtonTween.Restart();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            checkMoveUnitPanel.enabled = false;

            sellTowerButton.GetComponent<Button>().onClick.AddListener(SellTower);
        }

        private void MenuButtonInit()
        {
            _towerCost = startCost;
            _curTimeScale = 1;
            TowerCost = _towerCost;

            pauseButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Pause();
            });

            centerButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                centerButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                if (_cameraManager.transform.position == Vector3.zero) return;
                _cameraManager.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
            });
            resumeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                Resume();
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
                if (WaveManager.curWave <= 1) return;
                DataManager.SaveLastSurvivedWave();
            });

            speedUpButton.onClick.AddListener(() =>
            {
                speedUpButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                SpeedUp();
            });
            _curSpeedText = speedUpButton.GetComponentInChildren<TMP_Text>();

            bonusMapPanel.SetActive(false);
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

            if (towerInfoUI != null) towerInfoUI.LocaleTowerName();
            if (towerCardUI != null) towerCardUI.LocaleCardInfo();
        }

        #endregion

        #region Public Method

        public async UniTaskVoid MapSelectButton(int index)
        {
            FindObjectOfType<MapManager>().MakeMap(index);
            DataManager.SetLevel((byte)index);

            await transform.GetChild(0).DOScale(0, 0.5f).SetEase(Ease.InBack);
            _towerInfoWindowTween.Restart();
            await towerPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            _cameraManager.enabled = true;
            toggleTowerButton.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
        }

        public Sprite GetTowerType(TowerData t) => t.IsMagicTower ? magicSprite : physicalSprite;

        public void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var t = Instantiate(_towerObjDictionary[towerType], placePos, Quaternion.identity);
            if (t.TryGetComponent(out Tower tower))
            {
                var towerTransform = t.transform;
                towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
                towerTransform.GetChild(0).forward = towerForward;

                var towerData = _towerDataDictionary[towerType];
                BuildTower(tower, placePos, towerData).Forget();
            }
            else
            {
                var towerTransform = t.transform;
                towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);

                BuildSupportTower(t.GetComponent<SupportTower>(), placePos);
            }
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

        public async UniTaskVoid SlideDown()
        {
            _isShowTowerBtn = false;
            toggleTowerButton.SetActive(true);

            await UniTask.Delay(2000);
            if (!_isShowTowerBtn)
            {
                await _toggleTowerBtnImage.DOFade(0, 1);
                toggleTowerButton.SetActive(false);
            }
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
            if (_curSupportTower) _curSupportTower.Outline.enabled = false;
            _curSupportTower = null;
            _towerRangeIndicator.DisableIndicator();

            if (checkMoveUnitPanel.enabled) checkMoveUnitPanel.enabled = false;
            if (!_curUnitTower) return;
            _curUnitTower.OffUnitIndicator();
            _curUnitTower = null;
        }

        public bool IsEnoughCost(TowerType towerType)
        {
            if (_towerCost >= GetBuildCost(towerType))
            {
                return true;
            }

            _notEnoughCostSequence.Restart();
            return false;
        }

        public void GameEnd()
        {
            gameEndPanel.SetActive(true);
            _pauseSequence.Restart();
            DataManager.SaveLastSurvivedWave();
        }

        public void BonusMap()
        {
            bonusMapPanel.SetActive(true);
            bonusMapPanel.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    bonusMapPanel.transform.DOScale(0, 0.25f).SetEase(Ease.InBack).SetDelay(1)
                        .OnComplete(() => bonusMapPanel.SetActive(false));
                });
        }

        #endregion

        #region Private Method

        private void ToggleTowerButtons()
        {
            IsOnUI = false;
            if (_isShowTowerBtn) return;
            toggleTowerButton.SetActive(false);
            _isShowTowerBtn = true;
            _towerCardController.SlideUp();
        }

        private async UniTaskVoid BuildTower(Tower t, Vector3 placePos, TowerData towerData)
        {
            var towerType = t.TowerData.TowerType;
            var battleTowerData = (BattleTowerData)towerData;
            var lostCost = GetBuildCost(towerType);

            _towerCountDictionary[towerType]++;
            TowerCost -= lostCost;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetCostText(lostCost, false);
            _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "g";
            await DOTween.Sequence().Join(t.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    }));
            t.TowerLevelUp();
            t.TowerSetting(battleTowerData.TowerMeshes[t.TowerLevel], battleTowerData.BaseDamage,
                battleTowerData.AttackRange,
                battleTowerData.AttackRpm);
            t.OnClickTower += ClickTower;

            _towerManager.AddTower(t);
        }

        private void BuildSupportTower(SupportTower t, Vector3 placePos)
        {
            var towerType = t.TowerType;

            var lostCost = GetBuildCost(towerType);
            _towerCountDictionary[towerType]++;

            _sellTowerCost = GetTowerSellCost(towerType);

            TowerCost -= lostCost;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetCostText(lostCost, false);
            _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "g";
            DOTween.Sequence().Join(t.transform.GetChild(0).DOScale(t.transform.GetChild(0).localScale, 0.25f).From(0)
                    .SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(2, 0.5f).SetEase(Ease.InExpo))
                .OnComplete(() =>
                {
                    PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                    _cam.transform.DOShakePosition(0.05f);
                });
            t.OnClickTower += ClickSupportTower;
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerData.TowerType;
            var towerData = _towerDataDictionary[towerType];
            var battleTowerData = (BattleTowerData)towerData;

            var upgradeCost = GetUpgradeCost(in towerType);
            if (_towerCost < upgradeCost)
            {
                _notEnoughCostSequence.Restart();
                return;
            }

            TowerCost -= upgradeCost;
            var position = tempTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                .SetCostText(upgradeCost, false);

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

            tempTower.TowerLevelUp();
            var towerLevel = tempTower.TowerLevel;

            tempTower.TowerSetting(battleTowerData.TowerMeshes[towerLevel],
                battleTowerData.BaseDamage * (towerLevel + 1),
                battleTowerData.AttackRange, battleTowerData.AttackRpm);
            upgradeButton.SetActive(!towerLevel.Equals(4));

            _towerRangeIndicator.SetIndicator(position, tempTower.TowerRange);

            _sellTowerCost = GetTowerSellCost(towerType);
            towerInfoUI.SetTowerInfo(tempTower, towerData.IsUnitTower, towerLevel, GetUpgradeCost(in towerType),
                _sellTowerCost);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (Input.touchCount != 1) return;
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            _upgradeSellPanelTween.Restart();

            _isPanelOpen = true;
            if (_curSupportTower)
            {
                _curSupportTower.Outline.enabled = false;
                _curSupportTower = null;
            }

            if (_curSelectedTower) _curSelectedTower.Outline.enabled = false;
            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_curUnitTower)
            {
                _curUnitTower.OffUnitIndicator();
            }

            var isUnitTower = clickedTower.TowerData.IsUnitTower;
            if (isUnitTower)
            {
                _curUnitTower = clickedTower.GetComponent<UnitTower>();
            }

            _curSelectedTower = clickedTower;

            moveUnitButton.SetActive(isUnitTower);
            upgradeButton.SetActive(!clickedTower.TowerLevel.Equals(4));

            var position = clickedTower.transform.position;
            towerInfoUI.SetInfoUI(position);
            _towerRangeIndicator.SetIndicator(position, clickedTower.TowerRange);
            UpdateTowerInfo();
        }

        private void ClickSupportTower(SupportTower clickTower)
        {
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            _isPanelOpen = true;
            if (_curSupportTower) _curSupportTower.Outline.enabled = false;
            if (_curSelectedTower)
            {
                _curSelectedTower.Outline.enabled = false;
                _towerRangeIndicator.DisableIndicator();
                _curSelectedTower = null;
            }

            if (clickTower.Equals(_curSupportTower)) return;
            _curSupportTower = clickTower;
            upgradeButton.SetActive(false);
            moveUnitButton.SetActive(false);
            _upgradeSellPanelTween.Restart();
            towerInfoUI.SetInfoUI(clickTower.transform.position);
            towerInfoUI.SetSupportInfoUI();
            _sellTowerCost = GetTowerSellCost(clickTower.TowerType);
            towerInfoUI.SetSupportTowerInfo(clickTower, _sellTowerCost);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerData.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var curTowerData = _towerDataDictionary[towerType];
            _sellTowerCost = GetTowerSellCost(towerType);
            towerInfoUI.SetTowerInfo(_curSelectedTower, curTowerData.IsUnitTower,
                towerLevel, GetUpgradeCost(in towerType), _sellTowerCost);
        }

        private ushort GetBuildCost(in TowerType towerType)
        {
            return (ushort)(_towerDataDictionary[towerType].TowerBuildCost +
                            _towerDataDictionary[towerType].ExtraBuildCost * _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeCost(in TowerType towerType)
        {
            return (ushort)(_towerDataDictionary[towerType].TowerUpgradeCost +
                            _towerDataDictionary[towerType].ExtraUpgradeCost * _curSelectedTower.TowerLevel);
        }

        private ushort GetTowerSellCost(in TowerType towerType)
        {
            return (ushort)(_towerDataDictionary[towerType].TowerBuildCost +
                            _towerDataDictionary[towerType].ExtraBuildCost * (_towerCountDictionary[towerType] - 1));
        }

        private void MoveUnitButton()
        {
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            checkMoveUnitPanel.enabled = true;
            moveUnitController.FocusUnitTower(_curUnitTower);
            _upgradeSellPanelTween.PlayBackwards();
            moveUnitButton.SetActive(false);
        }

        private void SellTower()
        {
            if (_curSelectedTower)
            {
                SoundManager.Instance.PlaySound(_curSelectedTower.TowerLevel < 2 ? SoundEnum.LowCost :
                    _curSelectedTower.TowerLevel < 4 ? SoundEnum.MediumCost : SoundEnum.HighCost);

                var towerData = _curSelectedTower.TowerData;
                _towerCountDictionary[towerData.TowerType]--;
                _towerCostTextDictionary[towerData.TowerType].text = GetBuildCost(towerData.TowerType) + "g";
                var position = _curSelectedTower.transform.position;
                PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

                TowerCost += _sellTowerCost;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                    .SetCostText(_sellTowerCost);
                _sellTowerCost = 0;
                _towerManager.RemoveTower(_curSelectedTower);
                _curSelectedTower.ObjectDisable();
            }
            else
            {
                var towerType = _curSupportTower.TowerType;
                _towerCountDictionary[towerType]--;
                _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "g";
                var position = _curSupportTower.transform.position;
                PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

                TowerCost += _sellTowerCost;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                    .SetCostText(_sellTowerCost);
                _sellTowerCost = 0;
                _curSupportTower.ObjectDisable();
            }

            OffUI();
        }

        private async UniTaskVoid GameStart()
        {
            // StatusBarUIController.Instance.enabled = true;
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
            _towerManager.StopTargeting();
            DataManager.UpdateSurvivedWave((byte)(WaveManager.curWave - 1));
            DataManager.SaveLastSurvivedWave();
            Time.timeScale = 0;
        }

        private void SpeedUp()
        {
            _curTimeScale = (byte)(_curTimeScale % 3 + 1);
            Time.timeScale = _curTimeScale;

            _curSpeedText.text = "x" + _curTimeScale;
        }

        private void Pause()
        {
            Time.timeScale = 0;
            pausePanelBlockImage.raycastTarget = false;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            Time.timeScale = _curTimeScale;
            pausePanelBlockImage.raycastTarget = true;
            _pauseSequence.PlayBackwards();
        }

        #endregion
    }
}