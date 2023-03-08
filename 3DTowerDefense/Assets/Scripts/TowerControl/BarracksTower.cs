using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnityEngine.AI;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private BarracksUnit[] _barracksUnits;
        private bool _isSold;

        protected override void Awake()
        {
            base.Awake();
            _barracksUnits = new BarracksUnit[3];
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

        public override void SetUp(float attackDelay, int unitDamage, int unitHealth)
        {
            base.SetUp(attackDelay, unitDamage, unitHealth);
            UpgradeUnit(unitHealth, unitDamage, attackDelay).Forget();
        }

        private void BarrackUnitSetUp()
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] == null || !_barracksUnits[i].gameObject.activeSelf) continue;
                _barracksUnits[i].gameObject.SetActive(false);
                _barracksUnits[i].onDeadEvent -= ReSpawn;
                _barracksUnits[i] = null;
            }
        }

        private async UniTaskVoid UpgradeUnit(int unitHealth, int unitDamage, float attackDelay)
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    await UniTask.Delay(1000, cancellationToken: cts.Token);

                    if (_barracksUnits[i] != null && towerLevel == 4) //이미 스폰됨 && level = 4
                    {
                        _barracksUnits[i].onDeadEvent -= ReSpawn;
                        _barracksUnits[i].gameObject.SetActive(false);
                        var pos = _barracksUnits[i].transform.position;
                        _barracksUnits[i] = null;
                        _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, pos);
                        _barracksUnits[i].onDeadEvent += ReSpawn;
                    }
                    else if (_barracksUnits[i] == null) //스폰 되기 전
                    {
                        _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                        _barracksUnits[i].onDeadEvent += ReSpawn;
                    }

                    _barracksUnits[i].GetComponent<Health>().InitializeHealth(unitHealth);
                    _barracksUnits[i].UnitSetUp(damage);
                }
            }
        }

        private async UniTaskVoid ReSpawnTask()
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                foreach (var t in _barracksUnits)
                {
                    if (t.gameObject.activeSelf) continue;
                    await UniTask.Delay(5000, cancellationToken: cts.Token);
                    t.transform.position = hit.position;
                    t.gameObject.SetActive(true);
                }
            }
        }

        private void ReSpawn()
        {
            if (_isSold) return;
            ReSpawnTask().Forget();
        }

        protected override void UnitControl()
        {
            
        }
    }
}