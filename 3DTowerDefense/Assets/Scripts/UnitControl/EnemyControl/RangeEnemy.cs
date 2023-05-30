using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class RangeEnemy : EnemyUnit
    {
        protected abstract void SpawnProjectile(Transform t);

        protected override void Attack()
        {
            SpawnProjectile(Target);
        }
    }
}