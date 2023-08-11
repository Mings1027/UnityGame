using DataControl;
using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private Collider[] _targetColliders;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
        }

        protected override void FixedUpdate()
        {
            ParabolaPath(targetEndPos);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            ObjectPoolManager.Get(PoolObjectName.CanonHitVFX, pos);
            ObjectPoolManager.Get(PoolObjectName.CanonHitSfx, pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                Damaging(_targetColliders[i]);
            }
        }
    }
}