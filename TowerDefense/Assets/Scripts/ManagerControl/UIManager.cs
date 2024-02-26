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
using InterfaceControl;
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
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace ManagerControl
{
    public class UIManager : MonoSingleton<UIManager>, IAddressableObject
    {
        [Serializable]
        public class TowerDataPrefab
        {
            public TowerData towerData;
            public GameObject towerPrefab;
        }

#region Private Variable

        private TowerManager _towerManager;
        private TowerCardController _towerCardController;

        private CancellationTokenSource _cts;
        private Camera _cam;

        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerGoldTextDictionary;
        private Dictionary<TowerType, GameObject> _towerPrefabDic;

        private Tween _infoGroupTween;

        private Sequence _needMoreGoldSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;

        private Sequence _towerHealTween;

        private Tower _curSelectedTower;
        private SummonTower _curSummonTower;

        private Image _toggleTowerBtnImage;

        private TMP_Text[] _towerGoldTexts;

        private TowerRangeIndicator _towerRangeIndicator;

        private Image _sellButtonImage;

        private ushort _sellTowerGold;

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

        private TowerDescriptionCard _towerDescriptionCard;

        private RectTransform _towerCardPanel;
        private CanvasGroup _needMoreGoldGroup;
        private CanvasGroup _towerInfoGroup;

        private MoveUnitController _moveUnitController;

        [SerializeField] private TowerDataPrefab[] towerDataPrefabs;

#endregion

#region Property

        public static Dictionary<TowerType, TowerData> towerDataDic { get; private set; }
        public static Dictionary<TowerType, string> towerNameDic { get; private set; }
        public static Dictionary<TowerType, string> towerInfoDic { get; private set; }
        public static bool enableMoveUnitController => instance._moveUnitController.enabled;

#endregion

#region Unity Event

        protected override void Awake()
        {
            instance = this;
        }

        private void OnDisable()
        {
            _infoGroupTween?.Kill();
            _needMoreGoldSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _towerHealTween?.Kill();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnDestroy()
        {
            instance = null;
        }

#endregion

#region Init

        public void Init()
        {
            UIManagerInit();

            _towerHealthRect = _gameHUD.towerHealth.GetComponent<RectTransform>();

            TowerButtonInit();
            TowerInit();
            LocaleDictionaryInit();
            TweenInit();
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleTowerDictionary;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            GameStart();
            SoundManager.PlayBGM(SoundEnum.WaveEnd);
            SoundManager.FadeInVolume(SoundManager.BGMKey).Forget();
        }

        private void UIManagerInit()
        {
            Input.multiTouchEnabled = false;
            _cam = Camera.main;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _towerRangeIndicator = FindAnyObjectByType<TowerRangeIndicator>();
            _gameHUD = FindAnyObjectByType<GameHUD>();
            _towerInfoUI = FindAnyObjectByType<TowerInfoUI>();
            _gameHUD.Init();
            _towerInfoUI.Init();
            _towerManager = FindAnyObjectByType<TowerManager>();
            _towerManager.TowerManaInit(GetTowerMana());

            var uiPanel = transform.Find("UI Panel");
            var followTowerInfoUI = _towerInfoUI.transform.GetChild(0);
            _upgradeButton = followTowerInfoUI.Find("Upgrade Button").gameObject;
            _sellTowerButton = followTowerInfoUI.Find("Sell Tower Button").gameObject;
            _moveUnitButton = followTowerInfoUI.Find("Move Unit Button").gameObject;

            _towerDescriptionCard = FindAnyObjectByType<TowerDescriptionCard>();
            _towerCardPanel = uiPanel.Find("Tower Card Panel").GetChild(0).GetComponent<RectTransform>();
            _needMoreGoldGroup = uiPanel.Find("Need More Gold").GetComponent<CanvasGroup>();
            _toggleTowerBtnImage = uiPanel.Find("Toggle Tower Button").GetComponent<Image>();
            _moveUnitController = FindAnyObjectByType<MoveUnitController>();
        }

        private void TowerButtonInit()
        {
            _towerCardController = _towerCardPanel.GetComponentInChildren<TowerCardController>();
            _towerCardController.Init();

            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                _towerCardController.SetDictionary(towerDataPrefabs[i].towerData.towerType,
                    towerDataPrefabs[i].towerData);
            }

            var towerButtons = _towerCardController.GetComponentsInChildren<TowerButton>();
            _towerGoldTexts = new TMP_Text[towerButtons.Length];

            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerGoldTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }

            _toggleTowerBtnImage.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                ToggleTowerButtons();
            });
            _upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);

                if (_clickSellBtn) ResetSprite();

                TowerUpgrade();
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
            _sellButtonImage = _sellTowerButton.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        }

        private void TowerInit()
        {
            towerDataDic = new Dictionary<TowerType, TowerData>();
            _towerPrefabDic = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, ushort>();
            _towerGoldTextDictionary = new Dictionary<TowerType, TMP_Text>();

            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                towerDataDic.Add(towerDataPrefabs[i].towerData.towerType, towerDataPrefabs[i].towerData);
                _towerPrefabDic.Add(towerDataPrefabs[i].towerData.towerType, towerDataPrefabs[i].towerPrefab);
                var t = towerDataPrefabs[i].towerData;
                t.InitState();
                _towerCountDictionary.Add(t.towerType, 0);
                var towerType = towerDataPrefabs[i].towerData.towerType;
                _towerGoldTextDictionary.Add(towerType, _towerGoldTexts[i]);
                _towerGoldTextDictionary[towerType].text =
                    towerDataDic[towerType].towerBuildCost + "G";
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
            _towerInfoGroup = _towerInfoUI.GetComponent<CanvasGroup>();
            _infoGroupTween = _towerInfoGroup.DOFade(1, 0.15f).From(0).SetAutoKill(false).Pause();
            _towerInfoGroup.blocksRaycasts = false;

            var needMoreGoldPanelRect = _needMoreGoldGroup.GetComponent<RectTransform>();
            _needMoreGoldSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_needMoreGoldGroup.DOFade(1, 0.5f).From(0))
                .Join(needMoreGoldPanelRect.DOAnchorPosY(0, 0.25f).From(new Vector2(0, 100)))
                .Append(needMoreGoldPanelRect.DOAnchorPosY(100, 0.25f).From(Vector2.zero).SetDelay(1f))
                .Join(_needMoreGoldGroup.DOFade(0, 0.25f).From(1));
            _needMoreGoldGroup.blocksRaycasts = false;
            _needMoreGoldGroup.alpha = 0;

            _towerHealTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_towerHealthRect.DOScale(1.2f, 0.125f))
                .Append(_towerHealthRect.DOScale(1, 0.125f));
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

        public static async UniTaskVoid MapSelectButton(byte difficultyLevel)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            mapManager.MakeMap(difficultyLevel);

            instance._gameHUD.towerGold = difficultyLevel switch
            {
                0 => 2000,
                1 => 4000,
                2 => 5000,
                3 => 6000,
                _ => instance._gameHUD.towerGold
            };

            BackendGameData.instance.SetLevel(difficultyLevel);
            await UniTask.Delay(500, cancellationToken: instance._cts.Token);
            instance._gameHUD.DisplayHUD();
            await instance._towerCardPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            instance._toggleTowerBtnImage.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
        }

        public static Sprite GetTowerType(TowerType t) =>
            towerDataDic[t].isMagicTower
                ? instance._gameHUD.magicSprite
                : instance._gameHUD.physicalSprite;

        public static void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var towerObject = Instantiate(instance._towerPrefabDic[towerType], placePos,
                Quaternion.identity);
            var lostGold = instance.GetBuildGold(towerType);
            instance._towerCountDictionary[towerType]++;
            instance._gameHUD.towerGold -= lostGold;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetGoldText(lostGold, false);
            instance._towerGoldTextDictionary[towerType].text = instance.GetBuildGold(towerType) + "G";
            var towerTransform = towerObject.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;
            DOTween.Sequence()
                .Join(towerObject.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(towerObject.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        PoolObjectManager.Get(PoolObjectKey.BuildSmoke, placePos);
                        instance._cam.transform.DOShakePosition(0.05f);

                        if (towerObject.TryGetComponent(out AttackTower attackTower))
                        {
                            var towerData = towerDataDic[towerType];
                            instance.BuildTower(attackTower, towerData);
                        }
                        else if (towerObject.TryGetComponent(out SupportTower supportTower))
                        {
                            instance.BuildSupportTower(supportTower);
                        }

                        towerObject.TryGetComponent(out Tower tower);
                        tower.OnClickTowerAction += instance.ClickTower;
                    }));
        }

        public static void UIOff()
        {
            OffUI();

            if (!instance._isShowTowerBtn)
            {
                instance._toggleTowerBtnImage.DOFade(1, 0.2f)
                    .OnComplete(() => instance._toggleTowerBtnImage.raycastTarget = true);
            }

            instance._gameHUD.DisplayHUD();
        }

        public static void OffUI()
        {
            if (!instance._isPanelOpen) return;
            instance._towerInfoGroup.blocksRaycasts = false;
            instance._infoGroupTween.PlayBackwards();
            instance._startMoveUnit = false;
            instance._isPanelOpen = false;
            if (instance._curSelectedTower) instance._curSelectedTower.DeActiveIndicator();
            instance._curSelectedTower = null;
            instance._towerRangeIndicator.DisableIndicator();

            if (!instance._curSummonTower) return;
            instance._curSummonTower = null;
        }

        public static void ShowTowerButton() => instance._isShowTowerBtn = true;

        public static async UniTaskVoid SlideDown()
        {
            instance._isShowTowerBtn = false;
            await UniTask.Delay(2000, cancellationToken: instance._cts.Token);

            if (!instance._isShowTowerBtn)
            {
                await instance._toggleTowerBtnImage.DOFade(0, 1);
                instance._toggleTowerBtnImage.raycastTarget = false;
            }
        }

        public static void UnitCannotMove() => instance._gameHUD.CannotMove();

        public static TowerHealth GetTowerHealth() => instance._gameHUD.towerHealth;

        public static void TowerHeal()
        {
            instance._towerHealTween.Restart();
            instance._gameHUD.towerHealth.Heal(5);
        }

        public static Mana GetTowerMana() => instance._gameHUD.towerMana;

        public static bool IsEnoughGold(TowerType towerType)
        {
            if (instance._gameHUD.towerGold >= instance.GetBuildGold(towerType))
            {
                return true;
            }

            instance._needMoreGoldSequence.Restart();

            return false;
        }

        public static void RemoveAttackTower(AttackTower attackTower)
        {
            instance._towerManager.RemoveTower(attackTower);
        }

        public static void RemoveManaTower()
        {
            instance._gameHUD.towerMana.manaRegenValue -= 2;
        }

        public static void BuildManaTower()
        {
            instance._gameHUD.towerMana.manaRegenValue += 2;
        }

        public static void SetTowerGold(int gold)
        {
            instance._gameHUD.towerGold += gold;
        }

        public static void SetWaveText(string wave)
        {
            instance._gameHUD.waveText.text = wave;
        }

#endregion

#region Private Method

        private void ToggleTowerButtons()
        {
            if (_isShowTowerBtn) return;
            _isShowTowerBtn = true;
            _towerCardController.SlideUp();
        }

        private void BuildTower(AttackTower attackTower, TowerData towerData)
        {
            var attackTowerData = (AttackTowerData)towerData;
            attackTower.TowerLevelUp();
            attackTower.TowerSetting(attackTowerData.towerMeshes[attackTower.towerLevel], attackTowerData.curDamage,
                attackTowerData.curRange, attackTowerData.attackCooldown);
            _towerManager.AddTower(attackTower);
        }

        private void BuildSupportTower(SupportTower supportTower)
        {
            supportTower.TowerSetting();
        }

        private void TowerUpgrade()
        {
            var attackTower = (AttackTower)_curSelectedTower;
            var towerType = attackTower.towerType;
            var towerData = towerDataDic[towerType];
            var battleTowerData = (AttackTowerData)towerData;
            var upgradeGold = GetUpgradeGold(in towerType);

            if (_gameHUD.towerGold < upgradeGold)
            {
                _needMoreGoldSequence.Restart();
                return;
            }

            _gameHUD.towerGold -= upgradeGold;
            var position = attackTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(upgradeGold, false);
            PoolObjectManager.Get(PoolObjectKey.BuildSmoke, position);
            attackTower.TowerLevelUp();
            var towerLevel = attackTower.towerLevel;
            attackTower.TowerSetting(battleTowerData.towerMeshes[towerLevel],
                battleTowerData.curDamage * (towerLevel + 1), battleTowerData.curRange, battleTowerData.attackCooldown);
            _upgradeButton.SetActive(!towerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.towerRange);
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoUI.SetTowerInfo(attackTower, towerData, towerLevel,
                GetUpgradeGold(in towerType), _sellTowerGold, towerNameDic[towerType]);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (_startMoveUnit) return;
            if (Input.touchCount != 1) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _isPanelOpen = true;

            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_clickSellBtn) ResetSprite();
            _infoGroupTween.OnComplete(() => _towerInfoGroup.blocksRaycasts = true).Restart();
            if (_curSelectedTower) _curSelectedTower.DeActiveIndicator();
            if (_curSummonTower) _curSummonTower.DeActiveIndicator();
            var isUnitTower = towerDataDic[clickedTower.towerType].isUnitTower;
            if (isUnitTower) _curSummonTower = clickedTower.GetComponent<SummonTower>();
            _curSelectedTower = clickedTower;
            _curSelectedTower.ActiveIndicator();
            var position = clickedTower.transform.position;

            _sellTowerGold = GetTowerSellGold(clickedTower.towerType);

            if (_curSelectedTower is AttackTower curTower)
            {
                UpdateAttackTowerInfo(curTower, position, isUnitTower);
            }
            else
            {
                _towerRangeIndicator.DisableIndicator();
                UpdateSupportTowerInfo();
            }

            _towerInfoUI.SetInfoUI(position);
        }

        private void UpdateAttackTowerInfo(AttackTower attackTower, Vector3 position, bool isUnitTower)
        {
            _moveUnitButton.SetActive(isUnitTower);
            _upgradeButton.SetActive(!attackTower.towerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.towerRange);
            var towerType = attackTower.towerType;
            var towerLevel = attackTower.towerLevel;
            var curTowerData = towerDataDic[towerType];
            _towerInfoUI.SetTowerInfo(attackTower, curTowerData, towerLevel,
                GetUpgradeGold(in towerType), _sellTowerGold, towerNameDic[towerType]);
        }

        private void UpdateSupportTowerInfo()
        {
            _upgradeButton.SetActive(false);
            _moveUnitButton.SetActive(false);
            var supportTower = (SupportTower)_curSelectedTower;
            _towerInfoUI.SetSupportTowerInfo(supportTower, _sellTowerGold, towerNameDic[supportTower.towerType]);
        }

        private ushort GetBuildGold(in TowerType towerType)
        {
            return (ushort)(towerDataDic[towerType].towerBuildCost +
                            towerDataDic[towerType].extraBuildCost *
                            _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeGold(in TowerType towerType)
        {
            var curTower = (AttackTower)_curSelectedTower;

            return (ushort)(towerDataDic[towerType].towerUpgradeCost +
                            towerDataDic[towerType].extraUpgradeCost * curTower.towerLevel);
        }

        private ushort GetTowerSellGold(in TowerType towerType)
        {
            return (ushort)(towerDataDic[towerType].towerBuildCost +
                            towerDataDic[towerType].extraBuildCost *
                            (_towerCountDictionary[towerType] - 1));
        }

        private void MoveUnitButton()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _moveUnitController.FocusUnitTower(_curSummonTower);
            _towerInfoGroup.blocksRaycasts = false;
            _infoGroupTween.PlayBackwards();
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
            _gameHUD.towerGold += _sellTowerGold;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(_sellTowerGold);
            _sellTowerGold = 0;

            _curSelectedTower.DisableObject();
            OffUI();
        }

        private void GameStart()
        {
            _gameHUD.waveText.text = "0";
            Time.timeScale = 1;
        }

#endregion
    }
}