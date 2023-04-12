using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class CanonBullet : Projectile
    {
        private Collider[] _targetColliders;

        public MeshFilter CanonMeshFilter { get; private set; }

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        private void Awake()
        {
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
            StackObjectPool.Get("CanonEffect", pos);
            StackObjectPool.Get("CanonExplosionSFX", pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                base.ProjectileHit(_targetColliders[i]);
            }
        }
    }
}