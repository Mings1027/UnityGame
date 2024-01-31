using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.TowerDataControl;
using DG.Tweening;
using GameControl;
using IndicatorControl;
using MapControl;
using PoolObjectControl;
using StatusControl;
using TextControl;
using TMPro;
using TowerControl;
using UIControl;
using UnitControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class UIManager : Singleton<UIManager>
    {
#region Private Variable

        private WaveManager _waveManager;
        private TowerManager _towerManager;
        private TowerCardController _towerCardController;
        public ItemBagController itemBagController { get; private set; }

        private CancellationTokenSource _cts;
        private Camera _cam;

        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerGoldTextDictionary;

        private Tween _upgradeSellPanelTween;
        private Sequence _pauseSequence;
        private Sequence _notEnoughGoldSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;
        private Tween _upgradeButtonTween;
        private Sequence _towerGoldTween;
        private Sequence _towerHealTween;

        private Tower _curSelectedTower;
        private SummonTower _curSummonTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _towerGoldTexts;
        private TMP_Text _curSpeedText;

        private TowerRangeIndicator _towerRangeIndicator;

        private Image _sellButtonImage;

        private ushort _sellTowerGold;
        private int _towerGold;
        private byte _curTimeScale;

        private bool _isShowTowerBtn;
        private bool _isPanelOpen;
        private bool _startMoveUnit;
        private bool _clickSellBtn;

        private Vector3 _prevPos;

        private TowerInfoUI _towerInfoUI;

        private GameObject _upgradeButton;
        private GameObject _sellTowerButton;
        private GameObject _moveUnitButton;

        private GameHUD _gameHUD;
        private RectTransform _towerHealthRect;
        private TextMeshProUGUI _goldText;
        private Button _centerButton;
        private Button _speedButton;
        private Button _pauseButton;

        private GameObject _toggleTowerButton;
        private TowerDescriptionCard _towerDescriptionCard;

        private RectTransform _towerCardPanel;
        private Transform _notEnoughGoldPanel;

        private Transform _pausePanelBackground;
        private Button _resumeButton;
        private Button _mainMenuButton;
        private Toggle _bgmToggle;
        private Toggle _sfxToggle;
        private GameObject _gameOverPanel;
        private GameObject _gameEndPanel;
        private Image _pausePanelBlockImage;

        private MoveUnitController _moveUnitController;

#endregion

#region Property

        public Dictionary<TowerType, TowerDataPrefab> towerDataPrefabDictionary { get; private set; }

        public int towerGold
        {
            get => _towerGold;
            set
            {
                _towerGold = value;
                _goldText.text = _towerGold.ToString();
                _towerGoldTween.Restart();
            }
        }

        public CameraManager cameraManager { get; private set; }
        public TextMeshProUGUI waveText { get; private set; }

        public Dictionary<TowerType, string> towerNameDic { get; private set; }
        public Dictionary<TowerType, string> towerInfoDic { get; private set; }

        public bool enableMoveUnitController => _moveUnitController.enabled;

#endregion

#region Unity Event

        protected override void Awake()
        {
            base.Awake();
            Init();
            _towerInfoUI.Init();
            _gameHUD.Init();
            _towerManager.Init();

            _towerHealthRect = _gameHUD.towerHealth.GetComponent<RectTransform>();

            TowerButtonInit();
            TowerInit();
            LocaleDictionaryInit();
            TweenInit();
            MenuButtonInit();
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleTowerDictionary;
        }

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            cameraManager = _cam.GetComponentInParent<CameraManager>();
            _gameOverPanel.SetActive(false);
            _gameEndPanel.SetActive(false);
            GameStart();
            CheckCamPos().Forget();
            SoundManager.PlayBGM(SoundEnum.WaveEnd);
        }

        private void OnDisable()
        {
            _upgradeSellPanelTween?.Kill();
            _notEnoughGoldSequence?.Kill();
            _pauseSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _upgradeButtonTween?.Kill();
            _towerGoldTween?.Kill();
            _towerHealTween?.Kill();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Application.targetFrameRate = 60;
            }
            else
            {
                var curWave = WaveManager.curWave;
                if (curWave >= 1)
                {
                    BackendGameData.instance.UpdateSurvivedWave((byte)(_waveManager.isStartWave
                        ? curWave - 1
                        : curWave));
                }

                itemBagController.UpdateInventory();
                BackendGameData.instance.GameDataUpdate();
                Pause();
            }
        }

#endregion

#region Init

        private void Init()
        {
            Input.multiTouchEnabled = false;
            _cam = Camera.main;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _waveManager = FindAnyObjectByType<WaveManager>();
            _towerManager = FindAnyObjectByType<TowerManager>();
            _towerRangeIndicator = FindAnyObjectByType<TowerRangeIndicator>();
            itemBagController = FindAnyObjectByType<ItemBagController>();

            var uiPanel = transform.Find("UI Panel");
            _towerInfoUI = FindAnyObjectByType<TowerInfoUI>();
            var followTowerInfoUI = _towerInfoUI.transform.GetChild(0);
            _upgradeButton = followTowerInfoUI.Find("Upgrade Button").gameObject;
            _sellTowerButton = followTowerInfoUI.Find("Sell Tower Button").gameObject;
            _moveUnitButton = followTowerInfoUI.Find("Move Unit Button").gameObject;

            _gameHUD = FindAnyObjectByType<GameHUD>();
            var goldBackground = _gameHUD.transform.Find("Gold Background");
            _goldText = goldBackground.Find("Gold Text").GetComponent<TextMeshProUGUI>();
            var waveBackground = _gameHUD.transform.Find("Wave Background");
            waveText = waveBackground.Find("Wave Text").GetComponent<TextMeshProUGUI>();
            _centerButton = _gameHUD.transform.Find("Center Button").GetComponent<Button>();
            _speedButton = _gameHUD.transform.Find("Speed Button").GetComponent<Button>();
            _pauseButton = _gameHUD.transform.Find("Pause Button").GetComponent<Button>();
            _toggleTowerButton = uiPanel.Find("Toggle Tower Button").gameObject;

            _towerDescriptionCard = FindAnyObjectByType<TowerDescriptionCard>();
            _towerCardPanel = uiPanel.Find("Tower Card Panel").GetComponent<RectTransform>();
            _notEnoughGoldPanel = uiPanel.Find("Not Enough Gold Message").GetComponent<RectTransform>();
            _pausePanelBackground = uiPanel.Find("Pause Panel Background");
            _resumeButton = _pausePanelBackground.Find("Resume Button").GetComponent<Button>();
            var pausePanel = _pausePanelBackground.Find("Pause Panel");
            _mainMenuButton = pausePanel.Find("Main Menu Button").GetComponent<Button>();
            _bgmToggle = pausePanel.Find("BGM Toggle").GetComponent<Toggle>();
            _sfxToggle = pausePanel.Find("SFX Toggle").GetComponent<Toggle>();
            _gameOverPanel = pausePanel.Find("Game Over Background").gameObject;
            _gameEndPanel = pausePanel.Find("Game End Background").gameObject;
            _pausePanelBlockImage = pausePanel.Find("BlockImage").GetComponent<Image>();
            _moveUnitController = FindAnyObjectByType<MoveUnitController>();
        }

        private void TowerButtonInit()
        {
            _towerCardController = _towerCardPanel.GetComponentInChildren<TowerCardController>();
            _towerCardController.Init();

            for (var i = 0; i < _towerManager.towerDataPrefabs.Length; i++)
            {
                Debug.Log($"{_towerManager.towerDataPrefabs[i].towerData.towerType}  ");
                _towerCardController.SetDictionary(_towerManager.towerDataPrefabs[i].towerData.towerType,
                    _towerManager.towerDataPrefabs[i].towerData);
            }

            var towerButtons = _towerCardController.GetComponentsInChildren<TowerButton>();
            _towerGoldTexts = new TMP_Text[towerButtons.Length];

            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerGoldTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }

            _toggleTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                ToggleTowerButtons();
            });
            _upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);

                if (_clickSellBtn) ResetSprite();

                TowerUpgrade();
                _upgradeButtonTween.Restart();
            });
            _moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);
            _sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_clickSellBtn)
                {
                    _clickSellBtn = false;
                    _sellButtonImage.sprite = _gameHUD.sellSprite;
                    SellTower();
                }
                else
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    _clickSellBtn = true;
                    _sellButtonImage.sprite = _gameHUD.checkSprite;
                }
            });
            _sellButtonImage = _sellTowerButton.transform.GetChild(0).GetComponent<Image>();
        }

        private void TowerInit()
        {
            towerDataPrefabDictionary = new Dictionary<TowerType, TowerDataPrefab>();
            _towerCountDictionary = new Dictionary<TowerType, ushort>();
            _towerGoldTextDictionary = new Dictionary<TowerType, TMP_Text>();
            var towerDataPrefabs = _towerManager.towerDataPrefabs;

            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                towerDataPrefabDictionary.Add(towerDataPrefabs[i].towerData.towerType, towerDataPrefabs[i]);
                var t = towerDataPrefabs[i].towerData;
                t.InitState();
                _towerCountDictionary.Add(t.towerType, 0);
                var towerType = towerDataPrefabs[i].towerData.towerType;
                _towerGoldTextDictionary.Add(towerType, _towerGoldTexts[i]);
                _towerGoldTextDictionary[towerType].text =
                    towerDataPrefabDictionary[towerType].towerData.towerBuildCost + "G";
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
                towerNameDic.Add(towerType,
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString()));
                towerInfoDic.Add(towerType,
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, LocaleManager.CardKey + towerType));
            }
        }

        private void TweenInit()
        {
            _toggleTowerBtnImage = _toggleTowerButton.transform.GetChild(0).GetComponent<Image>();
            _upgradeSellPanelTween = _towerInfoUI.transform.DOScale(1, 0.15f).From(0).SetAutoKill(false)
                .Pause();
            _pauseSequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true).Pause()
                .Append(_pausePanelBackground.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Join(_pauseButton.transform.DOScale(0, 0.2f));
            _notEnoughGoldSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_notEnoughGoldPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(_notEnoughGoldPanel.DOScale(0, 0.2f).SetDelay(0.3f));
            _upgradeButtonTween = _upgradeButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack)
                .SetUpdate(true).SetAutoKill(false);
            _towerGoldTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_goldText.DOScale(1.2f, 0.125f))
                .Append(_goldText.DOScale(1, 0.125f));
            _towerHealTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_towerHealthRect.DOScale(1.2f, 0.125f))
                .Append(_towerHealthRect.DOScale(1, 0.125f));
        }

        private void MenuButtonInit()
        {
            _curTimeScale = 1;
            _pauseButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Pause();
            });
            _centerButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _centerButton.transform.DOScale(0, 0.2f).From(1).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(() => _centerButton.gameObject.SetActive(false));
                cameraManager.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
            });
            _centerButton.gameObject.SetActive(false);
            _resumeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Resume();
            });
            _bgmToggle.isOn = !SoundManager.bgmOn;
            _bgmToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.ToggleBGM(!_bgmToggle.isOn);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
            });
            _sfxToggle.isOn = !SoundManager.sfxOn;
            _sfxToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.ToggleSfx(!_sfxToggle.isOn);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
            });
            _mainMenuButton.onClick.AddListener(() =>
            {
                var curWave = WaveManager.curWave;
                if (curWave >= 1)
                {
                    BackendGameData.instance.UpdateSurvivedWave((byte)(_waveManager.isStartWave
                        ? curWave - 1
                        : curWave));
                }

                itemBagController.UpdateInventory();
                BackendGameData.instance.GameDataUpdate();
                SceneManager.LoadScene("Lobby");
            });
            _speedButton.onClick.AddListener(() =>
            {
                _speedButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                SpeedUp();
            });
            _curSpeedText = _speedButton.GetComponentInChildren<TMP_Text>();
        }

        private void ChangeLocaleTowerDictionary(Locale locale)
        {
            foreach (var towerType in towerNameDic.Keys.ToList())
            {
                towerNameDic[towerType] =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString());
            }

            foreach (var towerType in towerInfoDic.Keys.ToList())
            {
                towerInfoDic[towerType] =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, LocaleManager.CardKey + towerType);
            }

            if (_towerInfoUI != null) _towerInfoUI.LocaleTowerName();
            if (_towerDescriptionCard != null) _towerDescriptionCard.LocaleCardInfo();
        }

        private void ResetSprite()
        {
            _clickSellBtn = false;
            _sellButtonImage.sprite = _gameHUD.sellSprite;
        }

#endregion

#region Public Method

        public async UniTaskVoid MapSelectButton(byte difficultyLevel)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            mapManager.MakeMap(difficultyLevel);

            switch (difficultyLevel)
            {
                case 0:
                    towerGold = 2000;
                    BackendGameData.scoreMultiplier = 10;
                    break;
                case 1:
                    towerGold = 4000;
                    BackendGameData.scoreMultiplier = 20;
                    break;
                case 2:
                    towerGold = 5000;
                    BackendGameData.scoreMultiplier = 30;
                    break;
                case 3:
                    towerGold = 6000;
                    BackendGameData.scoreMultiplier = 40;
                    break;
            }

            BackendGameData.instance.SetLevel(difficultyLevel);
            await UniTask.Delay(500);
            _gameHUD.DisplayHUD();
            await _towerCardPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            cameraManager.enabled = true;
            _toggleTowerButton.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
        }

        public Sprite GetTowerType(TowerType t) =>
            towerDataPrefabDictionary[t].towerData.isMagicTower ? _gameHUD.magicSprite : _gameHUD.physicalSprite;

        public async UniTaskVoid InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var towerObject = Instantiate(towerDataPrefabDictionary[towerType].towerPrefab, placePos,
                Quaternion.identity);
            var lostGold = GetBuildGold(towerType);
            _towerCountDictionary[towerType]++;
            towerGold -= lostGold;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetGoldText(lostGold, false);
            _towerGoldTextDictionary[towerType].text = GetBuildGold(towerType) + "G";
            var towerTransform = towerObject.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;
            await DOTween.Sequence()
                .Join(towerObject.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack)).Append(
                    towerObject.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    }));

            if (towerObject.TryGetComponent(out AttackTower attackTower))
            {
                var towerData = towerDataPrefabDictionary[towerType].towerData;
                BuildTower(attackTower, towerData);
            }
            else
            {
                BuildSupportTower();
            }

            towerObject.TryGetComponent(out Tower tower);
            tower.OnClickTowerAction += ClickTower;
        }

        public void UIOff()
        {
            OffUI();

            if (!_isShowTowerBtn)
            {
                _toggleTowerButton.SetActive(true);
                _toggleTowerBtnImage.DOFade(1, 0.2f);
            }

            _gameHUD.DisplayHUD();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;
            _upgradeSellPanelTween.PlayBackwards();
            _startMoveUnit = false;
            _isPanelOpen = false;
            if (_curSelectedTower) _curSelectedTower.DeActiveIndicator();
            _curSelectedTower = null;
            _towerRangeIndicator.DisableIndicator();

            if (!_curSummonTower) return;
            _curSummonTower = null;
        }

        public void ShowTowerButton() => _isShowTowerBtn = true;

        public async UniTaskVoid SlideDown()
        {
            _isShowTowerBtn = false;
            _toggleTowerButton.SetActive(true);
            await UniTask.Delay(2000, cancellationToken: this.GetCancellationTokenOnDestroy());

            if (!_isShowTowerBtn)
            {
                await _toggleTowerBtnImage.DOFade(0, 1);
                _toggleTowerButton.SetActive(false);
            }
        }

        public void YouCannotMove() => _gameHUD.CannotMoveHere();

        public TowerHealth GetTowerHealth() => _gameHUD.towerHealth;

        public void TowerHeal()
        {
            _towerHealTween.Restart();
            _gameHUD.towerHealth.Heal(5);
        }

        public Mana GetTowerMana() => _gameHUD.towerMana;

        public bool IsEnoughGold(TowerType towerType)
        {
            if (_towerGold >= GetBuildGold(towerType))
            {
                return true;
            }

            _notEnoughGoldPanel.gameObject.SetActive(true);
            _notEnoughGoldSequence.OnComplete(() => _notEnoughGoldPanel.gameObject.SetActive(false)).Restart();

            return false;
        }

        public void GameEnd()
        {
            _pausePanelBlockImage.raycastTarget = false;
            _resumeButton.interactable = false;
            _gameEndPanel.SetActive(true);
            _pauseSequence.Restart();
            Time.timeScale = 0;
        }

#endregion

#region Private Method

        private async UniTaskVoid CheckCamPos()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(2000, cancellationToken: _cts.Token);

                if (Vector3.Distance(cameraManager.transform.position, Vector3.zero) > 10)
                {
                    if (_centerButton.gameObject.activeSelf) continue;
                    _centerButton.gameObject.SetActive(true);
                    await _centerButton.transform.DOScale(1, 0.2f).From(0).SetEase(Ease.OutBack);
                }
            }
        }

        private void ToggleTowerButtons()
        {
            if (_isShowTowerBtn) return;
            _toggleTowerButton.SetActive(false);
            _isShowTowerBtn = true;
            _towerCardController.SlideUp();
        }

        private void BuildTower(AttackTower attackTower, TowerData towerData)
        {
            var attackTowerData = (AttackTowerData)towerData;
            attackTower.TowerLevelUp();
            attackTower.TowerSetting(attackTowerData.towerMeshes[attackTower.TowerLevel], attackTowerData.curDamage,
                attackTowerData.curRange, attackTowerData.curRpm);
            _towerManager.AddTower(attackTower);
        }

        private void BuildSupportTower()
        {
            _gameHUD.towerMana.ManaRegenValue++;
        }

        private void TowerUpgrade()
        {
            var attackTower = (AttackTower)_curSelectedTower;
            var towerType = attackTower.towerType;
            var towerData = towerDataPrefabDictionary[towerType].towerData;
            var battleTowerData = (AttackTowerData)towerData;
            var upgradeGold = GetUpgradeGold(in towerType);

            if (_towerGold < upgradeGold)
            {
                _notEnoughGoldPanel.gameObject.SetActive(true);
                _notEnoughGoldSequence.OnComplete(() => _notEnoughGoldPanel.gameObject.SetActive(false)).Restart();

                return;
            }

            towerGold -= upgradeGold;
            var position = attackTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(upgradeGold, false);
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);
            attackTower.TowerLevelUp();
            var towerLevel = attackTower.TowerLevel;
            attackTower.TowerSetting(battleTowerData.towerMeshes[towerLevel],
                battleTowerData.curDamage * (towerLevel + 1), battleTowerData.curRange, battleTowerData.curRpm);
            _upgradeButton.SetActive(!towerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.TowerRange);
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoUI.SetTowerInfo(attackTower, towerData.isUnitTower, towerLevel, GetUpgradeGold(in towerType),
                _sellTowerGold);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (_startMoveUnit) return;
            if (Input.touchCount != 1) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_clickSellBtn) ResetSprite();
            _isPanelOpen = true;
            _upgradeSellPanelTween.Restart();

            if (clickedTower.Equals(_curSelectedTower)) return;
            if (_curSelectedTower) _curSelectedTower.DeActiveIndicator();
            if (_curSummonTower) _curSummonTower.DeActiveIndicator();
            var isUnitTower = towerDataPrefabDictionary[clickedTower.towerType].towerData.isUnitTower;
            if (isUnitTower) _curSummonTower = clickedTower.GetComponent<SummonTower>();
            _curSelectedTower = clickedTower;
            _curSelectedTower.ActiveIndicator();
            var position = clickedTower.transform.position;

            if (_curSelectedTower is AttackTower curTower)
            {
                UpdateAttackTowerInfo(curTower, position, isUnitTower);
            }
            else
            {
                _towerRangeIndicator.DisableIndicator();
                UpdateSupportTowerInfo(clickedTower);
            }

            _towerInfoUI.SetInfoUI(position);
        }

        private void UpdateAttackTowerInfo(AttackTower attackTower, Vector3 position, bool isUnitTower)
        {
            _moveUnitButton.SetActive(isUnitTower);
            _upgradeButton.SetActive(!attackTower.TowerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.TowerRange);
            var towerType = _curSelectedTower.towerType;
            var curTower = (AttackTower)_curSelectedTower;
            var towerLevel = curTower.TowerLevel;
            var curTowerData = towerDataPrefabDictionary[towerType].towerData;
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoUI.SetTowerInfo(curTower, curTowerData.isUnitTower, towerLevel, GetUpgradeGold(in towerType),
                _sellTowerGold);
        }

        private void UpdateSupportTowerInfo(Tower clickedTower)
        {
            _upgradeButton.SetActive(false);
            _moveUnitButton.SetActive(false);
            _towerInfoUI.SetSupportInfoUI();
            var supportTower = (SupportTower)_curSelectedTower;
            _sellTowerGold = GetTowerSellGold(clickedTower.towerType);
            _towerInfoUI.SetSupportTowerInfo(supportTower, _sellTowerGold);
        }

        private ushort GetBuildGold(in TowerType towerType)
        {
            return (ushort)(towerDataPrefabDictionary[towerType].towerData.towerBuildCost +
                            towerDataPrefabDictionary[towerType].towerData.extraBuildCost *
                            _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeGold(in TowerType towerType)
        {
            var curTower = (AttackTower)_curSelectedTower;

            return (ushort)(towerDataPrefabDictionary[towerType].towerData.towerUpgradeCost +
                            towerDataPrefabDictionary[towerType].towerData.extraUpgradeCost * curTower.TowerLevel);
        }

        private ushort GetTowerSellGold(in TowerType towerType)
        {
            return (ushort)(towerDataPrefabDictionary[towerType].towerData.towerBuildCost +
                            towerDataPrefabDictionary[towerType].towerData.extraBuildCost *
                            (_towerCountDictionary[towerType] - 1));
        }

        private void MoveUnitButton()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _moveUnitController.FocusUnitTower(_curSummonTower);
            _upgradeSellPanelTween.PlayBackwards();
            _moveUnitButton.SetActive(false);
            _startMoveUnit = true;
        }

        private void SellTower()
        {
            SoundManager.PlayUISound(_sellTowerGold < 100 ? SoundEnum.LowCost :
                _sellTowerGold < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);
            var towerType = _curSelectedTower.towerType;
            _towerCountDictionary[towerType]--;
            _towerGoldTextDictionary[towerType].text = GetBuildGold(towerType) + "G";
            var position = _curSelectedTower.transform.position;
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);
            towerGold += _sellTowerGold;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(_sellTowerGold);
            _sellTowerGold = 0;

            if (_curSelectedTower is AttackTower curTower) _towerManager.RemoveTower(curTower);
            else
            {
                // 현재 supporttower 종류가 manaRecovery 밖에 없기 때문에 TryGetComponent(out SupportTower _) 를 
                // 해도 되지만 여러개가 되면 TryGetComponent(out ManaTower _) 이런식으로 바꿔줘야함 
                if (_curSelectedTower.TryGetComponent(out SupportTower _)) _gameHUD.towerMana.ManaRegenValue--;
            }

            _curSelectedTower.DisableObject();
            OffUI();
        }

        private void GameStart()
        {
            waveText.text = "0";
            Time.timeScale = 1;
            cameraManager.GameStartCamZoom();
        }

        public void GameOver()
        {
            _towerCardController.SlideDown();
            _pausePanelBlockImage.raycastTarget = false;
            _resumeButton.interactable = false;
            _gameOverPanel.SetActive(true);
            _pauseSequence.Restart();
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
            Input.multiTouchEnabled = false;
            Time.timeScale = 0;
            _pausePanelBlockImage.raycastTarget = false;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            Input.multiTouchEnabled = true;
            Time.timeScale = _curTimeScale;
            _pausePanelBlockImage.raycastTarget = true;
            _pauseSequence.PlayBackwards();
        }

#endregion
    }
}