using System;
using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherProjectile : Projectile
    {
        private Action _arrowPathEvent;
        
        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get(PoolObjectName.ArrowHitVFX, pos);
            StackObjectPool.Get(PoolObjectName.ArrowHitSfx, pos);
            Damaging(col);
        }
    }
}