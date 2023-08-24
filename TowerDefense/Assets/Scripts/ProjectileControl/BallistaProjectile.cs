using DataControl;
using GameControl;
using InterfaceControl;
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
            ObjectPoolManager.Get(PoolObjectName.BallistaHitSfx, pos);
            ApplyDamage(col);
        }
        
        public void Init(Transform t, int dmg)
        {
            target = t;
            damage = dmg;
        }
    }
}