using UnityEngine;

namespace TowerControl
{
    public abstract class TowerUnitAttacker : Tower
    {
        protected abstract void UnitDisable();
        protected abstract void UnitUpgrade(int minDamage, int maxDamage, float range, float delay);

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable();
        }

        public override void ConstructionFinished(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.ConstructionFinished(towerMeshFilter, minDamage, maxDamage, range, delay);
            UnitUpgrade(minDamage, maxDamage, range, delay);
        }
    }
}