using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Arrow : Projectile
    {
        private Action arrowPathEvent;
        
        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get("ArrowEffect", pos);
            StackObjectPool.Get("ArrowExplosionSFX", pos);
            Damaging(col);
        }
    }
}