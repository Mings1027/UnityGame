using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private bool _isLockOnTarget;
        private Vector3 _targetEndPos;
        private Collider[] _targetColliders;
        private AudioSource _explosionAudio;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float atkRange;

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
            _explosionAudio = GetComponentInChildren<AudioSource>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _isLockOnTarget = false;
        }

        protected override void Update()
        {
            if (!_isLockOnTarget && Lerp >= 0.5f)
            {
                _isLockOnTarget = true;
                _targetEndPos = target.transform.position;
            }

            ProjectilePath(Lerp < 0.5f ? target.transform.position : _targetEndPos);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        public override void Hit()
        {
            base.Hit();
            _explosionAudio.Play();
            if (_targetEndPos == Vector3.zero) return;
            var pos = transform.position;

            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, enemyLayer);

            if (size <= 0) return;

            for (var i = 0; i < size; i++)
            {
                TryDamage(_targetColliders[i]);
            }
        }

        public override void Init(int dmg, Collider t)
        {
            base.Init(dmg, t);
            Physics.Raycast(t.bounds.center + t.transform.forward * 2 + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
        }
    }
}