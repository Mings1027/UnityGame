using ManagerControl;
using UnityEngine;

namespace EnemyControl
{
    public abstract class RangeEnemy : Enemy
    {
        protected abstract void SpawnProjectile(Vector3 t);

        protected override void Attack()
        {
            SoundManager.PlaySound(SoundManager.Sound.Arrow, transform.position);
            SpawnProjectile(targetFinder.Target.position);
        }
    }
}