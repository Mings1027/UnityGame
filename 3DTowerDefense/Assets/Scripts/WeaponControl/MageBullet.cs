using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Bullet
    {
        protected override void StraightPath()
        {
            var dir = (target.position - rigid.position).normalized;
            rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
            transform.forward = dir;
        }

        protected override void BulletHit(Component other)
        {
            var pos = transform.position;
            StackObjectPool.Get("MageHitSFX",pos);
            StackObjectPool.Get("MageHitVFX", pos);
            base.BulletHit(other);
        }
    }
}