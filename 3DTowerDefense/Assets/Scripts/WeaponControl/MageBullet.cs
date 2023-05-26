using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Bullet
    {
        protected override void AttackPath()
        {
            var dir = (target.position - rigid.position).normalized;
            rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
            transform.forward = dir;
        }

        protected override void BulletHit(Component other)
        {
            base.BulletHit(other);
            var pos = transform.position;
            StackObjectPool.Get(PoolObjectName.MageHitVFX, pos);
        }
    }
}