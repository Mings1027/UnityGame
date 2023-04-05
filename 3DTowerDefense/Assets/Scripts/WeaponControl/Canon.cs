using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Canon : Projectile
    {
        private MeshFilter _meshFilter;
        private Collider[] _targetColliders;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        private void Awake()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _targetColliders = new Collider[5];
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                var position = transform.position;
                StackObjectPool.Get("CanonEffect", position);
                StackObjectPool.Get("CanonExplosionSFX", position);
                var size = Physics.OverlapSphereNonAlloc(position, atkRange, _targetColliders, enemyLayer);
                for (var i = 0; i < size; i++)
                {
                    GetDamage(_targetColliders[i]);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public void ChangeMesh(MeshFilter meshFilter)
        {
            _meshFilter.sharedMesh = meshFilter.sharedMesh;
        }
    }
}