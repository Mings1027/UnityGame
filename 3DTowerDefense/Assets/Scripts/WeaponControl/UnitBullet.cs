using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class UnitBullet : Projectile
    {
        private Collider[] _targetColliders;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        private void Start()
        {
            _targetColliders = new Collider[3];
        }

        private void Explosion()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                if (_targetColliders[i].TryGetComponent(out Health h))
                {
                    h.GetHit(damage, gameObject).Forget();
                }
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                Explosion();
                StackObjectPool.Get("Explosion", transform.position);
                base.OnTriggerEnter(other);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
    }
}