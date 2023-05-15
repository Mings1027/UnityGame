using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameControl;
using UnitControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class UnitTower : Tower
    {
        protected FriendlyUnit[] units;

        private int deadUnitCount;
        public int UnitHealth { get; set; }

        protected override void Awake()
        {
            base.Awake();
            targetColliders = new Collider[3];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable(units);
        }

        private void UnitDisable(IList<FriendlyUnit> u)
        {
            for (var i = 0; i < u.Count; i++)
            {
                if (u[i] == null || !u[i].gameObject.activeSelf) continue;
                u[i].gameObject.SetActive(false);
                u[i] = null;
            }
        }

        protected abstract void UnitUpgrade(int minDamage, int maxDamage, float delay);
        protected abstract void UnitSpawn(int i, int health);

        protected void ReSpawn(Unit u)
        {
            if (isSold) return;
            if (u.GetComponent<Health>().IsDead)
            {
                deadUnitCount++;
            }

            if (deadUnitCount < 3) return;
            ReSpawnTask(units).Forget();
        }

        private async UniTaskVoid ReSpawnTask(IReadOnlyCollection<FriendlyUnit> u)
        {
            deadUnitCount = 0;
            await UniTask.Delay(5000);
            for (var i = 0; i < u.Count; i++)
            {
                UnitSpawn(i, UnitHealth);
            }
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay);
            UnitUpgrade(minDamage, maxDamage, delay);
        }
    }
}