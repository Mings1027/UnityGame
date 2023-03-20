using UnityEngine;

namespace TowerControl
{
    public abstract class TowerUnitAttacker : Tower
    {
        protected abstract void UnitSetUp();
        protected abstract void UnitUpgrade(int minDamage, int maxDamage, float range, float delay, int health = 0);

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitSetUp();
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            UnitUpgrade(minDamage, maxDamage, range, delay, health);
        }
    }
}