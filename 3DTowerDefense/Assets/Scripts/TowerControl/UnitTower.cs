using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnitControl.EnemyControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class UnitTower : Tower
    {
        private int _deadUnitCount;

        protected FriendlyUnit[] units;

        [SerializeField] private int unitsRange;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable(units);
        }


        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        protected abstract void UnitUpgrade(int minDamage, int maxDamage, float delay, float health);
        protected abstract void UnitSpawn(int i);

        private static void UnitDisable(IList<FriendlyUnit> u)
        {
            for (var i = 0; i < u.Count; i++)
            {
                if (u[i] == null || !u[i].gameObject.activeSelf) continue;
                u[i].gameObject.SetActive(false);
                u[i] = null;
            }
        }

        protected void ReSpawn(Unit u)
        {
            if (isSold) return;
            if (u.GetComponent<Health>().IsDead)
            {
                _deadUnitCount++;
            }

            if (_deadUnitCount < 3) return;
            ReSpawnTask(units).Forget();
        }

        private async UniTaskVoid ReSpawnTask(IReadOnlyCollection<FriendlyUnit> u)
        {
            _deadUnitCount = 0;
            await UniTask.Delay(5000);
            for (var i = 0; i < u.Count; i++)
            {
                UnitSpawn(i);
            }
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, float health = 0)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            UnitUpgrade(minDamage, maxDamage, delay, health);
        }
    }
}