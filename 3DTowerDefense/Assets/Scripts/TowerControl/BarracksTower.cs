using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnityEngine.AI;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private BarracksUnit[] _units;
        private bool _firstSpawn;

        protected override void Awake()
        {
            base.Awake();
            targetCount = 3;
            _units = new BarracksUnit[targetCount];
            if (NavMesh.SamplePosition(transform.position, out var hit, 10, NavMesh.AllAreas))
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    _units[i] = StackObjectPool.Get<BarracksUnit>("SwordManUnit", hit.position);
                    _units[i].onDeadEvent += DeadCount;
                    _units[i].GetComponent<Health>().InitializeHealth(health);
                    _units[i].UnitSetup(damage, atkDelay);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            BarrackUnitSetUp();
        }

        public override void Init(int unitHealth, int unitDamage, float attackRange, float attackDelay)
        {
            base.Init(unitHealth, unitDamage, attackRange, attackDelay);
            if (!_firstSpawn) return;
            UpgradeUnit().Forget();
        }

        private void BarrackUnitSetUp()
        {
            foreach (var t in _units)
            {
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        private async UniTaskVoid UpgradeUnit()
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 10, NavMesh.AllAreas))
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    await UniTask.Delay(1000, cancellationToken: cts.Token);

                    if (towerLevel == 4)
                    {
                        _units[i].onDeadEvent -= DeadCount;
                        _units[i].gameObject.SetActive(false);
                        var pos = _units[i].transform.position;
                        _units[i] = null;
                        _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, pos);
                        _units[i].onDeadEvent += DeadCount;
                    }

                    _units[i].GetComponent<Health>().InitializeHealth(health);
                    _units[i].UnitSetup(damage, atkDelay);
                }
            }

            if (!_firstSpawn) return;
            _firstSpawn = true;
        }

        protected override void UpdateTarget()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                if (targets[i] == null || !targets[i].gameObject.activeSelf) continue;
                _units[i].IsTargeting = true;
                _units[i].Target = targets[i].transform;
            }
        }

        private async UniTaskVoid CheckDeadUnit()
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 10, NavMesh.AllAreas))
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    if (!_units[i].gameObject.activeSelf)
                    {
                        await UniTask.Delay(2000, cancellationToken: cts.Token);
                        _units[i] = null;
                        _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                        _units[i].onDeadEvent += DeadCount;
                        _units[i].GetComponent<Health>().InitializeHealth(health);
                    }
                }
            }
        }

        private void DeadCount()
        {
            CheckDeadUnit().Forget();
        }
    }
}