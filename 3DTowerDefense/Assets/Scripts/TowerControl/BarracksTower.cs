using System;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private Vector3 _pos;
        private BarracksUnit[] _barracksUnits;


        protected override void Awake()
        {
            base.Awake();
            _barracksUnits = new BarracksUnit[3];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            BarrackUnitSetUp();
        }

        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        public void MoveUnit(Vector3 pos)
        {
            foreach (var t in _barracksUnits)
            {
                t.movePoint = true;
                t.point = pos;
            }
        }

        public override void Building(bool haveUnit, MeshFilter towerMeshFilter, float delay, float range, int damage,
            int health = 0)
        {
            base.Building(haveUnit, towerMeshFilter, delay, range, damage, health);
            UpgradeUnit(delay, damage, health).Forget();
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

        private async UniTaskVoid UpgradeUnit(float delay, int damage, int health)
        {
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            if (NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas))
            {
                for (var i = 0; i < _barracksUnits.Length; i++)
                {
                    await UniTask.Delay(100, cancellationToken: cts.Token);

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

                    _barracksUnits[i].GetComponent<TargetFinder>().SetUp(delay, damage);
                    _barracksUnits[i].GetComponent<Health>().InitializeHealth(health);
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
            if (isSold) return;
            ReSpawnTask().Forget();
        }
    }
}