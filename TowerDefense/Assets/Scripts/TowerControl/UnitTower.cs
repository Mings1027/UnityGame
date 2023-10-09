using System;
using Cysharp.Threading.Tasks;
using DataControl;
using PoolObjectControl;
using StatusControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private int _damage;
        private float _atkDelay;
        private byte deadUnitCount;
        public Vector3 unitSpawnPosition { get; set; }

        private FriendlyUnit[] _units;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        private void OnDestroy()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                // if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
                // _units[i] = null;
            }
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        protected override void Init()
        {
            base.Init();
            _units = new FriendlyUnit[3];
            deadUnitCount = 0;
        }

        public override void TowerTargetInit()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].TargetInit();
            }
        }

        public override void TowerTargeting()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].UnitTargeting();
            }
        }

        public override void TowerUpdate()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                if (!_units[i].gameObject.activeSelf) continue;
                _units[i].UnitUpdate();
            }
        }

        public override void FingerUp()
        {
            base.FingerUp();
            ActiveUnitIndicator();
        }

        private void ActiveUnitIndicator()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Indicator.enabled = true;
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData, float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            _damage = damageData;
            _atkDelay = attackDelayData;
            if (!_isUnitSpawn)
            {
                UnitSpawn();
            }

            UnitUpgrade(damageData, attackDelayData);
        }

        private void UnitSpawn()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                var angle = i * ((float)Math.PI * 2f) / _units.Length;
                var pos = unitSpawnPosition + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
                PoolObjectManager.Get(PoolObjectKey.UnitSpawnSmoke, pos);
                _units[i] = PoolObjectManager.Get<FriendlyUnit>(TowerData.PoolObjectKey, pos);
                _units[i].Init(this, TowerData.TowerType);
                _units[i].TryGetComponent(out Health health);
                health.OnDeadEvent += UnitReSpawn;
            }

            _isUnitSpawn = true;
        }

        private void UnitUpgrade(int damage, float delay)
        {
            for (var i = 0; i < _units.Length; i++)
            {
                var unitData = (UnitTowerData)TowerData;
                _units[i].UnitUpgrade(damage, unitData.UnitHealth * (1 + TowerLevel), delay);
            }
        }

        public async UniTask StartUnitMove(Vector3 touchPos)
        {
            var tasks = new UniTask[_units.Length - deadUnitCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                _units[i].TryGetComponent(out Health health);
                if (health.IsDead) continue;
                var angle = i * ((float)Math.PI * 2f) / _units.Length;
                var pos = touchPos + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
                tasks[i] = _units[i].MoveToTouchPosTest(pos);
            }

            await UniTask.WhenAll(tasks);
        }

        public void OffUnitIndicator()
        {
            if (!_isUnitSpawn) return;
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Indicator.enabled = false;
            }
        }

        private void UnitReSpawn()
        {
            print("dead");
            deadUnitCount++;
            if (deadUnitCount != _units.Length) return;

            deadUnitCount = 0;
            print("all unit dead");
            _isUnitSpawn = false;
            UnitReSpawnAsync().Forget();
        }

        private async UniTaskVoid UnitReSpawnAsync()
        {
            await UniTask.Delay(5000, delayType: DelayType.DeltaTime);
            if (_isUnitSpawn) return;
            UnitSpawn();
            UnitUpgrade(_damage, _atkDelay);
        }
    }
}