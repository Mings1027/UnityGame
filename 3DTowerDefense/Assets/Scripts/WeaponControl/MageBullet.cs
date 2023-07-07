using DataControl;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Bullet
    {
        protected override void AttackPath()
        {
            var dir = (target.position + target.up - transform.position).normalized;
            rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
        }

        protected override void BulletHit(Component other)
        {
            base.BulletHit(other);
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.MageHitVFX, pos);
        }
    }
}