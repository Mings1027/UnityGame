using GameControl;
using UnityEngine;

namespace EnemyControl
{
    public abstract class RangeEnemy : Enemy
    {
        protected abstract void SpawnProjectile(Vector3 t);

        protected override void Attack()
        {
            StackObjectPool.Get("ShootArrowSound", transform.position);
            SpawnProjectile(targetFinder.Target.position);
        }
    }
}