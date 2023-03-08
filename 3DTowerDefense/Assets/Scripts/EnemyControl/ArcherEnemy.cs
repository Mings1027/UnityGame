using GameControl;
using UnityEngine;
using WeaponControl;

namespace EnemyControl
{
    public class ArcherEnemy : Enemy
    {
        protected override void Attack()
        {
            SpawnArrow(target.position);
        }

        private void SpawnArrow(Vector3 t)
        {
            var p = StackObjectPool.Get<Projectile>("EnemyProjectile", transform.up, Quaternion.Euler(-90, 0, 0));
            p.Parabola(transform, t).Forget();
            p.damage = damage;
        }
    }
}