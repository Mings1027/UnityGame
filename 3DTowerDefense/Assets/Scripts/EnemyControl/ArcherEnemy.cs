using GameControl;
using UnityEngine;
using WeaponControl;

namespace EnemyControl
{
    public class ArcherEnemy : RangeEnemy
    {
        protected override void SpawnProjectile(Vector3 t)
        {
            var p = StackObjectPool.Get<Projectile>("EnemyProjectile", transform.up, Quaternion.Euler(-90, 0, 0));
            p.SetPosition(transform.position, t);
            p.damage = targetFinder.Damage;
        }
    }
}