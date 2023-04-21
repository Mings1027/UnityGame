using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherProjectile : Projectile
    {
        private Action arrowPathEvent;
        
        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get("ArrowHitVFX", pos);
            StackObjectPool.Get("ArrowHitSFX", pos);
            Damaging(col);
        }
    }
}