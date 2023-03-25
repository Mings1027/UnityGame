using UnityEngine;

namespace EnemyControl
{
    public abstract class RangeEnemy : Enemy
    {
        protected abstract void SpawnProjectile(Vector3 t);

        protected override void Attack()
        {
            SpawnProjectile(targetFinder.Target.position);
        }
    }
}