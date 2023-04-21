using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class CanonProjectile : Projectile
    {
        private Collider[] _targetColliders;

        public MeshFilter CanonMeshFilter { get; private set; }

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            CanonMeshFilter = GetComponentInChildren<MeshFilter>();
            _targetColliders = new Collider[5];
        }

        protected override void FixedUpdate()
        {
            ParabolaPath();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ProjectileHit(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get("CanonHitVFX", pos);
            StackObjectPool.Get("CanonHitSFX", pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                Damaging(_targetColliders[i]);
            }
        }
    }
}