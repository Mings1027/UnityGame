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
        private Vector3 _pos;
        private BarracksUnit[] _barracksUnits;

        public int UnitHealth { get; set; }

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
            foreach (var t in _barracksUnits)
            {
                if (t != null && t.gameObject.activeSelf)
                    t.gameObject.SetActive(false);
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float range, float delay)
        {
            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] != null) _barracksUnits[i].gameObject.SetActive(false);

                UnitSpawnAndInit(i, UnitHealth);
                _barracksUnits[i].GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
            }
        }

        private void ReSpawn()
        {
            if (isSold) return;

            foreach (var t in _barracksUnits)
            {
                if (t.GetComponent<Health>().IsDead)
                {
                    deadUnitCount++;
                }
            }

            if (deadUnitCount < 3) return;

            ReSpawnTask().Forget();
        }

        private async UniTaskVoid ReSpawnTask()
        {
            deadUnitCount = 0;
            await UniTask.Delay(5000);

            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                UnitSpawnAndInit(i, UnitHealth);
            }
        }

        private void UnitSpawnAndInit(int i, int health)
        {
            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";
            var ranPos = hit.position + Random.insideUnitSphere * 5f;
            _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, ranPos);
            _barracksUnits[i].GetComponent<Health>().Init(health);
            _barracksUnits[i].onDeadEvent += ReSpawn;
        }
    }
}