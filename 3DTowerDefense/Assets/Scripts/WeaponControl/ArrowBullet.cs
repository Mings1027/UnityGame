using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArrowBullet : Projectile
    {
        protected override void FixedUpdate()
        {
            if (level == 3)
            {
                StraightPath();
            }
            else
            {
                ParabolaPath();
            }
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get("ArrowEffect", pos);
            StackObjectPool.Get("ArrowExplosionSFX", pos);
            base.ProjectileHit(col);
        }

        public override void Init(Transform t, int d, int towerLevel)
        {
            base.Init(t, d, towerLevel);
            if (level == 3) projSpeed = 5;
        }
    }
}