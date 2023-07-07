using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class UnitTower : Tower
    {
        private int _deadUnitCount;
        private CancellationTokenSource _cts;
        private Vector3[] _spawnDirections;

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
            var position = transform.position;
            _spawnDirections[0] = position + Vector3.forward * 10;
            _spawnDirections[1] = position + Vector3.back * 10;
            _spawnDirections[2] = position + Vector3.left * 10;
            _spawnDirections[3] = position + Vector3.right * 10;
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
            _spawnDirections = new Vector3[4];
        }

        public override void TowerInit(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay, float health = 0)
        {
            base.TowerInit(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);
            UnitControl(minDamage, maxDamage, attackDelay, health);
        }

        public override void TowerSetting(MeshFilter towerMeshFilter)
        {
            base.TowerSetting(towerMeshFilter);
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }

        private void UnitControl(int minDamage, int maxDamage, float delay, float health)
        {
            if (TowerLevel == 0)
            {
                SpawnUnitOnTowerSpawn();
            }
            else if (IsUniqueTower)
            {
                SpawnUniqueUnit();
            }

            // Upgrade
            UnitInit(minDamage, maxDamage, delay, health);
        }

        private void SpawnUnitOnTowerSpawn()
        {
            // Call Only once when tower spawn
            // ↑ ↓ ← → Four Direction Check Ground and Unit Spawn 
            var position = transform.position;
            foreach (var dir in _spawnDirections)
            {
                var rayDir = dir - position;
                var ray = new Ray(position, rayDir);
                if (!Physics.SphereCast(ray, 1, 10, groundLayer)) continue;

                _unitSpawnPosition = dir;
                for (var i = 0; i < _units.Length; i++)
                {
                    _units[i] = UnitSpawn(PoolObjectName.PuddingJellyUnit, transform.position);
                }

                break;
            }
        }

        private void SpawnUniqueUnit()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].gameObject.SetActive(false);
                var prevUnitPos = _units[i].transform.position;
                _units[i] = UnitSpawn(PoolObjectName.BearJellyUnit, prevUnitPos);
            }
        }

        private FriendlyUnit UnitSpawn(string unitName, Vector3 pos)
        {
            var unit = ObjectPoolManager.Get<BarracksUnit>(unitName, pos);
            unit.OnDeadEvent += UnitReSpawn;
            return unit;
        }

        private void UnitInit(int minDamage, int maxDamage, float delay, float health)
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Init(minDamage, maxDamage, delay, health);
            }
        }

        private void UnitDisable(IList<FriendlyUnit> u)
        {
            for (var i = 0; i < u.Count; i++)
            {
                if (u[i] == null || !u[i].gameObject.activeSelf) continue;
                u[i].gameObject.SetActive(false);
                u[i] = null;
            }

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
            UnitReSpawnDelay().Forget();
        }

        private async UniTaskVoid UnitReSpawnDelay()
        {
            _deadUnitCount = 0;
            await UniTask.Delay(5000, cancellationToken: _cts.Token);
            var unitName = IsUniqueTower ? PoolObjectName.BearJellyUnit : PoolObjectName.PuddingJellyUnit;
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i] = UnitSpawn(unitName, transform.position);
                _units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }
    }
}