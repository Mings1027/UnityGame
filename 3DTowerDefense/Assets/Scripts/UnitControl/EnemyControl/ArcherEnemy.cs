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
            var p = ObjectPoolManager.Get<ArcherProjectile>(PoolObjectName.EnemyArrow,
                transform.position + new Vector3(0, 2, 0),
                Quaternion.Euler(-90, 0, 0));
            p.Init(t, Damage);
        }
    }
}