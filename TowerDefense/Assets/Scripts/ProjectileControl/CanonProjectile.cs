using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private bool _isLockOnTarget;
        private Vector3 _targetEndPos;
        private Collider[] _targetColliders;
        private AudioSource explosionAudio;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
            explosionAudio = GetComponentInChildren<AudioSource>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _isLockOnTarget = false;
        }

        protected override void FixedUpdate()
        {
            if (!_isLockOnTarget && lerp >= 0.5f)
            {
                _isLockOnTarget = true;
                _targetEndPos = target.position;
            }

            ProjectilePath(lerp < 0.5f ? target.position : _targetEndPos);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            explosionAudio.Play();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public override void Hit()
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

        public override void Init(ushort dmg, Transform t)
        {
            base.Init(dmg, t);
            Physics.Raycast(t.position + t.forward * 2 + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
        }
    }
}