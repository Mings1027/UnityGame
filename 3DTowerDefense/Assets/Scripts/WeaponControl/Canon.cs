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


        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _targetColliders = new Collider[10];
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            if (!other.CompareTag(tagName)) return;
            Explosion();
            var position = transform.position;
            StackObjectPool.Get("CanonEffect", position);
            StackObjectPool.Get("CanonExplosionSound", position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        private void Explosion()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                GetDamage(_targetColliders[i]);
            }
        }

        public void ChangeMesh(MeshFilter meshFilter)
        {
            _meshFilter.sharedMesh = meshFilter.sharedMesh;
        }
    }
}