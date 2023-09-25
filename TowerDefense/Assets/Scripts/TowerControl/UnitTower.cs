using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private int _deadUnitCount;
        private int _damage;
        private float _atkDelay;

        public Vector3 unitSpawnPosition { get; set; }

        private FriendlyUnit[] _units;
        [SerializeField] private int initHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Init()
        {
            base.Init();
            _units = new FriendlyUnit[3];
            _deadUnitCount = 0;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_isUnitSpawn) return;
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].StartTargeting(true);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!_isUnitSpawn) return;
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].StartTargeting(false);
            }
        }

        private void OnDestroy()
        {
            if (!_isUnitSpawn) return;

            _isUnitSpawn = false;

            for (var i = 0; i < _units.Length; i++)
            {
                if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
                _units[i] = null;
            }
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
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

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            _damage = damageData;
            _atkDelay = attackDelayData;
            if (!_isUnitSpawn)
            {
                _isUnitSpawn = true;
                UnitSpawn();
            }

            UnitInfoInit(damageData, attackDelayData);
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].StartTargeting(TowerManager.Instance.StartWave);
            }
        }

        private void UnitSpawn()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                var angle = (float)Math.PI * 0.5f - i * ((float)Math.PI * 2f) / _units.Length;
                var pos = unitSpawnPosition + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
                _units[i] = PoolObjectManager.Get<FriendlyUnit>(TowerData.poolObjectKey, pos);
                PoolObjectManager.Get(PoolObjectKey.UnitSpawnSmoke, pos);
                _units[i].OnReSpawnEvent += UnitReSpawn;
            }
        }

        private void UnitInfoInit(int damage, float delay)
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].InfoInit(this, TowerData.towerType, damage, delay, initHealth + initHealth * TowerLevel);
            }
        }

        public async UniTask StartUnitMove(Vector3 touchPos)
        {
            var tasks = new UniTask[_units.Length - _deadUnitCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                if (!_units[i].gameObject.activeSelf) continue;
                var angle = (float)Math.PI * 0.5f - i * ((float)Math.PI * 2f) / _units.Length;
                var pos = touchPos + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));

                tasks[i] = _units[i].MoveToTouchPos(pos);
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

        private void UnitReSpawn(FriendlyUnit u)
        {
            _deadUnitCount++;
            if (_deadUnitCount < 3) return;
            _deadUnitCount = 0;
            UnitReSpawnDelay();
        }

        private void UnitReSpawnDelay()
        {
            DOVirtual.DelayedCall(5, () =>
            {
                UnitSpawn();
                UnitInfoInit(_damage, _atkDelay);
            }, false);
        }
    }
}