using DataControl;
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
            var p = ObjectPoolManager.Get<Projectile>(PoolObjectName.EnemyArrow, position, Quaternion.Euler(-90, 0, 0));
            p.Init(t, Damage);
        }
    }
}