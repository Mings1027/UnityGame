using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using GameControl;
using IndicatorControl;
using InterfaceControl;
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
using UnityEngine.UI;
using Utilities;
using Sequence = DG.Tweening.Sequence;

namespace ManagerControl
{
    public class UIManager : MonoSingleton<UIManager>, IMainGameObject
    {
        [Serializable]
        public class TowerDataPrefab
        {
            public TowerData towerData;
            public GameObject towerPrefab;
        }

#region Private Variable

        private DataManager _dataManager;
        private TowerManager _towerManager;
        private TowerCardController _towerCardController;

        private CancellationTokenSource _cts;
        private Camera _cam;

        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, TMP_Text> _towerGoldTextDictionary;
        private Dictionary<TowerType, GameObject> _towerPrefabDic;

        private Sequence _needMoreGoldSequence;
        private Sequence _cantMoveImageSequence;
        private Sequence _camZoomSequence;

        private Sequence _towerHealTween;

        private Tower _curSelectedTower;
        private SummonTower _curSummonTower;

        private TMP_Text[] _towerGoldTexts;

        private TowerRangeIndicator _towerRangeIndicator;

        private Image _sellButtonImage;

        private ushort _sellTowerGold;

        private bool _isPanelOpen;
        private bool _startMoveUnit;
        private bool _clickSellBtn;

        private Vector3 _prevPos;

        private TowerInfoUI _towerInfoUI;
        private TowerDescriptionCard _towerDescriptionCard;
        private MoveUnitController _moveUnitController;

        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject maxLevelImage;
        [SerializeField] private GameObject sellTowerButton;
        [SerializeField] private GameObject moveUnitButton;

        [SerializeField] private CanvasGroup toggleTowerBtnImage;
        [SerializeField] private RectTransform towerCardPanel;

        [SerializeField] private CanvasGroup needMoreGoldGroup;

        [SerializeField] private TowerDataPrefab[] towerDataPrefabs;
        [SerializeField] private Sprite sellSprite;
        [SerializeField] private Sprite checkSprite;

#endregion

#region Property

        public static Dictionary<TowerType, TowerData> towerDataDic { get; private set; }

        public static bool enableMoveUnitController { get; set; }

#endregion

#region Unity Event

        private void OnDisable()
        {
            _needMoreGoldSequence?.Kill();
            _cantMoveImageSequence?.Kill();
            _camZoomSequence?.Kill();
            _towerHealTween?.Kill();
            _cts?.Cancel();
            _cts?.Dispose();
        }

#endregion

#region Init

        public void Init()
        {
            UIManagerInit();
            TowerButtonInit();
            TowerInit();
            TweenInit();

            Input.multiTouchEnabled = false;
            GameHUD.SetWaveText("0");
            Time.timeScale = 1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SoundManager.PlayBGM(SoundEnum.WaveEnd);
            SoundManager.FadeInVolume().Forget();
        }

        private void UIManagerInit()
        {
            _cam = Camera.main;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _towerRangeIndicator = FindAnyObjectByType<TowerRangeIndicator>();
            FindAnyObjectByType<GameHUD>().Init();
            _towerInfoUI = FindAnyObjectByType<TowerInfoUI>();
            _towerInfoUI.Init();
            _dataManager = FindAnyObjectByType<DataManager>();
            _towerManager = FindAnyObjectByType<TowerManager>();
            _towerDescriptionCard = FindAnyObjectByType<TowerDescriptionCard>();
            _moveUnitController = FindAnyObjectByType<MoveUnitController>();
        }

        private void TowerButtonInit()
        {
            _towerCardController = towerCardPanel.GetComponentInChildren<TowerCardController>();
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

            upgradeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);

                if (_clickSellBtn) ResetSprite();

                TowerUpgrade();
            });
            moveUnitButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                MoveUnitButton();
            });
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
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    _clickSellBtn = true;
                    _sellButtonImage.sprite = checkSprite;
                }
            });
            _sellButtonImage = sellTowerButton.transform.GetChild(0).GetChild(0).GetComponent<Image>();
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

        private void TweenInit()
        {
            var needMoreGoldPanelRect = needMoreGoldGroup.GetComponent<RectTransform>();
            _needMoreGoldSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(needMoreGoldGroup.DOFade(1, 0.5f).From(0))
                .Join(needMoreGoldPanelRect.DOAnchorPosY(0, 0.25f).From(new Vector2(0, 100)))
                .Append(needMoreGoldPanelRect.DOAnchorPosY(100, 0.25f).From(Vector2.zero).SetDelay(1f))
                .Join(needMoreGoldGroup.DOFade(0, 0.25f).From(1));
            needMoreGoldGroup.blocksRaycasts = false;
            needMoreGoldGroup.alpha = 0;
        }

        private void ResetSprite()
        {
            _clickSellBtn = false;
            _sellButtonImage.sprite = sellSprite;
        }

#endregion

#region Public Method

        public static void MapSelectButton(byte difficultyLevel)
        {
            instance.MapSelectButtonPrivate(difficultyLevel).Forget();
        }

        public static void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            instance.InstantiateTowerPrivate(towerType, placePos, towerForward);
        }

        public static void UIOff()
        {
            instance.OffUIPrivate();
            instance.AppearUI();
        }

        public static void OffUI()
        {
            instance.OffUIPrivate();
        }

        public static bool IsEnoughGold(TowerType towerType)
        {
            return instance.IsEnoughGoldPrivate(towerType);
        }

        public static void RemoveAttackTower(AttackTower attackTower)
        {
            instance._towerManager.RemoveTower(attackTower);
        }

#endregion

#region Private Method

        private async UniTaskVoid MapSelectButtonPrivate(byte difficultyLevel)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            mapManager.MakeMap(difficultyLevel);
            GameHUD.SetTowerGold(difficultyLevel);
            BackendGameData.instance.SetLevel(difficultyLevel);
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            GameHUD.DisplayHUD();
            await towerCardPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
            toggleTowerBtnImage.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
        }

        private void InstantiateTowerPrivate(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var towerObject = Instantiate(_towerPrefabDic[towerType], placePos,
                Quaternion.identity);
            var lostGold = GetBuildGold(towerType);
            _towerCountDictionary[towerType]++;
            GameHUD.DecreaseTowerGold(lostGold);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, placePos).SetGoldText(lostGold, false);
            _towerGoldTextDictionary[towerType].text = GetBuildGold(towerType) + "G";
            var towerTransform = towerObject.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;
            DOTween.Sequence()
                .Append(towerObject.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(towerObject.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        _cam.transform.DOShakePosition(0.05f);

                        if (towerObject.TryGetComponent(out AttackTower attackTower))
                        {
                            var towerData = towerDataDic[towerType];
                            BuildTower(attackTower, towerData);
                        }
                        else if (towerObject.TryGetComponent(out SupportTower supportTower))
                        {
                            BuildSupportTower(supportTower);
                        }

                        towerObject.TryGetComponent(out Tower tower);
                        tower.OnClickTowerAction += ClickTower;
                    }));
        }

        private void AppearUI()
        {
            _towerCardController.AppearToggleButton();
            GameHUD.DisplayHUD();
        }

        private void OffUIPrivate()
        {
            if (!_isPanelOpen) return;
            _towerInfoUI.SetCardPos(false, null);
            _towerInfoUI.CloseCard();
            _startMoveUnit = false;
            _isPanelOpen = false;
            if (_curSelectedTower) _curSelectedTower.DeActiveIndicator();
            _curSelectedTower = null;
            _towerRangeIndicator.DisableIndicator();

            if (!_curSummonTower) return;
            _curSummonTower = null;
        }

        private bool IsEnoughGoldPrivate(TowerType towerType)
        {
            if (GameHUD.GetTowerGold() >= GetBuildGold(towerType))
            {
                return true;
            }

            _needMoreGoldSequence.Restart();

            return false;
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
            var atkTowerData = (AttackTowerData)towerData;
            var upgradeGold = GetUpgradeGold(in towerType);

            if (GameHUD.GetTowerGold() < upgradeGold)
            {
                _needMoreGoldSequence.Restart();
                return;
            }

            GameHUD.DecreaseTowerGold(upgradeGold);
            var position = attackTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(upgradeGold, false);
            PoolObjectManager.Get(PoolObjectKey.TowerUpgradeParticle, position);
            attackTower.TowerLevelUp();
            var towerLevel = attackTower.towerLevel;
            attackTower.TowerSetting(atkTowerData.towerMeshes[towerLevel],
                atkTowerData.curDamage * (towerLevel + 1), atkTowerData.curRange, atkTowerData.attackCooldown);
            upgradeButton.SetActive(!towerLevel.Equals(4));
            maxLevelImage.SetActive(towerLevel.Equals(4));
            _towerRangeIndicator.SetIndicator(position, attackTower.towerRange);
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoUI.SetTowerInfo(attackTower, towerData, towerLevel,
                GetUpgradeGold(in towerType), _sellTowerGold, _dataManager.towerInfoTable[towerType].towerName);
        }

        private void ClickTower(Tower clickedTower)
        {
            if (_startMoveUnit) return;
            if (Input.touchCount != 1) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _isPanelOpen = true;

            if (clickedTower.Equals(_curSelectedTower)) return;

            if (_clickSellBtn) ResetSprite();
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

            _towerInfoUI.OpenCard();
            _towerInfoUI.SetCardPos(true, clickedTower.transform);
        }

        private void UpdateAttackTowerInfo(AttackTower attackTower, Vector3 position, bool isUnitTower)
        {
            moveUnitButton.SetActive(isUnitTower);
            var towerLevel = attackTower.towerLevel;

            upgradeButton.SetActive(!towerLevel.Equals(4));
            maxLevelImage.SetActive(towerLevel.Equals(4));

            _towerRangeIndicator.SetIndicator(position, attackTower.towerRange);
            var towerType = attackTower.towerType;
            var curTowerData = towerDataDic[towerType];
            _towerInfoUI.SetTowerInfo(attackTower, curTowerData, towerLevel,
                GetUpgradeGold(in towerType), _sellTowerGold, _dataManager.towerInfoTable[towerType].towerName);
        }

        private void UpdateSupportTowerInfo()
        {
            upgradeButton.SetActive(false);
            maxLevelImage.SetActive(false);
            moveUnitButton.SetActive(false);
            var supportTower = (SupportTower)_curSelectedTower;
            _towerInfoUI.SetSupportTowerInfo(supportTower, _sellTowerGold,
                _dataManager.towerInfoTable[supportTower.towerType].towerName);
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
            _moveUnitController.FocusUnitTower(_curSummonTower);
            _towerInfoUI.SetCardPos(false, null);
            _towerInfoUI.CloseCard();
            _startMoveUnit = true;
        }

        private void SellTower()
        {
            _towerInfoUI.SetCardPos(false, null);
            SoundManager.PlayUISound(_sellTowerGold < 100 ? SoundEnum.LowCost :
                _sellTowerGold < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);
            var towerType = _curSelectedTower.towerType;
            _towerCountDictionary[towerType]--;
            _towerGoldTextDictionary[towerType].text = GetBuildGold(towerType) + "G";
            var position = _curSelectedTower.transform.position;
            PoolObjectManager.Get(PoolObjectKey.TowerSellParticle, position);
            GameHUD.IncreaseTowerGold(_sellTowerGold);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, position).SetGoldText(_sellTowerGold);
            _sellTowerGold = 0;

            _curSelectedTower.DisableObject();
            OffUI();
        }

#endregion
    }
}