using Cysharp.Threading.Tasks;
using EnemyControl;
using GameControl;
using UnitControl;
using UnityEngine.AI;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private BarracksUnit[] _units;
        private bool _isSold;

        protected override void Awake()
        {
            base.Awake();
            targetCount = 3;
            _units = new BarracksUnit[targetCount];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _isSold = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isSold = true;
            BarrackUnitSetUp();
        }

        public override void Init(int unitHealth, int unitDamage, float attackDelay, float attackRange)
        {
            base.Init(unitHealth, unitDamage, attackDelay, attackRange);
            UpgradeUnit().Forget();
        }

        private void BarrackUnitSetUp()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                if (_units[i] != null && _units[i].gameObject.activeSelf)
                {
                    _units[i].gameObject.SetActive(false);
                    _units[i].onDeadEvent -= ReSpawn;
                    _units[i] = null;
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

                    if (_units[i] == null)
                    {
                        _units[i] = StackObjectPool.Get<BarracksUnit>("SwordManUnit", hit.position);
                        _units[i].onDeadEvent += ReSpawn;
                    }
                    else
                    {
                        if (towerLevel == 4)
                        {
                            _units[i].onDeadEvent -= ReSpawn;
                            _units[i].gameObject.SetActive(false);
                            var pos = _units[i].transform.position;
                            _units[i] = null;
                            _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, pos);
                            _units[i].onDeadEvent += ReSpawn;
                        }
                    }

                    _units[i].GetComponent<Health>().InitializeHealth(health);
                    _units[i].UnitSetup(damage, atkDelay);
                }
            }
        }

        protected override void UpdateTarget()
        {
        }

        private async UniTaskVoid ReSpawnTask()
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 10, NavMesh.AllAreas))
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    if (!_units[i].gameObject.activeSelf)
                    {
                        await UniTask.Delay(5000, cancellationToken: cts.Token);
                        _units[i] = null;
                        _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                        _units[i].onDeadEvent += ReSpawn;
                        _units[i].GetComponent<Health>().InitializeHealth(health);
                    }
                }
            }
        }

        private void ReSpawn()
        {
            if (_isSold) return;
            ReSpawnTask().Forget();
        }
    }
}