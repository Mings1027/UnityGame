using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnityEngine.AI;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private BarracksUnit[] _units;
        private bool _isUnitSpawned;

        protected override void Awake()
        {
            base.Awake();
            targetCount = 3;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _units = new BarracksUnit[targetCount];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isUnitSpawned = false;
        }

        public override void Init(int unitHealth, int unitDamage, float attackRange, float attackDelay)
        {
            base.Init(unitHealth, unitDamage, attackRange, attackDelay);
            SpawnUnit().Forget();
        }

        protected override void UpdateTarget()
        {
            for (var i = 0; i < targetCount; i++)
            {
                if (targets[i] == null) continue;
                _units[i].IsTargeting = true;
                _units[i].Target = targets[i].transform;
            }
        }

        private async UniTaskVoid SpawnUnit()
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            for (var i = 0; i < targetCount; i++)
            {
                await UniTask.Delay(1000, cancellationToken: cts.Token);
                if (NavMesh.SamplePosition(transform.position, out var hit, 10, NavMesh.AllAreas))
                {
                    if (_units[i] != null && _units[i].gameObject.activeSelf)
                    {print("off");
                        _units[i].gameObject.SetActive(false);
                    }

                    _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                    _units[i].onDeadEvent += DeadCount;
                }

                _units[i].UnitSetup(damage, atkDelay);
                _units[i].GetComponent<Health>().InitializeHealth(health);
            }

            if (_isUnitSpawned) return;
            _isUnitSpawned = true;
        }

        private void DeadCount()
        {
            SpawnUnit().Forget();
        }
    }
}