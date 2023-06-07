using System;
using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherProjectile : Projectile
    {
        private Transform _target;
        [SerializeField] private string tagName;

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                ProjectileHit(other);
                gameObject.SetActive(false);
            }
            else if (other.CompareTag("Ground"))
            {
                gameObject.SetActive(false);
            }
        }

        protected override void ParabolaPath()
        {
            var gravity = lerp < 0.5f ? 1f : 1.2f;
            lerp += Time.fixedDeltaTime * gravity * ProjectileSpeed;
            curPos = Vector3.Lerp(startPos, _target.position, lerp);
            curPos.y += ProjectileCurve.Evaluate(lerp);
            var t = rigid.transform;
            var dir = (curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = curPos;
            t.forward = dir;
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.ArrowHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.ArrowHitSfx, pos);
            Damaging(col);
        }

        public void Init(Transform target, int dmg)
        {
            _target = target;
            damage = dmg;
        }
    }
}