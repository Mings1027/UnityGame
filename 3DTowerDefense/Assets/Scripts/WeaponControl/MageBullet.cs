using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MageBullet : Projectile
    {
        protected override void FixedUpdate()
        {
            
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get("MageEffect", pos);
            StackObjectPool.Get("MageExplosionSFX", pos);
            Damaging(col);
        }
    }
}