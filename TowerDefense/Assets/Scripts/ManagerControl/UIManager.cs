using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.TowerDataControl;
using DG.Tweening;
using GameControl;
using IndicatorControl;
using MapControl;
using PoolObjectControl;
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
    [Serializable]
    public class TowerDataPrefab
    {
        public TowerData towerData;
        public GameObject towerPrefab;
    }

    public class UIManager : Singleton<UIManager>
    {
        private WaveManager _waveManager;
        private TowerManager _towerManager;
        private TowerCardController _towerCardController;

        private CancellationTokenSource _cts;
        private Camera _cam;

        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerCostTextDictionary;

        private Tween _upgradeSellPanelTween;
        private Sequence _pauseSequence;
        private Sequence _notEnoughCostSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;
        private Tween _upgradeButtonTween;

        private AttackTower _curSelectedTower;
        private SummonTower _curSummonTower;
        private SupportTower _curSupportTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _towerCostTexts;
        private TMP_Text _curSpeedText;

        private TowerRangeIndicator _towerRangeIndicator;

        // private Image _upgradeButtonImage;
        private Image _sellButtonImage;

        private ushort _sellTowerCost;
        private int _towerCost;
        private byte _curTimeScale;
        private bool _isShowTowerBtn;

        private bool _isPanelOpen;

        // private bool _clickUpgradeBtn;
        private bool _clickSellBtn;
        private Vector3 _prevPos;

        public Dictionary<TowerType, TowerDataPrefab> TowerDataPrefabDictionary { get; private set; }

        public int TowerCost
        {
            get => _towerCost;
            set
            {
                _towerCost = value;
                costText.text = _towerCost.ToString();
            }
        }

        public CameraManager CameraManager { get; private set; }

        public TextMeshProUGUI WaveText => waveText;

        public Dictionary<TowerType, string> towerNameDic { get; private set; }
        public Dictionary<TowerType, string> towerInfoDic { get; private set; }

        [SerializeField] private GameHUD gameHUD;
        [SerializeField] private TowerMana towerMana;

        [SerializeField] private Transform notEnoughCostPanel;
        [SerializeField] private RectTransform towerPanel;
        [SerializeField] private Image cantMoveImage;
        [SerializeField] private Sprite physicalSprite;

        [SerializeField] private Sprite magicSprite;

        // [SerializeField] private Sprite upgradeSprite;
        [SerializeField] private Sprite sellSprite;
        [SerializeField] private Sprite checkSprite;

        [SerializeField] private TowerDataPrefab[] towerDataPrefabs;
        [SerializeField] private TowerInfoUI towerInfoUI;

        [SerializeField] private TowerDescriptionCard towerDescriptionCard;

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

        #region Unity Event

        protected override void Awake()
        {
            base.Awake();
            Input.multiTouchEnabled = false;
            _cam = Camera.main;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            TowerButtonInit();
            TowerInit();
            LocaleDictionaryInit();
            TweenInit();
            MenuButtonInit();
            _towerRangeIndicator = (TowerRangeIndicator)FindAnyObjectByType(typeof(TowerRangeIndicator));
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleDictionary;
        }

        private void Start()
        {
            CameraManager = _cam.GetComponentInParent<CameraManager>();
            _waveManager = (WaveManager)FindAnyObjectByType(typeof(WaveManager));
            _towerManager = (TowerManager)FindAnyObjectByType(typeof(TowerManager));
            gameOverPanel.SetActive(false);
            gameEndPanel.SetActive(false);
            GameStart();
            CheckCamPos().Forget();
        }

        private void OnDisable()
        {
            _upgradeSellPanelTween?.Kill();
            _notEnoughCostSequence?.Kill();
            _pauseSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _upgradeButtonTween?.Kill();

            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Pause();
            }
            else
            {
                Application.targetFrameRate = 60;
            }
        }

        #endregion

        #region Init

        private void TowerInit()
        {
            TowerDataPrefabDictionary = new Dictionary<TowerType, TowerDataPrefab>();
            _towerCountDictionary = new Dictionary<TowerType, ushort>();
            _towerCostTextDictionary = new Dictionary<TowerType, TMP_Text>();

            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                TowerDataPrefabDictionary.Add(towerDataPrefabs[i].towerData.TowerType, towerDataPrefabs[i]);

                var t = towerDataPrefabs[i].towerData;
                t.InitState();
                _towerCountDictionary.Add(t.TowerType, 0);

                var towerType = towerDataPrefabs[i].towerData.TowerType;
                _towerCostTextDictionary.Add(towerType, _towerCostTexts[i]);
                _towerCostTextDictionary[towerType].text =
                    TowerDataPrefabDictionary[towerType].towerData.TowerBuildCost + "G";
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
                .Append(pausePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Join(pauseButton.transform.DOScale(0, 0.2f));

            _notEnoughCostSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notEnoughCostPanel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce))
                .Append(notEnoughCostPanel.DOScale(0, 0.2f).SetDelay(0.3f));

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                    .SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));

            _upgradeButtonTween = upgradeButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack)
                .SetUpdate(true).SetAutoKill(false);
        }

        private void TowerButtonInit()
        {
            _towerCardController = towerPanel.GetComponentInChildren<TowerCardController>();
            _towerCardController.Init();
            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                _towerCardController.SetDictionary(i, towerDataPrefabs[i].towerData);
            }

            var towerButtons = _towerCardController.GetComponentsInChildren<TowerButton>();
            _towerCostTexts = new TMP_Text[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerCostTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }

            toggleTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                ToggleTowerButtons();
            });

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                if (_clickSellBtn)
                {
                    _sellButtonImage.sprite = sellSprite;
                    _clickSellBtn = false;
                }

                // if (_clickUpgradeBtn)
                // {
                //     _clickUpgradeBtn = false;
                //     _upgradeButtonImage.sprite = upgradeSprite;
                TowerUpgrade();
                // }
                // else
                // {
                //     _clickUpgradeBtn = true;
                //     _upgradeButtonImage.sprite = checkSprite;
                // }

                _upgradeButtonTween.Restart();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                // if (_clickUpgradeBtn)
                // {
                //     _upgradeButtonImage.sprite = upgradeSprite;
                //     _clickUpgradeBtn = false;
                // }

                if (_clickSellBtn)
                {
                    _clickSellBtn = false;
                    _sellButtonImage.sprite = sellSprite;
                    SellTower();
                }
                else
                {
                    SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                    _clickSellBtn = true;
                    _sellButtonImage.sprite = checkSprite;
                }
            });

            // _upgradeButtonImage = upgradeButton.transform.GetChild(0).GetComponent<Image>();
            _sellButtonImage = sellTowerButton.transform.GetChild(0).GetComponent<Image>();
        }

        private void MenuButtonInit()
        {
            _curTimeScale = 1;

            pauseButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                Pause();
            });

            centerButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                centerButton.transform.DOScale(0, 0.2f).From(1).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(() => centerButton.gameObject.SetActive(false));
                CameraManager.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);
            });
            centerButton.gameObject.SetActive(false);

            resumeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
                Resume();
            });
            bgmToggle.isOn = !SoundManager.Instance.BGMOn;
            bgmToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.ToggleBGM(!bgmToggle.isOn);
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            });
            sfxToggle.isOn = !SoundManager.Instance.SfxOn;
            sfxToggle.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.ToggleSfx(!sfxToggle.isOn);
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            });

            mainMenuButton.onClick.AddListener(delegate
            {
                SceneManager.LoadScene("Lobby");
                if (_waveManager.curWave < 1) return;
                DataManager.UpdateSurvivedWave((byte)(_waveManager.IsStartWave
                    ? _waveManager.curWave - 1
                    : _waveManager.curWave));
                DataManager.SaveLastSurvivedWave();
            });

            speedUpButton.onClick.AddListener(() =>
            {
                speedUpButton.transform.DOScale(1, 0.2f).From(0.7f).SetEase(Ease.OutBack).SetUpdate(true);
                SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
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

            if (towerInfoUI != null) towerInfoUI.LocaleTowerName();
            if (towerDescriptionCard != null) towerDescriptionCard.LocaleCardInfo();
        }

        private void ResetSprite()
        {
            // if (_clickUpgradeBtn)
            // {
            //     _clickUpgradeBtn = false;
            //     _upgradeButtonImage.sprite = upgradeSprite;
            // }

            if (_clickSellBtn)
            {
                _clickSellBtn = false;
                _sellButtonImage.sprite = sellSprite;
            }
        }

        #endregion

        #region Public Method

        public async UniTaskVoid MapSelectButton(int index)
        {
            var mapManager = (MapManager)FindAnyObjectByType(typeof(MapManager));
            mapManager.MakeMap(index);

            switch (index)
            {
                case 1:
                    mapManager.connectionProbability = 30;
                    TowerCost = 2000;
                    break;
                case 2:
                    mapManager.connectionProbability = 50;
                    TowerCost = 4000;
                    break;
                case 3:
                    mapManager.connectionProbability = 60;
                    TowerCost = 5000;
                    break;
                case 4:
                    mapManager.connectionProbability = 70;
                    TowerCost = 6000;
                    break;
                default:
                    mapManager.connectionProbability = mapManager.connectionProbability;
                    break;
            }

            DataManager.SetLevel((byte)index);

            await UniTask.Delay(500);
            gameHUD.DisplayHUD();
            await towerPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            CameraManager.enabled = true;
            toggleTowerButton.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
        }

        public Sprite GetTowerType(TowerType t) =>
            TowerDataPrefabDictionary[t].towerData.IsMagicTower ? magicSprite : physicalSprite;

        public void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var t = Instantiate(TowerDataPrefabDictionary[towerType].towerPrefab, placePos, Quaternion.identity);

            var lostCost = GetBuildCost(towerType);
            _towerCountDictionary[towerType]++;
            TowerCost -= lostCost;

            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetCostText(lostCost, false);
            _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "G";


            if (t.TryGetComponent(out AttackTower tower))
            {
                var towerTransform = t.transform;
                towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
                towerTransform.GetChild(0).forward = towerForward;

                var towerData = TowerDataPrefabDictionary[towerType].towerData;
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

            gameHUD.DisplayHUD();
        }

        public void ShowTowerButton() => _isShowTowerBtn = true;

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
            cantMoveImage.enabled = true;
            _cantMoveImageSequence.OnComplete(() => cantMoveImage.enabled = false).Restart();
        }

        public void OffUI()
        {
            if (!_isPanelOpen) return;

            _upgradeSellPanelTween.PlayBackwards();

            _isPanelOpen = false;
            _curSelectedTower = null;
            _curSupportTower = null;
            _towerRangeIndicator.DisableIndicator();

            if (!_curSummonTower) return;
            _curSummonTower.DeActiveUnitIndicator();
            _curSummonTower = null;
        }

        public bool IsEnoughCost(TowerType towerType)
        {
            if (_towerCost >= GetBuildCost(towerType))
            {
                return true;
            }

            notEnoughCostPanel.gameObject.SetActive(true);
            _notEnoughCostSequence.OnComplete(() => notEnoughCostPanel.gameObject.SetActive(false)).Restart();

            return false;
        }

        public void GameEnd()
        {
            gameEndPanel.SetActive(true);
            resumeButton.interactable = false;
            _pauseSequence.Restart();
            DataManager.SaveLastSurvivedWave();
        }

        #endregion

        #region Private Method

        private async UniTaskVoid CheckCamPos()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(2000, cancellationToken: _cts.Token);
                if (Vector3.Distance(CameraManager.transform.position, Vector3.zero) > 10)
                {
                    if (centerButton.gameObject.activeSelf) continue;
                    centerButton.gameObject.SetActive(true);
                    centerButton.transform.DOScale(1, 0.2f).From(0).SetEase(Ease.OutBack);
                }
            }
        }

        private void ToggleTowerButtons()
        {
            if (_isShowTowerBtn) return;
            toggleTowerButton.SetActive(false);
            _isShowTowerBtn = true;
            _towerCardController.SlideUp();
        }

        private async UniTaskVoid BuildTower(AttackTower t, Vector3 placePos, TowerData towerData)
        {
            var attackTowerData = (AttackTowerData)towerData;
            await DOTween.Sequence().Join(t.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    }));
            t.TowerLevelUp();
            t.TowerSetting(attackTowerData.TowerMeshes[t.TowerLevel], attackTowerData.BaseDamage,
                attackTowerData.AttackRange, attackTowerData.AttackRpm);
            t.OnClickTowerAction += ClickTower;

            _towerManager.AddTower(t);
        }

        private void BuildSupportTower(SupportTower t, Vector3 placePos)
        {
            DOTween.Sequence().Join(t.transform.GetChild(0).DOScale(t.transform.GetChild(0).localScale, 0.25f).From(0)
                    .SetEase(Ease.OutBack))
                .Append(t.transform.GetChild(0).DOMoveY(2, 0.5f).SetEase(Ease.InExpo))
                .OnComplete(() =>
                {
                    PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                    _cam.transform.DOShakePosition(0.05f);
                });
            t.OnClickTowerAction += ClickSupportTower;
            towerMana.towerMana.ManaRegenValue++;
        }

        private void TowerUpgrade()
        {
            var tempTower = _curSelectedTower;
            var towerType = tempTower.TowerType;
            var towerData = TowerDataPrefabDictionary[towerType].towerData;
            var battleTowerData = (AttackTowerData)towerData;

            var upgradeCost = GetUpgradeCost(in towerType);
            if (_towerCost < upgradeCost)
            {
                notEnoughCostPanel.gameObject.SetActive(true);
                _notEnoughCostSequence.OnComplete(() => notEnoughCostPanel.gameObject.SetActive(false)).Restart();
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
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            ResetSprite();
            _isPanelOpen = true;
            _upgradeSellPanelTween.Restart();

            if (_curSupportTower) _curSupportTower = null;

            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_curSummonTower) _curSummonTower.DeActiveUnitIndicator();

            var isUnitTower = TowerDataPrefabDictionary[clickedTower.TowerType].towerData.IsUnitTower;
            if (isUnitTower) _curSummonTower = clickedTower.GetComponent<SummonTower>();

            _curSelectedTower = (AttackTower)clickedTower;

            moveUnitButton.SetActive(isUnitTower);
            upgradeButton.SetActive(!_curSelectedTower.TowerLevel.Equals(4));

            var position = clickedTower.transform.position;
            towerInfoUI.SetInfoUI(position);
            _towerRangeIndicator.SetIndicator(position, _curSelectedTower.TowerRange);
            UpdateTowerInfo();
        }

        private void ClickSupportTower(Tower clickTower)
        {
            if (Input.touchCount != 1) return;
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            ResetSprite();
            _isPanelOpen = true;
            if (_curSelectedTower)
            {
                _towerRangeIndicator.DisableIndicator();
                _curSelectedTower = null;
            }

            if (clickTower.Equals(_curSupportTower)) return;
            _curSupportTower = (SupportTower)clickTower;
            upgradeButton.SetActive(false);
            moveUnitButton.SetActive(false);
            _upgradeSellPanelTween.Restart();
            var position = clickTower.transform.position;
            towerInfoUI.SetInfoUI(position);
            towerInfoUI.SetSupportInfoUI();
            _towerRangeIndicator.SetIndicator(position);
            _sellTowerCost = GetTowerSellCost(clickTower.TowerType);
            towerInfoUI.SetSupportTowerInfo(_curSupportTower, _sellTowerCost);
        }

        private void UpdateTowerInfo()
        {
            var towerType = _curSelectedTower.TowerType;
            var towerLevel = _curSelectedTower.TowerLevel;
            var curTowerData = TowerDataPrefabDictionary[towerType].towerData;
            _sellTowerCost = GetTowerSellCost(towerType);
            towerInfoUI.SetTowerInfo(_curSelectedTower, curTowerData.IsUnitTower,
                towerLevel, GetUpgradeCost(in towerType), _sellTowerCost);
        }

        private ushort GetBuildCost(in TowerType towerType)
        {
            return (ushort)(TowerDataPrefabDictionary[towerType].towerData.TowerBuildCost +
                            TowerDataPrefabDictionary[towerType].towerData.ExtraBuildCost *
                            _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeCost(in TowerType towerType)
        {
            return (ushort)(TowerDataPrefabDictionary[towerType].towerData.TowerUpgradeCost +
                            TowerDataPrefabDictionary[towerType].towerData.ExtraUpgradeCost *
                            _curSelectedTower.TowerLevel);
        }

        private ushort GetTowerSellCost(in TowerType towerType)
        {
            return (ushort)(TowerDataPrefabDictionary[towerType].towerData.TowerBuildCost +
                            TowerDataPrefabDictionary[towerType].towerData.ExtraBuildCost *
                            (_towerCountDictionary[towerType] - 1));
        }

        private void MoveUnitButton()
        {
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            moveUnitController.FocusUnitTower(_curSummonTower);
            _upgradeSellPanelTween.PlayBackwards();
            moveUnitButton.SetActive(false);
        }

        private void SellTower()
        {
            if (_curSelectedTower)
            {
                SoundManager.Instance.PlayUISound(_curSelectedTower.TowerLevel < 2 ? SoundEnum.LowCost :
                    _curSelectedTower.TowerLevel < 4 ? SoundEnum.MediumCost : SoundEnum.HighCost);

                var towerType = _curSelectedTower.TowerType;
                _towerCountDictionary[towerType]--;
                _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "G";
                var position = _curSelectedTower.transform.position;
                PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

                TowerCost += _sellTowerCost;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                    .SetCostText(_sellTowerCost);
                _sellTowerCost = 0;
                _towerManager.RemoveTower(_curSelectedTower);
                _curSelectedTower.DisableObject();
            }
            else
            {
                SoundManager.Instance.PlayUISound(_sellTowerCost < 100 ? SoundEnum.LowCost :
                    _sellTowerCost < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);

                var towerType = _curSupportTower.TowerType;
                _towerCountDictionary[towerType]--;
                _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "G";
                var position = _curSupportTower.transform.position;
                PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

                TowerCost += _sellTowerCost;
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                    .SetCostText(_sellTowerCost);
                _sellTowerCost = 0;
                _curSupportTower.DisableObject();
                towerMana.towerMana.ManaRegenValue--;
            }

            OffUI();
        }

        private void GameStart()
        {
            waveText.text = "0";
            Time.timeScale = 1;
            CameraManager.GameStartCamZoom();
            SoundManager.Instance.SoundManagerInit();
        }

        public void GameOver()
        {
            pausePanelBlockImage.raycastTarget = false;
            resumeButton.interactable = false;
            gameOverPanel.SetActive(true);
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
            pausePanelBlockImage.raycastTarget = false;
            _pauseSequence.Restart();
        }

        private void Resume()
        {
            Input.multiTouchEnabled = true;
            Time.timeScale = _curTimeScale;
            pausePanelBlockImage.raycastTarget = true;
            _pauseSequence.PlayBackwards();
        }

        #endregion
    }
}