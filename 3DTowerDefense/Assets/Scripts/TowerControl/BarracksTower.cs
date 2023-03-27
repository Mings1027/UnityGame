using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksTower : TowerUnitAttacker
    {
        private int deadUnitCount;
        private int unitHealth;
        private Vector3 _pos;
        private BarracksUnit[] _barracksUnits;

        protected override void Awake()
        {
            base.Awake();
            _barracksUnits = new BarracksUnit[3];
        }

        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        public void MoveUnit(Vector3 pos)
        {
            foreach (var t in _barracksUnits)
            {
                t.isMoving = true;
                t.point = pos;
            }
        }

        protected override void UnitSetUp()
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] == null || !_barracksUnits[i].gameObject.activeSelf) continue;
                _barracksUnits[i].gameObject.SetActive(false);
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float range, float delay, int health = 0)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 50, NavMesh.AllAreas))
            {
                var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";
                unitHealth = health;

                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    if (_barracksUnits[i] != null)
                    {
                        _barracksUnits[i].gameObject.SetActive(false);
                    }

                    UnitSpawnAndInit(i, hit.position, unitName);
                    _barracksUnits[i].GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
                }
            }
        }

        private void ReSpawn()
        {
            if (isSold) return;
            deadUnitCount++;
            if (deadUnitCount < 3) return;
            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";
                ReSpawnTask(hit.position, unitName).Forget();
            }
        }

        private async UniTaskVoid ReSpawnTask(Vector3 pos, string unit)
        {
            deadUnitCount = 0;
            await UniTask.Delay(5000);

            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                UnitSpawnAndInit(i, pos, unit);
            }
        }

        private void UnitSpawnAndInit(int i, Vector3 pos, string unit)
        {
            var ranPos = pos + Random.insideUnitSphere * 5f;
            _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unit, ranPos);
            _barracksUnits[i].GetComponent<Health>().Init(unitHealth);
            _barracksUnits[i].onDeadEvent += ReSpawn;
        }
    }
}