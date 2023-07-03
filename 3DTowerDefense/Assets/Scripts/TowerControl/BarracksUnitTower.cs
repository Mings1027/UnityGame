using System;
using DataControl;
using GameControl;
using UnitControl.EnemyControl;
using UnitControl.FriendlyControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class BarracksUnitTower : UnitTower
    {
        private Collider[] _unitCenterPos;
        private Vector3[] _spawnDirections;

        [SerializeField] private LayerMask groundLayer;

        protected override void OnEnable()
        {
            base.OnEnable();
            _unitCenterPos = new Collider[1];
            var position = transform.position;
            _spawnDirections = new[]
            {
                position + Vector3.forward * 10, position + Vector3.back * 10,
                position + Vector3.left * 10, position + Vector3.right * 10
            };
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        //==================================Custom Function====================================================
        //==================================Custom Function====================================================

        protected override void Init()
        {
            base.Init();
            units = new FriendlyUnit[3];
        }

        public void UnitMove(Vector3 touchPos)
        {
            for (var i = 0; i < units.Length; i++)
            {
                var t = units[i];
                t.GoToTouchPosition(touchPos);
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float delay, float health)
        {
            foreach (var u in units)
            {
                if (u != null) u.gameObject.SetActive(false);
            }

            if (unitSpawnPosition == Vector3.zero)
            {
                // Spawn The Tower Once
                // ↑ ↓ ← → Four Direction Check Ground and Unit Spawn 
                foreach (var dir in _spawnDirections)
                {
                    var size = Physics.OverlapSphereNonAlloc(dir, 1, _unitCenterPos, groundLayer);
                    if (size <= 0) continue;
                    unitSpawnPosition = dir;
                    for (var i = 0; i < units.Length; i++)
                    {
                        UnitSpawn(unitSpawnPosition, i);
                        units[i].Init(minDamage, maxDamage, delay, health);
                    }

                    break;
                }
            }
            else
            {
                // Level up after being spawned
                for (var i = 0; i < units.Length; i++)
                {
                    UnitSpawn(unitSpawnPosition, i);
                    units[i].Init(minDamage, maxDamage, delay, health);
                }
            }
        }

        protected override void UnitSpawn(Vector3 pos, int i)
        {
            var unitName = IsUniqueTower ? PoolObjectName.SpearManUnit : PoolObjectName.SwordManUnit;
            var ranPos = pos + Random.insideUnitSphere * 2f;
            units[i] = ObjectPoolManager.Get<BarracksUnit>(unitName, ranPos);
            units[i].OnDeadEvent += ReSpawn;
        }
    }
}