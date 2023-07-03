using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherBullet : Bullet
    {
        private float _lerp;
        private Vector3 _startPos;

        protected override void OnEnable()
        {
            base.OnEnable();
            _lerp = 0;
            _startPos = transform.position;
        }

        protected override void AttackPath()
        {
            _lerp += BulletSpeed * Time.fixedDeltaTime;
            rigid.position = Vector3.Lerp(_startPos, target.position + new Vector3(0, 1, 0), _lerp);
        }

        protected override void BulletHit(Component other)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.BulletHitSfx, pos);
            ObjectPoolManager.Get(PoolObjectName.BulletHitVFX, pos);
            base.BulletHit(other);
        }
    }
}