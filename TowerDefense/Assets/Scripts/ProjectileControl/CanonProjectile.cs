using DataControl;
using GameControl;
using InterfaceControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private Collider[] _targetColliders;

        [SerializeField] private ParticleSystem explosionParticle;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        private void Awake()
        {
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
            explosionParticle.Play();
            ObjectPoolManager.Get(PoolObjectName.CanonHitSfx, pos);
            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);
            for (var i = 0; i < size; i++)
            {
                ApplyDamage(_targetColliders[i]);
            }
        }

        public void Init(Vector3 t, int dmg)
        {
            Physics.Raycast(t + Vector3.up * 2, Vector3.down, out var hit, 10);
            targetEndPos = hit.point;
            targetEndPos.y = 0;
            damage = dmg;
        }
    }
}