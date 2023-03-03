using System;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private BarracksUnit[] _units;
        private bool _isSpawned;
        
        [SerializeField] private int unitCount;

        protected override void OnEnable()
        {
            base.OnEnable();
            _units = new BarracksUnit[unitCount];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isSpawned = false;
        }

        public override void Init(int unitHealth, int unitDamage, float attackRange, float attackDelay)
        {
            base.Init(unitHealth, unitDamage, attackRange, attackDelay);
            SpawnUnit(unitCount).Forget();
        }

        public override void SetUp()
        {
        }

        protected override void Targeting()
        {
            if (!_isSpawned) return;
            for (var i = 0; i < unitCount; i++)
            {
                if (!_units[i].gameObject.activeSelf) continue;
                _units[i].IsTargeting = isTargeting;
                _units[i].Target = target;
            }
        }

        private async UniTaskVoid SpawnUnit(int count)
        {
            await UniTask.Delay(1000, cancellationToken: cts.Token);
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            for (var i = 0; i < count; i++)
            {
                _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, transform.position);
                _units[i].onDeadEvent += DeadCount;
                _units[i].Init(damage, atkDelay);
                _units[i].GetComponent<Health>().InitializeHealth(health);
            }

            _isSpawned = true;
        }

        private async UniTaskVoid ReSpawnUnit()
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            for (int i = 0; i < unitCount; i++)
            {
                if (!_units[i].gameObject.activeSelf)
                {
                    await UniTask.Delay(1000, cancellationToken: cts.Token);

                    _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, transform.position);
                    _units[i].onDeadEvent += DeadCount;
                    _units[i].Init(damage, atkDelay);
                    _units[i].GetComponent<Health>().InitializeHealth(health);
                }
            }
        }

        private void DeadCount()
        {
            ReSpawnUnit().Forget();
        }
    }
}