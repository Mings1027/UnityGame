using System;
using System.Collections.Generic;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using PoolObjectControl;
using StatusControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Sequence reSpawnBarSequence;
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private bool _isReSpawning;
        private ushort _damage;
        private float _atkDelay;
        private byte deadUnitCount;
        public Vector3 unitSpawnPosition { get; set; }
        private List<FriendlyUnit> _units;
        private ReSpawnBar unitReSpawnBar;

        [SerializeField, Range(0, 5)] private byte unitCount;
        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        private void OnDestroy()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                if (!_units[i]) continue;
                _units[i].gameObject.SetActive(false);
            }
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        protected override void Init()
        {
            base.Init();
            _units = new List<FriendlyUnit>(unitCount);
            deadUnitCount = 0;
            unitReSpawnBar = GetComponentInChildren<ReSpawnBar>();
            reSpawnBarSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(unitReSpawnBar.transform.DOScale(0.02f, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(unitReSpawnBar.transform.DOLocalMoveY(3, 0.5f).SetEase(Ease.OutBack));
        }

        public override void TowerTargetInit()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].UnitTargetInit();
            }
        }

        public override void TowerTargeting()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].UnitTargeting();
            }
        }

        public override void TowerUpdate()
        {
            for (var i = 0; i < _units.Count; i++)
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
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].Indicator.enabled = true;
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, ushort damageData, byte rangeData,
            float attackDelayData)
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
            _units.Clear();

            for (var i = 0; i < unitCount; i++)
            {
                var angle = i * ((float)Math.PI * 2f) / unitCount;
                var pos = unitSpawnPosition + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
                var unit = PoolObjectManager.Get<FriendlyUnit>(TowerData.PoolObjectKey, transform.position);
                _units.Add(unit);
                _units[i].transform.DOJump(pos, 2, 1, 0.5f).SetEase(Ease.OutSine);
                _units[i].SpawnInit(this, TowerData.TowerType);
                _units[i].TryGetComponent(out Health health);
                health.OnDeadEvent += () => DeadEvent(unit);
            }

            _isUnitSpawn = true;

            if (!_isReSpawning) return;
            unitReSpawnBar.StopReSpawning();
            reSpawnBarSequence.PlayBackwards();
        }

        private void UnitUpgrade(ushort damage, float delay)
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var unitData = (UnitTowerData)TowerData;
                _units[i].UnitUpgrade(damage, unitData.UnitHealth * (1 + TowerLevel), delay);
            }
        }

        public async UniTask StartUnitMove(Vector3 touchPos)
        {
            var tasks = new UniTask[_units.Count];
            for (var i = 0; i < tasks.Length; i++)
            {
                var angle = i * ((float)Math.PI * 2f) / _units.Count;
                var pos = touchPos + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
                tasks[i] = _units[i].MoveToTouchPos(pos);
            }

            await UniTask.WhenAll(tasks);
        }

        public void OffUnitIndicator()
        {
            if (!_isUnitSpawn) return;
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].Indicator.enabled = false;
            }
        }

        private void DeadEvent(FriendlyUnit unit)
        {
            deadUnitCount++;
            _units.Remove(unit);
            if (deadUnitCount != unitCount) return;

            deadUnitCount = 0;
            _isUnitSpawn = false;
            UnitReSpawnAsync().Forget();
        }

        private async UniTaskVoid UnitReSpawnAsync()
        {
            _isReSpawning = true;

            unitReSpawnBar.enabled = false;
            unitReSpawnBar.enabled = true;

            reSpawnBarSequence.Restart();

            await unitReSpawnBar.UpdateBarEvent();

            reSpawnBarSequence.PlayBackwards();

            _isReSpawning = false;

            if (_isUnitSpawn) return;
            UnitSpawn();
            UnitUpgrade(_damage, _atkDelay);
        }
    }
}