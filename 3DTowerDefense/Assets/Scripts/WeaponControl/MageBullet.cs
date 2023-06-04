using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Bullet
    {
        protected override void AttackPath()
        {
            var dir = (target.position - transform.position).normalized;
            // var velocity = dir * BulletSpeed;
            // rigid.position = velocity * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.transform.position + dir * (BulletSpeed * Time.fixedDeltaTime));
            // rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
            // rigid.transform.forward = dir;
        }

        protected override void BulletHit(Component other)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.MageHitVFX, pos);
            base.BulletHit(other);
        }
    }
}