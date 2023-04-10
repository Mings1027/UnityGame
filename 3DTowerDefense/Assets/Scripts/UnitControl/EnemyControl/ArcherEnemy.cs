using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl.EnemyControl
{
    public class ArcherEnemy : RangeEnemy
    {
        protected override void SpawnProjectile(Transform t)
        {
            var position = transform.position;
            StackObjectPool.Get("ArrowShootSFX", position);
            var p = StackObjectPool.Get<Projectile>("EnemyArrow", position, Quaternion.Euler(-90, 0, 0));
            p.Setting(t, Damage);
        }
    }
}