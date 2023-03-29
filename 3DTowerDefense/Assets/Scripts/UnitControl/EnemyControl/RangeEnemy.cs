using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class RangeEnemy : EnemyUnit
    {
        protected abstract void SpawnProjectile(Vector3 t);

        protected override void Attack()
        {
            SpawnProjectile(target.position);
        }
    }
}