using GameControl;
using UnityEngine;
using WeaponControl;

namespace EnemyControl
{
    public class ArcherEnemy : RangeEnemy
    {
        protected override void SpawnProjectile(Vector3 t)
        {
            var position = transform.position;
            StackObjectPool.Get("ArrowShootSound", position);
            var p = StackObjectPool.Get<Projectile>("EnemyArrow", position, Quaternion.Euler(-90, 0, 0));
            p.SetPosition(t);
            p.damage = targetFinder.Damage;
        }
    }
}