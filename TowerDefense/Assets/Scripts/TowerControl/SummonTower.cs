using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;

namespace TowerControl
{
    public class SummonTower : AttackTower
    {
        private bool _isUnitSpawn;
        private bool _isReSpawning;
        private Vector3 _unitCenterPosition;
        private List<TowerUnit> _units;
        private ReSpawnBar _unitReSpawnBar;
        private Transform _reSpawnBarTransform;
        private SummoningTowerData _summoningTowerData;

        public int unitHealth { get; private set; }
        public float unitReSpawnTime { get; private set; }

        [SerializeField, Range(1, 10)] private byte unitCount;
        [SerializeField, Range(0, 2)] private float unitRadius;
        [SerializeField] private PoolObjectKey unitObjectKey;

#region Unit Control

        private void UnitSpawn()
        {
            NavMesh.SamplePosition(transform.position, out var hit, 5, NavMesh.AllAreas);
            _unitCenterPosition = hit.position;

            for (var i = 0; i < unitCount; i++)
            {
                var angle = i * ((float)Math.PI * 2f) / unitCount;
                var pos = _unitCenterPosition + new Vector3((float)Math.Cos(angle) * unitRadius, 0,
                    (float)Math.Sin(angle) * unitRadius);
                var towerUnit = PoolObjectManager.Get<TowerUnit>(unitObjectKey, transform.position);
                _units.Add(towerUnit);
                _units[i].transform.DOJump(pos, 2, 1, 0.5f).SetEase(Ease.OutSine);
                _units[i].Init(this, pos);
                _units[i].UnitTargetInit();
                _units[i].GetComponent<UnitHealth>().OnDeadEvent += () => DeadEvent(towerUnit);

                var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.UnitHealthBar,
                    _units[i].healthBarTransform.position);
                healthBar.Init(_units[i].GetComponent<Progressive>());

                StatusBarUIController.Add(healthBar, _units[i].healthBarTransform);
            }

            _isUnitSpawn = true;

            if (!_isReSpawning) return;
            _unitReSpawnBar.StopLoading();
            StatusBarUIController.Remove(_reSpawnBarTransform);
        }

        private void UnitUpgrade(int unitDamage, float attackDelay)
        {
            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                _units[i].UnitUpgrade(unitDamage, _summoningTowerData.curUnitHealth * (1 + towerLevel), attackDelay);
            }
        }

        private void DeadEvent(TowerUnit unit)
        {
            StatusBarUIController.Remove(unit.healthBarTransform);

            unit.DisableObject();
            _units.Remove(unit);
            if (_units.Count > 0) return;

            _isUnitSpawn = false;
            if (_isReSpawning) return;
            UnitReSpawnAsync();
        }

        private void UnitReSpawnAsync()
        {
            _isReSpawning = true;
            _unitReSpawnBar =
                PoolObjectManager.Get<ReSpawnBar>(UIPoolObjectKey.ReSpawnBar, _reSpawnBarTransform.position);
            StatusBarUIController.Add(_unitReSpawnBar, _reSpawnBarTransform);
            _unitReSpawnBar.Init();
            _unitReSpawnBar.OnRespawnEvent += UnitReSpawn;
            _unitReSpawnBar.StartLoading(unitReSpawnTime);
        }

        private void UnitReSpawn()
        {
            StatusBarUIController.Remove(_reSpawnBarTransform);

            _isReSpawning = false;

            if (_isUnitSpawn || _isReSpawning) return;
            UnitSpawn();
            UnitUpgrade(towerDamage, attackCooldown.cooldownTime);
        }

        public override void ActiveIndicator()
        {
            base.ActiveIndicator();
            var count = _units.Count;
            for (var i = 0; i < count; i++)
            {
                _units[i].ActiveIndicator();
            }
        }

        public override void DeActiveIndicator()
        {
            base.DeActiveIndicator();
            var count = _units.Count;
            for (var i = 0; i < count; i++)
            {
                _units[i].DeActiveIndicator();
            }
        }

#endregion

#region Override Function

        protected override void Init()
        {
            base.Init();
            _summoningTowerData = (SummoningTowerData)UIManager.towerDataDic[towerType];
            _reSpawnBarTransform = transform.GetChild(1);
            _units = new List<TowerUnit>(unitCount);

            unitHealth = _summoningTowerData.curUnitHealth;
            unitReSpawnTime = _summoningTowerData.initReSpawnTime;
        }

        public override void TowerTargetInit()
        {
            var count = _units.Count - 1;
            for (var i = count; i >= 0; i--)
            {
                _units[i].UnitTargetInit();
            }

            UnitMove(_unitCenterPosition);
        }

        public override void TowerUpdate()
        {
            var count = _units.Count - 1;
            for (var i = count; i >= 0; i--)
            {
                _units[i].UnitUpdate();
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float cooldownData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, cooldownData);

            if (!_isUnitSpawn)
            {
                UnitSpawn();
            }

            UnitUpgrade(damageData, attackCooldown.cooldownTime);
        }

        public override void DisableObject()
        {
            if (_isReSpawning)
            {
                _unitReSpawnBar.StopLoading();
                StatusBarUIController.Remove(_reSpawnBarTransform, true);
            }

            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                StatusBarUIController.Remove(_units[i].healthBarTransform, true);
            }

            for (var i = count - 1; i >= 0; i--)
            {
                if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
                _units[i].DisableObject();
            }

            base.DisableObject();
        }

#endregion

        public void UnitMove(Vector3 touchPos)
        {
            _unitCenterPosition = touchPos;
            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var angle = i * ((float)Math.PI * 2f) / count;
                var pos = touchPos + new Vector3((float)Math.Cos(angle) * unitRadius, 0,
                    (float)Math.Sin(angle) * unitRadius);
                _units[i].Move(pos);
            }

            DeActiveIndicator();
        }
    }
}