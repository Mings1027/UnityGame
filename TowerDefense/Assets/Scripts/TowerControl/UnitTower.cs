using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using PoolObjectControl;
using StatusControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Sequence _reSpawnBarSequence;
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private bool _isReSpawning;
        private int _damage;
        private float _atkDelay;
        public Vector3 unitSpawnPosition { get; set; }
        private List<FriendlyUnit> _units;
        private ReSpawnBar _unitReSpawnBar;

        [SerializeField, Range(0, 5)] private byte unitCount;

        #region Unity Event

        private void OnDestroy()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                if (!_units[i]) continue;
                _units[i].gameObject.SetActive(false);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            if (Input.touchCount > 1) return;
            ActiveUnitIndicator();
        }

        #endregion

        #region Private Function

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
            _unitReSpawnBar.StopReSpawning();
            _reSpawnBarSequence.PlayBackwards();
        }

        private void UnitUpgrade(int damage, float delay)
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var unitData = (UnitTowerData)TowerData;
                _units[i].UnitUpgrade(damage, unitData.UnitHealth * (1 + TowerLevel), delay);
            }
        }

        private void DeadEvent(FriendlyUnit unit)
        {
            _units.Remove(unit);
            if (_units.Count > 0) return;

            _isUnitSpawn = false;
            if (_isReSpawning) return;
            UnitReSpawnAsync().Forget();
        }

        private async UniTaskVoid UnitReSpawnAsync()
        {
            _isReSpawning = true;
            _unitReSpawnBar.enabled = true;
            _reSpawnBarSequence.Restart();
            await _unitReSpawnBar.UpdateBarEvent();
            _reSpawnBarSequence.PlayBackwards();

            _isReSpawning = false;

            if (_isUnitSpawn) return;
            UnitSpawn();
            UnitUpgrade(_damage, _atkDelay);
        }

        private void ActiveUnitIndicator()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].outline.enabled = true;
            }
        }

        #endregion

        #region Override Function

        protected override void Init()
        {
            base.Init();
            _units = new List<FriendlyUnit>(unitCount);
            _unitReSpawnBar = GetComponentInChildren<ReSpawnBar>();
            _reSpawnBarSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_unitReSpawnBar.transform.DOScale(0.02f, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(_unitReSpawnBar.transform.DOLocalMoveY(3, 0.5f).SetEase(Ease.OutBack));
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

        public override async UniTaskVoid TowerAttackAsync(CancellationTokenSource cts)
        {
            await UniTask.Delay(100, cancellationToken: cts.Token);
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].UnitAttackAsync(cts).Forget();
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
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

        #endregion

        public void OffUnitIndicator()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                _units[i].outline.enabled = false;
            }
        }

        public async UniTask StartUnitMove(Vector3 touchPos)
        {
            var tasks = new UniTask[_units.Count];
            for (var i = 0; i < _units.Count; i++)
            {
                var angle = i * ((float)Math.PI * 2f) / _units.Count;
                var pos = touchPos + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));

                tasks[i] = _units[i].MoveToTouchPos(pos);
            }

            OffUnitIndicator();
            await UniTask.WhenAll(tasks);
        }
    }
}