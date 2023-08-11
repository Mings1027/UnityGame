using DataControl;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class ArcherEnemy : RangeEnemy
    {
        protected override void SpawnProjectile(Transform t)
        {
            var p = ObjectPoolManager.Get<BallistaProjectile>(PoolObjectName.EnemyArrow,
                transform.position + new Vector3(0, 2, 0),
                Quaternion.Euler(-90, 0, 0));
            p.Init(t, damage);
        }
    }
}