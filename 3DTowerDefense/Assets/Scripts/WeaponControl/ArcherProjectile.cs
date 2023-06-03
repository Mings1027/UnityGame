using System;
using DataControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class ArcherProjectile : Projectile
    {
        private Action _arrowPathEvent;

        [SerializeField] private string tagName;

        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

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

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.ArrowHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.ArrowHitSfx, pos);
            Damaging(col);
        }
    }
}