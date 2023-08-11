using DataControl;
using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public class BallistaBullet : Bullet
    {
        protected override void BulletHit(Component other)
        {
            base.BulletHit(other);
            var pos = transform.position;
            // ObjectPoolManager.Get(PoolObjectName.BulletHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.BulletHitSfx, pos);
        }
    }
}