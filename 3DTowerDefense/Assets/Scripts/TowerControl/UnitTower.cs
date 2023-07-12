using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using UnitControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class UnitTower : Tower
    {
        private int _deadUnitCount;
        private bool _unitIsFull;
        private CancellationTokenSource _cts;

        private FriendlyUnit[] _units;
        private Vector3 _unitSpawnPosition;

        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private int unitCount;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            UnitDisable(_units);
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Init()
        {
            base.Init();
            _units = new FriendlyUnit[unitCount];
        }

        public override void BuildTowerWithDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage,
            float attackRange,
            float attackDelay, float health = 0)
        {
            base.BuildTowerWithDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            if (TowerLevel == 0)
            {
                SpawnUnitOnTowerSpawn(minDamage, maxDamage, attackDelay, health);
            }
            else if (IsUniqueTower)
            {
                SpawnUniqueUnit(minDamage, maxDamage, attackDelay, health);
            }
        }

        public override void BuildTower(MeshFilter towerMeshFilter)
        {
            base.BuildTower(towerMeshFilter);
            if (TowerLevel != 0) return;
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }

        private void SpawnUnitOnTowerSpawn(int minDamage, int maxDamage, float delay, float health)
        {
            // Call Only once when tower spawn

            _unitSpawnPosition = transform.position - transform.forward * 10;
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i] = UnitSpawn(PoolObjectName.PuddingJellyUnit, transform.position);
                _units[i].Init(minDamage, maxDamage, delay, health);
            }

            _unitIsFull = true;
        }

        private void SpawnUniqueUnit(int minDamage, int maxDamage, float delay, float health)
        {
            if (_unitIsFull)
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    _units[i].gameObject.SetActive(false);
                    var prevUnitPos = _units[i].transform.position;
                    _units[i] = UnitSpawn(PoolObjectName.BearJellyUnit, prevUnitPos);
                    _units[i].Init(minDamage, maxDamage, delay, health);
                }
            }
            else
            {
                for (int i = 0; i < _units.Length; i++)
                {
                    _units[i] = UnitSpawn(PoolObjectName.BearJellyUnit, transform.position);
                    _units[i].Init(minDamage, maxDamage, delay, health);
                }

                UnitMove(_unitSpawnPosition);
                _unitIsFull = true;
            }
        }

        private FriendlyUnit UnitSpawn(string unitName, Vector3 pos)
        {
            var unit = ObjectPoolManager.Get<BarracksUnit>(unitName, pos);
            unit.OnDeadEvent += UnitReSpawn;
            return unit;
        }

        private void UnitDisable(IList<FriendlyUnit> u)
        {
            for (var i = 0; i < u.Count; i++)
            {
                if (u[i] == null || !u[i].gameObject.activeSelf) continue;
                u[i].gameObject.SetActive(false);
                u[i] = null;
            }

            _unitIsFull = false;
            _unitSpawnPosition = Vector3.zero;
        }

        public void UnitMove(Vector3 touchPos)
        {
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].MoveToTouchPos(touchPos);
            }
        }

        private void UnitReSpawn(Unit u)
        {
            if (isSold) return;
            if (u.GetComponent<Health>().IsDead)
            {
                _deadUnitCount++;
            }

            if (_deadUnitCount < 3) return;
            _unitIsFull = false;
            UnitReSpawnDelay().Forget();
        }

        private async UniTaskVoid UnitReSpawnDelay()
        {
            _deadUnitCount = 0;

            await UniTask.Delay(5000, cancellationToken: _cts.Token);

            if (_unitIsFull) return;
            var unitName = IsUniqueTower ? PoolObjectName.BearJellyUnit : PoolObjectName.PuddingJellyUnit;
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i] = UnitSpawn(unitName, transform.position);
                _units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }
    }
}