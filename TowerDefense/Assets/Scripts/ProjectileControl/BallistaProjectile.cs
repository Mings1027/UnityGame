using DataControl;
using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public class BallistaProjectile : Projectile
    {
        protected override void FixedUpdate()
        {
            ParabolaPath(target.position);
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            // ObjectPoolManager.Get(PoolObjectName.ArrowHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.ArrowHitSfx, pos);
            Damaging(col);
        }
    }
}