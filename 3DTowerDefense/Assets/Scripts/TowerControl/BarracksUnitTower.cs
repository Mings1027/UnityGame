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
            for (var i = 0; i < units.Length; i++)
            {
                if (units[i] != null) units[i].gameObject.SetActive(false);

                UnitSpawn(i);
                units[i].Init(minDamage, maxDamage, delay, health);
            }
        }

        protected override void UnitSpawn(int i)
        {
            if (!NavMesh.SamplePosition(transform.position, out var hit, 15, NavMesh.AllAreas)) return;

            var unitName = IsUniqueTower ? PoolObjectName.SpearManUnit : PoolObjectName.SwordManUnit;
            var ranPos = hit.position + Random.insideUnitSphere * 5f;
            units[i] = ObjectPoolManager.Get<BarracksUnit>(unitName, ranPos);
            units[i].onDeadEvent += ReSpawn;
        }
    }
}