using System;
using System.Collections.Generic;
using CustomEnumControl;
using DataControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using GameControl;
using IndicatorControl;
using InterfaceControl;
using PoolObjectControl;
using TextControl;
using TowerControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class BuildTowerManager : MonoSingleton<BuildTowerManager>, IMainGameObject
    {
        [Serializable]
        public class TowerDataPrefab
        {
            public TowerData towerData;
            public GameObject towerPrefab;
        }

        private Tower _selectedTower;
        private TowerData _towerData;
        private TowerManager _towerManager;
        private TowerInfoCard _towerInfoCard;
        private TowerRangeIndicator _towerRangeIndicator;

        private ushort _sellTowerGold;
        private bool _isSelectTower;

        private Dictionary<TowerType, TowerData> _towerDataDic;
        private Dictionary<TowerType, ushort> _towerCountDictionary;
        private Dictionary<TowerType, GameObject> _towerPrefabDic;

        [SerializeField] private TowerDataPrefab[] towerDataPrefabs;
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;

#region Init

        public void Init()
        {
            TowerInit();
            _towerInfoCard.OnTowerUpgradeEvent += TowerUpgrade;
            _towerInfoCard.OnSellTowerEvent += SellTower;
        }

        private void TowerInit()
        {
            _towerManager = FindAnyObjectByType<TowerManager>();
            _towerInfoCard = FindAnyObjectByType<TowerInfoCard>();
            _towerRangeIndicator = FindAnyObjectByType<TowerRangeIndicator>();
            _towerDataDic = new Dictionary<TowerType, TowerData>();
            _towerPrefabDic = new Dictionary<TowerType, GameObject>();
            _towerCountDictionary = new Dictionary<TowerType, ushort>();
            var towerCardController = FindAnyObjectByType<TowerCardController>();
            towerCardController.Init();
            for (var i = 0; i < towerDataPrefabs.Length; i++)
            {
                _towerDataDic.Add(towerDataPrefabs[i].towerData.towerType, towerDataPrefabs[i].towerData);
                _towerPrefabDic.Add(towerDataPrefabs[i].towerData.towerType, towerDataPrefabs[i].towerPrefab);
                var t = towerDataPrefabs[i].towerData;
                t.InitState();
                _towerCountDictionary.Add(t.towerType, 0);
                UIManager.SetTowerGoldText(t.towerType, _towerDataDic[t.towerType].towerBuildCost + "G");
                towerCardController.SetDictionary(t.towerType, _towerDataDic[t.towerType]);
            }
        }

#endregion

        public static void InstantiateTower(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            instance.InstantiateTowerPrivate(towerType, placePos, towerForward);
        }

        public static bool IsEnoughGold(TowerType towerType)
        {
            return instance.IsEnoughGoldPrivate(towerType);
        }

        public static void DeSelectTower()
        {
            instance.DeSelectedTower();
        }

#region Private Method

        private bool IsEnoughGoldPrivate(TowerType towerType)
        {
            if (GameHUD.GetTowerGold() >= GetBuildGold(towerType))
            {
                return true;
            }

            FloatingNotification.FloatingNotify(FloatingNotifyEnum.NeedMoreGold);

            return false;
        }

        private void InstantiateTowerPrivate(TowerType towerType, Vector3 placePos, Vector3 towerForward)
        {
            var towerObj = Instantiate(_towerPrefabDic[towerType], placePos, Quaternion.identity);
            towerObj.TryGetComponent(out Tower tower);
            tower.Init();
            var lostGold = GetBuildGold(towerType);
            _towerCountDictionary[towerType]++;
            GameHUD.DecreaseTowerGold(lostGold);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.GoldText, placePos).SetGoldText(lostGold, false);
            UIManager.SetTowerGoldText(towerType, GetBuildGold(towerType) + "G");
            var towerTransform = towerObj.transform;
            towerTransform.GetChild(0).position = towerTransform.position + new Vector3(0, 2, 0);
            towerTransform.GetChild(0).forward = towerForward;
            DOTween.Sequence()
                .Append(towerObj.transform.GetChild(0).DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack))
                .Append(towerObj.transform.GetChild(0).DOMoveY(placePos.y, 0.5f).SetEase(Ease.InExpo)
                    .OnComplete(() =>
                    {
                        Camera.main.DOShakePosition(0.05f, strength, vibrato);

                        tower.OnSelectTowerEvent += SelectTower;
                        tower.OnRemoveEvent += _towerManager.RemoveTower;
                        tower.SetTowerData(_towerDataDic[towerType]);
                        tower.LevelUp();
                        _towerManager.AddTower(tower);
                        if (tower.TryGetComponent(out AttackTower attackTower))
                        {
                            var atkTowerData = (AttackTowerData)_towerDataDic[towerType];
                            attackTower.TowerSetting(atkTowerData.curDamage, atkTowerData.attackCooldown,
                                atkTowerData.towerMeshes[tower.towerLevel - 1]);
                        }
                        else if (tower.TryGetComponent(out SupportTower supportTower))
                        {
                            var supportTowerData = (SupportTowerData)_towerDataDic[towerType];
                            supportTower.TowerSetting(supportTowerData.towerUpdateCooldown);
                        }
                    }));
        }

        private void SelectTower(Tower tower, TowerData towerData)
        {
            if (_towerInfoCard.startMoveUnit) return;
            if (Input.touchCount != 1) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (tower.Equals(_selectedTower)) return;
            _isSelectTower = true;
            if (_selectedTower) _selectedTower.DeActiveIndicator();
            _selectedTower = tower;
            _selectedTower.ActiveIndicator();
            _towerData = towerData;
            var towerType = towerData.towerType;
            _towerRangeIndicator.SetIndicator(tower.transform.position, towerData.curRange);
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoCard.SetTowerInfo(tower, towerData, tower.towerLevel, GetUpgradeGold(in towerType),
                _sellTowerGold, TowerDataManager.TowerInfoTable[towerType].towerName);

            _towerInfoCard.OpenCard();
            _towerInfoCard.SetCardPos(true, _selectedTower.transform);
        }

        private void DeSelectedTower()
        {
            if (!_isSelectTower) return;
            _isSelectTower = false;
            _towerInfoCard.SetCardPos(false, null);
            _towerInfoCard.CloseCard();
            if (_selectedTower) _selectedTower.DeActiveIndicator();
            _selectedTower = null;
            _towerRangeIndicator.DisableIndicator();
        }

        private void TowerUpgrade()
        {
            var towerType = _towerData.towerType;
            var upgradeGold = GetUpgradeGold(in towerType);

            if (GameHUD.GetTowerGold() < upgradeGold)
            {
                FloatingNotification.FloatingNotify(FloatingNotifyEnum.NeedMoreGold);
                return;
            }

            GameHUD.DecreaseTowerGold(upgradeGold);
            var position = _selectedTower.transform.position;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.GoldText, position).SetGoldText(upgradeGold, false);
            PoolObjectManager.Get(PoolObjectKey.TowerUpgradeParticle, position);
            _selectedTower.LevelUp();
            var towerLevel = _selectedTower.towerLevel;
            if (_selectedTower.TryGetComponent(out AttackTower attackTower))
            {
                var atkTowerData = (AttackTowerData)_towerDataDic[towerType];
                attackTower.TowerSetting(atkTowerData.curDamage * towerLevel, atkTowerData.attackCooldown,
                    atkTowerData.towerMeshes[towerLevel - 1]);
            }

            _towerRangeIndicator.SetIndicator(position, _towerData.curRange);
            _sellTowerGold = GetTowerSellGold(towerType);
            _towerInfoCard.SetTowerInfo(_selectedTower, _towerData, towerLevel, GetUpgradeGold(in towerType),
                _sellTowerGold, TowerDataManager.TowerInfoTable[towerType].towerName);
        }

        private void SellTower()
        {
            _towerCountDictionary[_towerData.towerType]--;
            _towerInfoCard.SetCardPos(false, null);
            SoundManager.PlayUISound(_sellTowerGold < 100 ? SoundEnum.LowCost :
                _sellTowerGold < 250 ? SoundEnum.MediumCost : SoundEnum.HighCost);
            var towerType = _towerData.towerType;
            UIManager.SetTowerGoldText(towerType, GetBuildGold(towerType) + "G");
            var position = _selectedTower.transform.position;
            PoolObjectManager.Get(PoolObjectKey.TowerSellParticle, position);
            GameHUD.IncreaseTowerGold(_sellTowerGold);
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.GoldText, position).SetGoldText(_sellTowerGold);
            _sellTowerGold = 0;
            _selectedTower.DisableObject();
        }

        private ushort GetBuildGold(in TowerType towerType)
        {
            return (ushort)(_towerDataDic[towerType].towerBuildCost +
                            _towerDataDic[towerType].extraBuildCost *
                            _towerCountDictionary[towerType]);
        }

        private ushort GetUpgradeGold(in TowerType towerType)
        {
            return (ushort)(_towerDataDic[towerType].towerUpgradeCost +
                            _towerDataDic[towerType].extraUpgradeCost * _selectedTower.towerLevel);
        }

        private ushort GetTowerSellGold(in TowerType towerType)
        {
            return (ushort)(_towerDataDic[towerType].towerBuildCost +
                            _towerDataDic[towerType].extraBuildCost *
                            (_towerCountDictionary[towerType] - 1));
        }

#endregion
    }
}