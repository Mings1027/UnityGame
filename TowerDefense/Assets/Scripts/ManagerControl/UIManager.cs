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

        private Tower _curSelectedTower;
        private SummonTower _curSummonTower;

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
        private bool _startMoveUnit;
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
        
        public bool enableMoveUnitController => moveUnitController.enabled;
        
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

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Application.targetFrameRate = 60;
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

                TowerUpgrade();
                _upgradeButtonTween.Restart();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(MoveUnitButton);

            sellTowerButton.GetComponent<Button>().onClick.AddListener(() =>
            {
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
            _clickSellBtn = false;
            _sellButtonImage.sprite = sellSprite;
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
                    mapManager.connectionProbability = 25;
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

        public async UniTaskVoid InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var towerObject = Instantiate(TowerDataPrefabDictionary[towerType].towerPrefab, placePos, Quaternion.identity);

            var lostCost = GetBuildCost(towerType);
            _towerCountDictionary[towerType]++;
            TowerCost -= lostCost;

            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetCostText(lostCost, false);
            _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "G";

            var towerTransform = towerObject.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;

            await DOTween.Sequence().Join(towerObject.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(towerObject.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        _cam.transform.DOShakePosition(0.05f);
                    }));

            if (towerObject.TryGetComponent(out AttackTower attackTower))
            {
                var towerData = TowerDataPrefabDictionary[towerType].towerData;
                BuildTower(attackTower, towerData);
            }
            else
            {
                BuildSupportTower(towerObject.GetComponent<SupportTower>());
            }

            towerObject.TryGetComponent(out Tower tower);
            tower.OnClickTowerAction += ClickTower;
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
            pausePanelBlockImage.raycastTarget = false;
            resumeButton.interactable = false;
            gameEndPanel.SetActive(true);
            _pauseSequence.Restart();
            DataManager.SaveLastSurvivedWave();
            Time.timeScale = 0;
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
                    await centerButton.transform.DOScale(1, 0.2f).From(0).SetEase(Ease.OutBack);
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

        private void BuildTower(AttackTower attackTower, TowerData towerData)
        {
            var attackTowerData = (AttackTowerData)towerData;

            attackTower.TowerLevelUp();
            attackTower.TowerSetting(attackTowerData.TowerMeshes[attackTower.TowerLevel], attackTowerData.BaseDamage,
                attackTowerData.AttackRange, attackTowerData.AttackRpm);

            _towerManager.AddTower(attackTower);
        }

        private void BuildSupportTower(SupportTower t)
        {
            towerMana.towerMana.ManaRegenValue++;
        }

        private void TowerUpgrade()
        {
            var attackTower = (AttackTower)_curSelectedTower;
            var towerType = attackTower.TowerType;
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
            var position = attackTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                .SetCostText(upgradeCost, false);

            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

            attackTower.TowerLevelUp();
            var towerLevel = attackTower.TowerLevel;

            attackTower.TowerSetting(battleTowerData.TowerMeshes[towerLevel],
                battleTowerData.BaseDamage * (towerLevel + 1),
                battleTowerData.AttackRange, battleTowerData.AttackRpm);
            upgradeButton.SetActive(!towerLevel.Equals(4));

            _towerRangeIndicator.SetIndicator(position, attackTower.TowerRange);

            _sellTowerCost = GetTowerSellCost(towerType);
            towerInfoUI.SetTowerInfo(attackTower, towerData.IsUnitTower, towerLevel, GetUpgradeCost(in towerType),
                _sellTowerCost);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (_startMoveUnit) return;
            if (Input.touchCount != 1) return;
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            if (_clickSellBtn) ResetSprite();
            _isPanelOpen = true;
            _upgradeSellPanelTween.Restart();

            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_curSelectedTower) _curSelectedTower.DeActiveIndicator();
            if (_curSummonTower) _curSummonTower.DeActiveIndicator();

            var isUnitTower = TowerDataPrefabDictionary[clickedTower.TowerType].towerData.IsUnitTower;
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

            towerInfoUI.SetInfoUI(position);
        }

        private void UpdateAttackTowerInfo(AttackTower attackTower, Vector3 position, bool isUnitTower)
        {
            moveUnitButton.SetActive(isUnitTower);
            upgradeButton.SetActive(!attackTower.TowerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.TowerRange);
            var towerType = _curSelectedTower.TowerType;
            var curTower = (AttackTower)_curSelectedTower;
            var towerLevel = curTower.TowerLevel;
            var curTowerData = TowerDataPrefabDictionary[towerType].towerData;
            _sellTowerCost = GetTowerSellCost(towerType);
            towerInfoUI.SetTowerInfo(curTower, curTowerData.IsUnitTower,
                towerLevel, GetUpgradeCost(in towerType), _sellTowerCost);
        }

        private void UpdateSupportTowerInfo(Tower clickedTower)
        {
            upgradeButton.SetActive(false);
            moveUnitButton.SetActive(false);
            towerInfoUI.SetSupportInfoUI();
            var supportTower = (SupportTower)_curSelectedTower;
            _sellTowerCost = GetTowerSellCost(clickedTower.TowerType);
            towerInfoUI.SetSupportTowerInfo(supportTower, _sellTowerCost);
        }

        private ushort GetBuildCost(in TowerType towerType)
        {
            return (ushort)(TowerDataPrefabDictionary[towerType].towerData.TowerBuildCost +
                            TowerDataPrefabDictionary[towerType].towerData.ExtraBuildCost *
                            _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeCost(in TowerType towerType)
        {
            var curTower = (AttackTower)_curSelectedTower;
            return (ushort)(TowerDataPrefabDictionary[towerType].towerData.TowerUpgradeCost +
                            TowerDataPrefabDictionary[towerType].towerData.ExtraUpgradeCost *
                            curTower.TowerLevel);
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
            _startMoveUnit = true;
        }

        private void SellTower()
        {
            SoundManager.Instance.PlayUISound(_sellTowerCost < 100 ? SoundEnum.LowCost :
                _sellTowerCost < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);

            var towerType = _curSelectedTower.TowerType;
            _towerCountDictionary[towerType]--;
            _towerCostTextDictionary[towerType].text = GetBuildCost(towerType) + "G";
            var position = _curSelectedTower.transform.position;
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);

            TowerCost += _sellTowerCost;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position)
                .SetCostText(_sellTowerCost);
            _sellTowerCost = 0;
            if (_curSelectedTower is AttackTower curTower) _towerManager.RemoveTower(curTower);
            else
            {
                // 현재 supporttower 종류가 manaRecovery 밖에 없기 때문에 TryGetComponent(out SupportTower _) 를 
                // 해도 되지만 여러개가 되면 TryGetComponent(out ManaTower _) 이런식으로 바꿔줘야함 
                if (_curSelectedTower.TryGetComponent(out SupportTower _)) towerMana.towerMana.ManaRegenValue--;
            }

            _curSelectedTower.DisableObject();

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