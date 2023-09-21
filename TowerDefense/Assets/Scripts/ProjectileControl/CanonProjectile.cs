using ManagerControl;
using PoolObjectControl;
using SoundControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private bool isLockOnTarget;
        private Vector3 _targetEndPos;
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
            if (!isLockOnTarget && lerp > 0.5f)
            {
                isLockOnTarget = true;
                _targetEndPos = target.position;
            }

            ProjectilePath(lerp < 0.5f ? target.position : _targetEndPos);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (_targetEndPos == Vector3.zero) return;
            PoolObjectManager.Get<SoundPlayer>(PoolObjectKey.CanonExplosion, transform.position).Play();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            isLockOnTarget = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void TryHit()
        {
            if (_targetEndPos == Vector3.zero) return;
            var pos = transform.position;

            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);

            if (size <= 0) return;

            for (var i = 0; i < size; i++)
            {
                TryDamage(_targetColliders[i].transform);
            }
        }

        public override void Init(int dmg, Transform t, TowerType towerType)
        {
            base.Init(dmg, t, towerType);
            Physics.Raycast(t.position + t.forward * 2 + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
        }
    }
}