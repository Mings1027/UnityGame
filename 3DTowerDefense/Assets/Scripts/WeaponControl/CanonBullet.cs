using UnityEngine;

namespace WeaponControl
{
    public class CanonBullet : Projectile
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

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ProjectileHit(Collider col)
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, _targetColliders, enemyLayer);
            print(size);
            for (var i = 0; i < size; i++)
            {
                Damage(_targetColliders[i]);
            }
        }

        public override void Init(Transform t, int damage)
        {
            _endPos = t.position + new Vector3(Random.Range(-7, 7), 0);
            _damage = damage;
        }

        public void ChangeMesh(MeshFilter meshFilter)
        {
            _meshFilter.sharedMesh = meshFilter.sharedMesh;
        }
    }
}