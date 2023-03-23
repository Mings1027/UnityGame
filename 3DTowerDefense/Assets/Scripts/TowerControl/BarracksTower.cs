using AttackControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;

namespace TowerControl
{
    public class BarracksTower : TowerUnitAttacker
    {
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
                _barracksUnits[i] = null;
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float range, float delay, int health = 0)
        {
            var unitName = TowerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            for (var i = 0; i < _barracksUnits.Length; i++)
            {
                if (_barracksUnits[i] == null)
                {
                    _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, hit.position);
                    _barracksUnits[i].onDeadEvent += ReSpawn;
                }
                else if (TowerLevel == 4)
                {
                    _barracksUnits[i].gameObject.SetActive(false);
                    var pos = _barracksUnits[i].transform.position;
                    _barracksUnits[i] = StackObjectPool.Get<BarracksUnit>(unitName, pos);
                }

                _barracksUnits[i].GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, 5f, delay);
                _barracksUnits[i].GetComponent<Health>().InitializeHealth(health);
            }
        }

        private void ReSpawn()
        {
            if (isSold) return;
            ReSpawnTask();
        }

        private void ReSpawnTask()
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 15, NavMesh.AllAreas))
            {
                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    var t = _barracksUnits[i];
                    if (!t.gameObject.activeSelf && t.GetComponent<Health>().CurHealth <= 0)
                    {
                        // await UniTask.Delay(5000, cancellationToken: cts.Token);
                        DOVirtual.DelayedCall(5, () =>
                        {
                            t.transform.position = hit.position;
                            t.gameObject.SetActive(true);
                        });
                    }
                }
            }
        }
    }
}