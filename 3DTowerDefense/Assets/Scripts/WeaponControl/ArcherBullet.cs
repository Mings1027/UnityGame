using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherBullet : Bullet
    {
        private float lerp;
        private Vector3 startPos;

        protected override void OnEnable()
        {
            base.OnEnable();
            lerp = 0;
            startPos = transform.position;
        }

        protected override void StraightPath()
        {
            lerp += BulletSpeed * Time.fixedDeltaTime;
            rigid.position = Vector3.Lerp(startPos, target.position + new Vector3(0, 1, 0), lerp);
        }

        protected override void BulletHit(Component other)
        {
            var pos = transform.position;
            StackObjectPool.Get(PoolObjectName.BulletHitSfx, pos);
            StackObjectPool.Get(PoolObjectName.BulletHitVFX, pos);
            base.BulletHit(other);
        }
    }
}