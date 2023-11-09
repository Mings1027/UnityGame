using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private bool _isReSpawning;
        private int _damage;
        private float _atkDelay;
        public Vector3 unitSpawnPosition { get; set; }
        private List<FriendlyUnit> _units;
        private ReSpawnBar _unitReSpawnBar;
        private Transform _reSpawnBarTransform;

        [SerializeField, Range(0, 5)] private byte unitCount;

        #region Unity Event

        private void OnDisable()
        {
            if (_isReSpawning)
            {
                _unitReSpawnBar.StopLoading().Forget();
                ProgressBarUIController.Remove(_reSpawnBarTransform);
            }

            for (int i = _units.Count - 1; i >= 0; i--)
            {
                ProgressBarUIController.Remove(_units[i].healthBarTransform);
            }

            for (var i = _units.Count - 1; i >= 0; i--)
            {
                if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
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
                var position = transform.position;
                var unit = PoolObjectManager.Get<FriendlyUnit>(TowerData.PoolObjectKey, position);
                _units.Add(unit);
                _units[i].transform.DOJump(pos, 2, 1, 0.5f).SetEase(Ease.OutSine);
                _units[i].SpawnInit(this, TowerData.TowerType);
                var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.UnitHealthBar);
                healthBar.Init(_units[i].GetComponent<Progressive>());

                var unitHealth = _units[i].GetComponent<UnitHealth>();
                var unitTowerData = (UnitTowerData)TowerData;
                unitHealth.Init(unitTowerData.UnitHealth);
                unitHealth.OnDeadEvent += () => DeadEvent(unit);
                _units[i].OnDisableEvent += healthBar.RemoveEvent;

                ProgressBarUIController.Add(healthBar, _units[i].healthBarTransform);
            }

            _isUnitSpawn = true;

            if (!_isReSpawning) return;
            _unitReSpawnBar.StopLoading();
            ProgressBarUIController.Remove(_reSpawnBarTransform);
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
            ProgressBarUIController.Remove(unit.healthBarTransform);

            _units.Remove(unit);
            if (_units.Count > 0) return;

            _isUnitSpawn = false;
            if (_isReSpawning) return;
            UnitReSpawnAsync().Forget();
        }

        private async UniTaskVoid UnitReSpawnAsync()
        {
            _isReSpawning = true;
            _unitReSpawnBar = PoolObjectManager.Get<ReSpawnBar>(UIPoolObjectKey.ReSpawnBar);
            ProgressBarUIController.Add(_unitReSpawnBar, _reSpawnBarTransform);
            _unitReSpawnBar.Init();
            await _unitReSpawnBar.StartLoading(5);
            _unitReSpawnBar.gameObject.SetActive(false);
            ProgressBarUIController.Remove(_reSpawnBarTransform);

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
            _reSpawnBarTransform = transform.GetChild(1);
            _units = new List<FriendlyUnit>(unitCount);
        }

        public override void TowerTargetInit()
        {
            for (var i = _units.Count - 1; i >= 0; i--)
            {
                _units[i].UnitTargetInit();
            }
        }

        public override void TowerTargeting()
        {
            for (var i = _units.Count - 1; i >= 0; i--)
            {
                _units[i].UnitTargeting();
            }
        }

        public override async UniTaskVoid TowerAttackAsync(CancellationTokenSource cts)
        {
            await UniTask.Delay(100, cancellationToken: cts.Token);
            for (var i = _units.Count - 1; i >= 0; i--)
            {
                _units[i].UnitAttackAsync(cts).Forget();
                _units[i].UnitAnimation();
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
            for (var i = _units.Count - 1; i >= 0; i--)
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