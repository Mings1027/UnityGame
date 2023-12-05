using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using PoolObjectControl;
using StatusControl;
using UIControl;
using UnitControl.TowerUnitControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private CancellationTokenSource _cts;
        private bool _isUnitSpawn;
        private bool _isReSpawning;
        private Vector3 _unitCenterPosition;
        private List<TowerUnit> _units;
        private ReSpawnBar _unitReSpawnBar;
        private Transform _reSpawnBarTransform;

        public int UnitHealth { get; private set; }
        public float UnitReSpawnTime { get; private set; }

        [SerializeField, Range(1, 10)] private byte unitCount;
        [SerializeField, Range(0, 2)] private float unitRadius;
        [SerializeField] private PoolObjectKey unitObjectKey;

        #region Unity Event

        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            if (_isReSpawning)
            {
                _unitReSpawnBar.StopLoading();
                StatusBarUIController.Instance.Remove(_reSpawnBarTransform);
            }

            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                StatusBarUIController.Instance.Remove(_units[i].healthBarTransform);
            }

            for (var i = count - 1; i >= 0; i--)
            {
                if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
                _units[i].DisableParent();
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
                _units[i].Init();
                _units[i].InfoInit(this, pos);
                _units[i].UnitTargetInit();
                _units[i].GetComponent<UnitHealth>().OnDeadEvent += () => DeadEvent(towerUnit);

                var healthBar = PoolObjectManager.Get<HealthBar>(UIPoolObjectKey.UnitHealthBar,
                    _units[i].healthBarTransform.position);
                healthBar.Init(_units[i].GetComponent<Progressive>());

                StatusBarUIController.Instance.Add(healthBar, _units[i].healthBarTransform);
            }

            _isUnitSpawn = true;

            if (!_isReSpawning) return;
            _unitReSpawnBar.StopLoading();
            StatusBarUIController.Instance.Remove(_reSpawnBarTransform);
        }

        private void UnitUpgrade(int damage, float delay)
        {
            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var unitData = (UnitTowerData)TowerData;
                _units[i].UnitUpgrade(damage, unitData.UnitHealth * (1 + TowerLevel), delay);
            }
        }

        private void DeadEvent(TowerUnit unit)
        {
            StatusBarUIController.Instance.Remove(unit.healthBarTransform);

            unit.DisableParent();
            _units.Remove(unit);
            if (_units.Count > 0) return;

            _isUnitSpawn = false;
            if (_isReSpawning) return;
            UnitReSpawnAsync().Forget();
        }

        private async UniTaskVoid UnitReSpawnAsync()
        {
            _isReSpawning = true;
            _unitReSpawnBar =
                PoolObjectManager.Get<ReSpawnBar>(UIPoolObjectKey.ReSpawnBar, _reSpawnBarTransform.position);
            StatusBarUIController.Instance.Add(_unitReSpawnBar, _reSpawnBarTransform);
            _unitReSpawnBar.Init();
            await _unitReSpawnBar.StartLoading(10, _cts);
            StatusBarUIController.Instance.Remove(_reSpawnBarTransform);

            _isReSpawning = false;

            if (_isUnitSpawn) return;
            UnitSpawn();
            UnitUpgrade(Damage, AttackDelay);
        }

        private void ActiveUnitIndicator()
        {
            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
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
            _units = new List<TowerUnit>(unitCount);

            var unitTowerData = (UnitTowerData)TowerData;
            UnitHealth = unitTowerData.UnitHealth;
            UnitReSpawnTime = unitTowerData.UnitReSpawnTime;
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

        public override void TowerTargeting()
        {
            var count = _units.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                _units[i].UnitTargeting();
            }
        }

        public override void TowerUpdate(CancellationTokenSource cts)
        {
            var count = _units.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                _units[i].UnitUpdate(cts);
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, rpmData);

            if (!_isUnitSpawn)
            {
                UnitSpawn();
            }

            UnitUpgrade(damageData, AttackDelay);
        }

        #endregion

        public void OffUnitIndicator()
        {
            var count = _units.Count - 1;
            for (var i = count; i >= 0; i--)
            {
                _units[i].outline.enabled = false;
            }
        }

        public void UnitMove(Vector3 touchPos)
        {
            _unitCenterPosition = touchPos;
            // var tasks = new UniTask[_units.Count];
            var count = _units.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var angle = i * ((float)Math.PI * 2f) / count;
                var pos = touchPos + new Vector3((float)Math.Cos(angle) * unitRadius, 0,
                    (float)Math.Sin(angle) * unitRadius);
                _units[i].Move(pos);
            }

            OffUnitIndicator();
            // await UniTask.WhenAll(tasks);
        }
    }
}