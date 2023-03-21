using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class UnitBullet : Projectile
    {
        private MeshFilter _meshFilter;
        private Collider[] _targetColliders;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;


        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _targetColliders = new Collider[3];
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                Explosion();
                // StackObjectPool.Get("ExplosionSound", transform.position);
                base.OnTriggerEnter(other);
            }
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
                if (_targetColliders[i].TryGetComponent(out Health h))
                {
                    h.GetHit(damage, gameObject).Forget();
                }
            }
        }

        public void ChangeMesh(MeshFilter meshFilter)
        {
            _meshFilter.sharedMesh = meshFilter.sharedMesh;
        }
    }
}